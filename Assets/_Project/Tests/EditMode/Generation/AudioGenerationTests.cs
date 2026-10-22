using System.Collections.Generic;
using LastHaven.Audio;
using LastHaven.Core;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Ses altyapısının üretilen asset'leri (14.2): mixer grupları, <see cref="AudioConfig"/> bağlantıları, ses kaynağı prefab'ı,
    /// <see cref="AudioEventMap"/>'in tüm yerleri ve tanımlar; Validate'in klipsiz ses kuralı (<see cref="SoundDataValidator"/>).
    /// Asset oluşturmaz (yoksa Inconclusive).
    /// </summary>
    public class AudioGenerationTests
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
        public void Definitions_UniqueNames_AndEveryMusicCue()
        {
            var names = new HashSet<string>();
            foreach (AudioDefinitions.Definition definition in AudioDefinitions.Sfx)
            {
                Assert.IsTrue(names.Add(definition.AssetName), definition.AssetName);
                Assert.AreNotEqual(AudioDefinitions.Slot.Music, definition.Slot, definition.AssetName);
            }

            var cues = new HashSet<MusicCue>();
            foreach (AudioDefinitions.Definition definition in AudioDefinitions.Music)
            {
                Assert.IsTrue(names.Add(definition.AssetName), definition.AssetName);
                Assert.AreEqual(SoundSpatial.TwoD, definition.Spatial, "Müzik 2D.");
                cues.Add(definition.Cue);
            }

            CollectionAssert.AreEquivalent(
                new[] { MusicCue.Day, MusicCue.Night, MusicCue.Boss, MusicCue.GameOver, MusicCue.Victory }, cues);
        }

        [Test]
        public void Apply_WritesPlaybackSettings_ClipsUntouched()
        {
            var sound = ScriptableObject.CreateInstance<SoundData>();
            _created.Add(sound);
            var definition = new AudioDefinitions.Definition(
                "Test", AudioDefinitions.Slot.CoinCollected, SoundSpatial.TwoD, 0.6f, 0.9f, 1.2f, 3, 0.05f);

            AudioGenerator.Apply(sound, definition);

            Assert.AreEqual(SoundSpatial.TwoD, sound.Spatial);
            Assert.AreEqual(0.6f, sound.Volume, 0.0001f);
            Assert.AreEqual(0.9f, sound.MinPitchValue, 0.0001f);
            Assert.AreEqual(1.2f, sound.MaxPitchValue, 0.0001f);
            Assert.AreEqual(3, sound.MaxSimultaneous);
            Assert.AreEqual(0.05f, sound.Cooldown, 0.0001f);
            Assert.IsFalse(sound.HasClips);
        }

        [Test]
        public void SoundDataValidator_NoClipsIsWarningNotError()
        {
            var sound = ScriptableObject.CreateInstance<SoundData>();
            _created.Add(sound);
            var report = new ValidationReport();
            report.BeginSection("Test");

            Assert.IsTrue(SoundDataValidator.Validate(sound, report));
            Assert.AreEqual(0, report.ErrorCount);
            Assert.AreEqual(1, report.WarningCount);

            AudioClip clip = AudioClip.Create("clip", 4410, 1, 44100, false);
            _created.Add(clip);
            var serialized = new SerializedObject(sound);
            SerializedProperty clips = serialized.FindProperty("_clips");
            clips.arraySize = 2;
            clips.GetArrayElementAtIndex(0).objectReferenceValue = clip;
            clips.GetArrayElementAtIndex(1).objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var second = new ValidationReport();
            second.BeginSection("Test");

            Assert.IsFalse(SoundDataValidator.Validate(sound, second), "Klibi var: çalar.");
            Assert.AreEqual(0, second.ErrorCount);
            Assert.AreEqual(1, second.WarningCount, "Boş eleman uyarısı.");
        }

        [Test]
        public void MixerAsset_HasMusicAndSfxGroups_BoundToConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(AudioGenerator.ConfigPath);
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(AudioGenerator.MixerPath);
            if (config == null || mixer == null)
            {
                Assert.Inconclusive("Ses asset'leri yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            AudioMixerGroup music = AudioMixerFactory.FindGroup(AudioGenerator.MixerPath, AudioMixerFactory.MusicGroupName);
            AudioMixerGroup sfx = AudioMixerFactory.FindGroup(AudioGenerator.MixerPath, AudioMixerFactory.SfxGroupName);
            Assert.IsNotNull(AudioMixerFactory.FindGroup(AudioGenerator.MixerPath, AudioMixerFactory.MasterGroupName));
            Assert.IsNotNull(music);
            Assert.IsNotNull(sfx);
            Assert.AreSame(music, config.MusicGroup);
            Assert.AreSame(sfx, config.SfxGroup);
            Assert.AreEqual(1, mixer.FindMatchingGroups($"{AudioMixerFactory.MasterGroupName}/{AudioMixerFactory.SfxGroupName}").Length,
                "SFX Master'ın çocuğu.");
            Assert.GreaterOrEqual(AudioMixerFactory.GetViewCount(mixer), 1, "Audio Mixer penceresinde görünüm var.");
        }

        [Test]
        public void VoicePrefab_HasSourceAndIsBound()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(AudioGenerator.ConfigPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SoundVoicePrefab.Path);
            if (config == null || prefab == null)
            {
                Assert.Inconclusive("Ses asset'leri yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.IsTrue(prefab.TryGetComponent(out SoundVoice voice));
            Assert.IsNotNull(voice.Source);
            Assert.IsFalse(voice.Source.playOnAwake);
            Assert.AreSame(voice, config.VoicePrefab);
        }

        [Test]
        public void EventMapAsset_EveryEventHasSound()
        {
            var map = AssetDatabase.LoadAssetAtPath<AudioEventMap>(AudioGenerator.EventMapPath);
            if (map == null)
            {
                Assert.Inconclusive($"{AudioGenerator.EventMapPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            foreach (string weaponId in new[] { WeaponsGenerator.PistolId, WeaponsGenerator.ShotgunId, WeaponsGenerator.RifleId, AudioDefinitions.TurretWeaponId })
            {
                Assert.IsNotNull(map.GetWeaponFired(weaponId), weaponId);
                Assert.AreNotSame(map.WeaponFiredFallback, map.GetWeaponFired(weaponId), $"{weaponId} kendi sesine sahip.");
            }

            Assert.IsNotNull(map.WeaponFiredFallback);
            Assert.IsNotNull(map.ShotHitFlesh);
            Assert.IsNotNull(map.ShotHitSurface);
            Assert.IsNotNull(map.EnemyKilled);
            Assert.IsNotNull(map.BossSpawned);
            Assert.IsNotNull(map.CoinCollected);
            Assert.IsNotNull(map.PurchaseCompleted);
            Assert.IsNotNull(map.StructurePlaced);
            Assert.IsNotNull(map.StructureDestroyed);
            Assert.IsNotNull(map.PowerLost);
            Assert.IsNotNull(map.DayEndingSoon);
            Assert.IsNotNull(map.LevelUp);
            Assert.IsNotNull(map.UiClick);
            foreach (GamePhase phase in new[] { GamePhase.Day, GamePhase.Dusk, GamePhase.Night, GamePhase.Dawn })
            {
                Assert.IsNotNull(map.GetPhaseChanged(phase), phase.ToString());
            }

            foreach (MusicCue cue in new[] { MusicCue.Day, MusicCue.Night, MusicCue.Boss, MusicCue.GameOver, MusicCue.Victory })
            {
                Assert.IsTrue(map.TryGetMusic(cue, out MusicTrackEntry track), cue.ToString());
                Assert.IsNotNull(track.Sound, cue.ToString());
            }
        }
    }
}
