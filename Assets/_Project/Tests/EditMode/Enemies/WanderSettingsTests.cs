using System;
using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>WanderSettings: gezgin halkası (GDD §9, §17), ayak hedefi seçimi ve bekleme süresi.</summary>
    public class WanderSettingsTests
    {
        private const float Tolerance = 0.001f;
        private const int Samples = 500;

        private static readonly Vector3 Center = new Vector3(5f, 0f, -3f);

        private static WanderSettings Create(float minLeg = 4f, float maxLeg = 10f)
        {
            return new WanderSettings(Center, 20f, 38f, minLeg, maxLeg, 1f, 3f, 12f, 0.5f, 3f);
        }

        [Test]
        public void Constructor_ClampsInvertedAndNegativeRanges()
        {
            var settings = new WanderSettings(Vector3.zero, 30f, 10f, -2f, -5f, 3f, 1f, -1f, -1f, -1f);

            Assert.AreEqual(30f, settings.MinRadius);
            Assert.AreEqual(30f, settings.MaxRadius, "Dış yarıçap içten küçük olamaz.");
            Assert.AreEqual(0f, settings.MinLegDistance);
            Assert.AreEqual(0f, settings.MaxLegDistance);
            Assert.AreEqual(3f, settings.MaxPause);
            Assert.AreEqual(0f, settings.MaxLegDuration);
            Assert.AreEqual(0f, settings.ArrivalTolerance);
            Assert.AreEqual(0f, settings.NavMeshSampleDistance);
        }

        [Test]
        public void Contains_IsPlanarAndInclusive()
        {
            WanderSettings settings = Create();

            Assert.IsTrue(settings.Contains(Center + new Vector3(20f, 0f, 0f)));
            Assert.IsTrue(settings.Contains(Center + new Vector3(0f, 9f, -38f)), "Yükseklik sayılmaz.");
            Assert.IsFalse(settings.Contains(Center + new Vector3(19.9f, 0f, 0f)));
            Assert.IsFalse(settings.Contains(Center + new Vector3(0f, 0f, 38.1f)));
        }

        [Test]
        public void SampleRing_ExtremesHitInnerAndOuterRadius()
        {
            WanderSettings settings = Create();

            Vector3 inner = settings.SampleRing(() => 0f);
            Vector3 outer = settings.SampleRing(() => 0.99999f);

            Assert.AreEqual(20f, PlanarDistance.Between(Center, inner), Tolerance);
            Assert.AreEqual(Center.x + 20f, inner.x, Tolerance, "Açı 0: +X yönü.");
            Assert.AreEqual(38f, PlanarDistance.Between(Center, outer), 0.01f);
        }

        [Test]
        public void SampleRing_AlwaysInsideRing()
        {
            WanderSettings settings = Create();
            Func<float> random = Seeded(7);

            for (int i = 0; i < Samples; i++)
            {
                Assert.IsTrue(settings.Contains(settings.SampleRing(random)));
            }
        }

        [Test]
        public void PickLegDestination_FromInsideRing_StaysInRingWithinLegDistance()
        {
            WanderSettings settings = Create();
            Func<float> random = Seeded(11);
            Vector3 from = Center + new Vector3(29f, 0f, 0f);

            for (int i = 0; i < Samples; i++)
            {
                Vector3 point = settings.PickLegDestination(from, random);
                float leg = PlanarDistance.Between(from, point);

                Assert.IsTrue(settings.Contains(point), $"{point} halkanın dışında.");
                Assert.That(leg, Is.InRange(4f - Tolerance, 10f + Tolerance));
            }
        }

        [Test]
        public void PickLegDestination_FromBaseCenter_FallsBackToRingPoint()
        {
            WanderSettings settings = Create();
            Func<float> random = Seeded(3);

            // Merkezden 10 m'lik ayak 20 m'lik halkaya ulaşamaz: halkaya geri dönülür.
            for (int i = 0; i < Samples; i++)
            {
                Assert.IsTrue(settings.Contains(settings.PickLegDestination(Center, random)));
            }
        }

        [Test]
        public void PickPause_LerpsBetweenMinAndMax_Clamped()
        {
            WanderSettings settings = Create();

            Assert.AreEqual(1f, settings.PickPause(0f), Tolerance);
            Assert.AreEqual(2f, settings.PickPause(0.5f), Tolerance);
            Assert.AreEqual(3f, settings.PickPause(1f), Tolerance);
            Assert.AreEqual(3f, settings.PickPause(5f), Tolerance);
            Assert.AreEqual(1f, settings.PickPause(-1f), Tolerance);
        }

        private static Func<float> Seeded(int seed)
        {
            var random = new System.Random(seed);
            return () => (float)random.NextDouble();
        }
    }
}
