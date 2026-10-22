using System.Collections.Generic;
using LastHaven.Audio;
using LastHaven.Core;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Olay → ses eşlemesi (14.2): <see cref="AudioEventMap"/> aramaları, üreticinin "boşsa doldur" kuralı
    /// (<see cref="AudioEventMapFiller"/>) ve arayüz tıklama süzgeci (<see cref="UiClickFilter"/>). Asset oluşturmaz.
    /// </summary>
    public class AudioEventMapTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [Test]
        public void GetWeaponFired_ById_ElseFallback()
        {
            AudioEventMap map = CreateMap();
            SoundData pistol = CreateSound("pistol");
            SoundData fallback = CreateSound("fallback");
            var serialized = new SerializedObject(map);
            AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.WeaponFired, WeaponsGenerator.PistolId), pistol);
            AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.WeaponFiredFallback), fallback);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreSame(pistol, map.GetWeaponFired(WeaponsGenerator.PistolId));
            Assert.AreSame(fallback, map.GetWeaponFired("turret"));
            Assert.AreSame(fallback, map.GetWeaponFired("PISTOL"), "Id ordinal (büyük/küçük harf duyarlı).");
            Assert.AreSame(fallback, map.GetWeaponFired(null));
        }

        [TestCase(HitTargetType.Enemy, true)]
        [TestCase(HitTargetType.Player, true)]
        [TestCase(HitTargetType.Structure, false)]
        [TestCase(HitTargetType.Surface, false)]
        public void GetShotHit_FleshOrSurface(HitTargetType type, bool flesh)
        {
            AudioEventMap map = CreateMap();
            SoundData fleshSound = CreateSound("flesh");
            SoundData surfaceSound = CreateSound("surface");
            var serialized = new SerializedObject(map);
            AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.ShotHitFlesh), fleshSound);
            AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.ShotHitSurface), surfaceSound);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreSame(flesh ? fleshSound : surfaceSound, map.GetShotHit(type));
        }

        [Test]
        public void PhaseAndMusic_Lookups()
        {
            AudioEventMap map = CreateMap();
            SoundData night = CreateSound("night");
            SoundData boss = CreateSound("boss");
            var serialized = new SerializedObject(map);
            AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.PhaseChanged).ForPhase(GamePhase.Night), night);
            AudioEventMapFiller.Fill(serialized, Music(MusicCue.Boss, loop: true), boss);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreSame(night, map.GetPhaseChanged(GamePhase.Night));
            Assert.IsNull(map.GetPhaseChanged(GamePhase.Day));
            Assert.IsTrue(map.TryGetMusic(MusicCue.Boss, out MusicTrackEntry track));
            Assert.AreSame(boss, track.Sound);
            Assert.IsTrue(track.Loop);
            Assert.IsFalse(map.TryGetMusic(MusicCue.Victory, out _));
        }

        [Test]
        public void Filler_FillsOnlyEmpty_AndAddsMissingRowsOnce()
        {
            AudioEventMap map = CreateMap();
            SoundData generated = CreateSound("generated");
            SoundData userChoice = CreateSound("user");
            var serialized = new SerializedObject(map);
            serialized.FindProperty("_enemyKilled").objectReferenceValue = userChoice;
            AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.WeaponFired, WeaponsGenerator.RifleId), userChoice);

            Assert.IsFalse(AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.EnemyKilled), generated),
                "Dolu alana dokunulmaz.");
            Assert.IsFalse(AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.WeaponFired, WeaponsGenerator.RifleId), generated),
                "Var olan satırın dolu sesine dokunulmaz.");
            Assert.IsTrue(AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.UiClick), generated));
            Assert.IsFalse(AudioEventMapFiller.Fill(serialized, Definition(AudioDefinitions.Slot.UiClick), generated), "İkinci çalıştırma değişiklik yapmaz.");
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreSame(userChoice, map.EnemyKilled);
            Assert.AreSame(userChoice, map.GetWeaponFired(WeaponsGenerator.RifleId));
            Assert.AreEqual(1, map.WeaponFired.Count);
            Assert.AreSame(generated, map.UiClick);
        }

        [Test]
        public void Filler_EmptyRowSound_IsFilled()
        {
            AudioEventMap map = CreateMap();
            SoundData sound = CreateSound("sound");
            var serialized = new SerializedObject(map);
            SerializedProperty music = serialized.FindProperty("_music");
            music.arraySize = 1;
            music.GetArrayElementAtIndex(0).FindPropertyRelative("_cue").intValue = (int)MusicCue.Day;

            Assert.IsTrue(AudioEventMapFiller.Fill(serialized, Music(MusicCue.Day, loop: true), sound));
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1, map.Music.Count);
            Assert.AreSame(sound, map.Music[0].Sound);
        }

        [Test]
        public void UiClickFilter_ButtonOrItsChild_WhenEnabled()
        {
            var button = new Button();
            var label = new Label("Satın Al");
            button.Add(label);
            var plain = new VisualElement();

            Assert.AreSame(button, UiClickFilter.FindButton(label));
            Assert.IsTrue(UiClickFilter.IsClickable(label));
            Assert.IsTrue(UiClickFilter.IsClickable(button));
            Assert.IsFalse(UiClickFilter.IsClickable(plain));
            Assert.IsFalse(UiClickFilter.IsClickable(null));

            button.SetEnabled(false);
            Assert.IsFalse(UiClickFilter.IsClickable(label), "Devre dışı buton ses çıkarmaz.");
        }

        private AudioEventMap CreateMap()
        {
            var map = ScriptableObject.CreateInstance<AudioEventMap>();
            _created.Add(map);
            return map;
        }

        private SoundData CreateSound(string name)
        {
            var sound = ScriptableObject.CreateInstance<SoundData>();
            sound.name = name;
            _created.Add(sound);
            return sound;
        }

        private static AudioDefinitions.Definition Definition(AudioDefinitions.Slot slot, string weaponId = null)
        {
            var definition = new AudioDefinitions.Definition("Test", slot, SoundSpatial.TwoD, 1f, 1f, 1f, 1, 0f);
            return weaponId != null ? definition.ForWeapon(weaponId) : definition;
        }

        private static AudioDefinitions.Definition Music(MusicCue cue, bool loop)
        {
            return new AudioDefinitions.Definition("Test", AudioDefinitions.Slot.Music, SoundSpatial.TwoD, 1f, 1f, 1f, 1, 0f)
                .ForMusic(cue, loop);
        }
    }
}
