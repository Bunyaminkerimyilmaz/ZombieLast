using System.Collections.Generic;
using LastHaven.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// <see cref="SoundData"/> (14.2): rastgele klip seçimi (boş elemanlar atlanır), perde aralığı, mesafe ve süre kuralları.
    /// Asset oluşturmaz (bellek içi SO ve <see cref="AudioClip.Create(string,int,int,int,bool)"/>).
    /// </summary>
    public class SoundDataTests
    {
        private const float Tolerance = 0.0001f;

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
        public void NoClips_IsSilent()
        {
            SoundData sound = CreateSound();

            Assert.IsFalse(sound.HasClips);
            Assert.AreEqual(0, sound.CountClips());
            Assert.IsNull(sound.PickClip(0.5f));
        }

        [Test]
        public void OnlyNullClips_IsSilent()
        {
            SoundData sound = CreateSound(null, null);

            Assert.IsFalse(sound.HasClips);
            Assert.IsNull(sound.PickClip(0f));
        }

        [Test]
        public void PickClip_SkipsNullsAndCoversRange()
        {
            AudioClip a = CreateClip("a");
            AudioClip b = CreateClip("b");
            SoundData sound = CreateSound(null, a, null, b);

            Assert.AreEqual(2, sound.CountClips());
            Assert.AreSame(a, sound.PickClip(0f));
            Assert.AreSame(a, sound.PickClip(0.49f));
            Assert.AreSame(b, sound.PickClip(0.5f));
            Assert.AreSame(b, sound.PickClip(1f), "1 son klibi verir (taşma yok).");
            Assert.AreSame(a, sound.PickClip(-3f), "Aralık dışı değer kırpılır.");
        }

        [Test]
        public void PickPitch_LerpsWithinRange_EvenIfSwapped()
        {
            SoundData sound = CreateSound();
            Set(sound, "_minPitch", 1.2f);
            Set(sound, "_maxPitch", 0.8f);

            Assert.AreEqual(0.8f, sound.MinPitchValue, Tolerance);
            Assert.AreEqual(1.2f, sound.MaxPitchValue, Tolerance);
            Assert.AreEqual(0.8f, sound.PickPitch(0f), Tolerance);
            Assert.AreEqual(1f, sound.PickPitch(0.5f), Tolerance);
            Assert.AreEqual(1.2f, sound.PickPitch(1f), Tolerance);
        }

        [Test]
        public void MaxDistance_NeverBelowMinDistance()
        {
            SoundData sound = CreateSound();
            Set(sound, "_minDistance", 10f);
            Set(sound, "_maxDistance", 4f);

            Assert.AreEqual(10f, sound.MinDistance, Tolerance);
            Assert.AreEqual(10f, sound.MaxDistance, Tolerance);
        }

        [Test]
        public void GetPlayDuration_DividesByPitch()
        {
            Assert.AreEqual(2f, SoundData.GetPlayDuration(2f, 1f), Tolerance);
            Assert.AreEqual(1f, SoundData.GetPlayDuration(2f, 2f), Tolerance);
            Assert.AreEqual(4f, SoundData.GetPlayDuration(2f, 0.5f), Tolerance);
            Assert.AreEqual(2f / SoundData.MinPitch, SoundData.GetPlayDuration(2f, 0f), Tolerance, "Perde 0 sonsuz süre vermez.");
            Assert.AreEqual(0f, SoundData.GetPlayDuration(null, 1f), Tolerance);
        }

        [Test]
        public void Defaults_AreThreeDimensionalAndAudible()
        {
            SoundData sound = CreateSound();

            Assert.AreEqual(SoundSpatial.ThreeD, sound.Spatial);
            Assert.IsTrue(sound.IsSpatial);
            Assert.AreEqual(1f, sound.Volume, Tolerance);
            Assert.AreEqual(1f, sound.PickPitch(0.7f), Tolerance);
        }

        private SoundData CreateSound(params AudioClip[] clips)
        {
            var sound = ScriptableObject.CreateInstance<SoundData>();
            _created.Add(sound);
            var serialized = new SerializedObject(sound);
            SerializedProperty list = serialized.FindProperty("_clips");
            list.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return sound;
        }

        private AudioClip CreateClip(string name)
        {
            AudioClip clip = AudioClip.Create(name, 4410, 1, 44100, false);
            _created.Add(clip);
            return clip;
        }

        private static void Set(Object target, string field, float value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
