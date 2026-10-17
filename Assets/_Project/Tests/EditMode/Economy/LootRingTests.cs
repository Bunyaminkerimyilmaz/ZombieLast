using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Tests.EditMode
{
    /// <summary>LootRing (GDD §9, 11.1): noktalar 22–38 m halkasında, alana göre eşit dağılır; aralık kontrolü dahil.</summary>
    public class LootRingTests
    {
        private const float Tolerance = 0.001f;

        private readonly LootRing _ring = new LootRing(22f, 38f);
        private readonly Vector3 _center = new Vector3(5f, 0.2f, -3f);

        [Test]
        public void Sample_InsideRing_AtCenterHeight()
        {
            var random = new Random(9);

            for (int i = 0; i < 1000; i++)
            {
                Vector3 point = _ring.Sample(_center, random);

                Assert.That(PlanarDistance.Between(_center, point), Is.InRange(22f - Tolerance, 38f + Tolerance));
                Assert.AreEqual(_center.y, point.y, Tolerance);
            }
        }

        [Test]
        public void Sample_UniformByArea_HalfBeyondEqualAreaRadius()
        {
            // Halkayı eşit alanlı iki parçaya bölen yarıçap: sqrt((22² + 38²) / 2) ≈ 31.05 m.
            float split = Mathf.Sqrt((22f * 22f + 38f * 38f) * 0.5f);
            var random = new Random(11);
            const int samples = 4000;
            int outer = 0;

            for (int i = 0; i < samples; i++)
            {
                if (PlanarDistance.Between(_center, _ring.Sample(_center, random)) > split)
                {
                    outer++;
                }
            }

            Assert.That(outer / (float)samples, Is.InRange(0.46f, 0.54f));
        }

        [Test]
        public void Sample_CoversAllDirections()
        {
            var random = new Random(13);
            var quadrants = new bool[4];

            for (int i = 0; i < 200; i++)
            {
                Vector3 offset = _ring.Sample(_center, random) - _center;
                quadrants[(offset.x >= 0f ? 0 : 1) + (offset.z >= 0f ? 0 : 2)] = true;
            }

            CollectionAssert.AreEqual(new[] { true, true, true, true }, quadrants);
        }

        [TestCase(21.99f, false)]
        [TestCase(22f, true)]
        [TestCase(30f, true)]
        [TestCase(38f, true)]
        [TestCase(38.01f, false)]
        public void Contains_BoundsInclusive_IgnoresHeight(float distance, bool expected)
        {
            Vector3 point = _center + new Vector3(0f, 7f, distance);

            Assert.AreEqual(expected, _ring.Contains(_center, point));
        }

        [Test]
        public void HasSpacing_ChecksEveryOther_Inclusive()
        {
            var others = new List<Vector3> { new Vector3(0f, 0f, 0f), new Vector3(20f, 0f, 0f) };

            Assert.IsTrue(LootRing.HasSpacing(new Vector3(10f, 0f, 0f), others, 6f));
            Assert.IsTrue(LootRing.HasSpacing(new Vector3(6f, 3f, 0f), others, 6f), "Tam sınır geçerli; yükseklik sayılmaz.");
            Assert.IsFalse(LootRing.HasSpacing(new Vector3(15f, 0f, 0f), others, 6f), "İkinciye 5 m.");
            Assert.IsTrue(LootRing.HasSpacing(Vector3.zero, new List<Vector3>(), 6f), "Başka sandık yok.");
        }

        [Test]
        public void Constructor_SanitizesRadii()
        {
            var ring = new LootRing(-5f, -10f);

            Assert.AreEqual(0f, ring.MinRadius);
            Assert.AreEqual(0f, ring.MaxRadius);

            var inverted = new LootRing(30f, 20f);
            Assert.AreEqual(30f, inverted.MaxRadius, "En büyük en küçükten küçük olamaz.");
        }
    }
}
