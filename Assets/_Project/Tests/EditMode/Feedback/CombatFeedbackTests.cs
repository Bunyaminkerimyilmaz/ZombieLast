using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;
using LastHaven.Feedback;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Atış efektlerinin saf kuralları: iz rengi, iz çizme koşulu, vuruş türü.</summary>
    public class CombatFeedbackTests
    {
        private static readonly Color DefaultColor = Color.white;

        private readonly TestWeapons _weapons = new TestWeapons();

        [TearDown]
        public void TearDown()
        {
            _weapons.DestroyAll();
        }

        [Test]
        public void TracerPalette_ReturnsWeaponColorById()
        {
            WeaponData pistol = CreateWeapon("pistol", Color.yellow);
            WeaponData rifle = CreateWeapon("rifle", Color.cyan);
            var palette = new TracerPalette(new List<WeaponData> { pistol, rifle }, DefaultColor);

            Assert.AreEqual(Color.yellow, palette.Get("pistol"));
            Assert.AreEqual(Color.cyan, palette.Get("rifle"));
        }

        [Test]
        public void TracerPalette_UnknownOrNullId_ReturnsDefault()
        {
            var palette = new TracerPalette(new List<WeaponData> { CreateWeapon("pistol", Color.yellow) }, DefaultColor);

            Assert.AreEqual(DefaultColor, palette.Get("shotgun"));
            Assert.AreEqual(DefaultColor, palette.Get(null));
            Assert.AreEqual(DefaultColor, palette.Get("PISTOL"), "Id'ler ordinal (büyük/küçük harf duyarlı).");
        }

        [Test]
        public void TracerPalette_SkipsNullWeapons_AndKeepsFirstDuplicate()
        {
            WeaponData first = CreateWeapon("pistol", Color.yellow);
            WeaponData duplicate = CreateWeapon("pistol", Color.red);
            var palette = new TracerPalette(new List<WeaponData> { null, first, duplicate }, DefaultColor);

            Assert.AreEqual(Color.yellow, palette.Get("pistol"));
        }

        [Test]
        public void TracerPalette_NullList_ReturnsDefault()
        {
            var palette = new TracerPalette(null, DefaultColor);

            Assert.AreEqual(DefaultColor, palette.Get("pistol"));
        }

        [Test]
        public void ShouldDrawTracer_ForwardSegmentLongerThanMin()
        {
            Assert.IsTrue(FeedbackDirector.ShouldDrawTracer(Vector3.zero, Vector3.forward * 5f, Vector3.forward, 0.3f));
        }

        [Test]
        public void ShouldDrawTracer_ShortOrBackwardSegment_IsSkipped()
        {
            Assert.IsFalse(FeedbackDirector.ShouldDrawTracer(Vector3.zero, Vector3.forward * 0.1f, Vector3.forward, 0.3f), "Kısa");
            Assert.IsFalse(FeedbackDirector.ShouldDrawTracer(Vector3.zero, Vector3.back * 0.5f, Vector3.forward, 0.3f), "Namlunun gerisi");
        }

        [Test]
        public void ImpactEffect_FleshForEnemyAndPlayer_SparkOtherwise()
        {
            Assert.IsTrue(ImpactEffect.IsFlesh(HitTargetType.Enemy));
            Assert.IsTrue(ImpactEffect.IsFlesh(HitTargetType.Player));
            Assert.IsFalse(ImpactEffect.IsFlesh(HitTargetType.Surface));
            Assert.IsFalse(ImpactEffect.IsFlesh(HitTargetType.Structure));
        }

        private WeaponData CreateWeapon(string id, Color tracerColor)
        {
            WeaponData weapon = _weapons.Create(id: id);
            var serialized = new SerializedObject(weapon);
            serialized.FindProperty("_tracerColor").colorValue = tracerColor;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return weapon;
        }
    }
}
