using LastHaven.Core;
using LastHaven.DebugTools;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Debug paneli (15.1) saf parçaları: FPS ölçer ve ölümsüzlük filtresi.</summary>
    public class DebugToolsTests
    {
        private sealed class AllowAll : IDamageFilter
        {
            public bool CanTakeDamage(in DamageInfo damage) => true;
        }

        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                Object.DestroyImmediate(_host);
            }
        }

        // --- FrameRateCounter ---

        [Test]
        public void FrameRate_ReportsAverageWhenWindowFills()
        {
            var counter = new FrameRateCounter(0.5f);

            Assert.IsFalse(counter.Tick(0.125f));
            Assert.IsFalse(counter.Tick(0.125f));
            Assert.IsFalse(counter.Tick(0.125f));
            Assert.IsFalse(counter.HasSample);
            Assert.AreEqual(0f, counter.Fps);

            Assert.IsTrue(counter.Tick(0.125f));
            Assert.IsTrue(counter.HasSample);
            Assert.AreEqual(8f, counter.Fps, 0.0001f);
        }

        [Test]
        public void FrameRate_NewWindowStartsFresh()
        {
            var counter = new FrameRateCounter(0.5f);
            counter.Tick(0.5f);
            Assert.AreEqual(2f, counter.Fps, 0.0001f);

            Assert.IsFalse(counter.Tick(0.25f));
            Assert.IsTrue(counter.Tick(0.25f));
            Assert.AreEqual(4f, counter.Fps, 0.0001f);
        }

        [Test]
        public void FrameRate_IgnoresNonPositiveFrames_AndResets()
        {
            var counter = new FrameRateCounter(0.5f);
            Assert.IsFalse(counter.Tick(0f));
            Assert.IsFalse(counter.Tick(-1f));
            Assert.IsFalse(counter.Tick(float.NaN));
            counter.Tick(0.5f);
            Assert.AreEqual(2f, counter.Fps, 0.0001f, "Geçersiz kareler sayılmadı.");

            counter.Reset();
            Assert.IsFalse(counter.HasSample);
            Assert.AreEqual(0f, counter.Fps);
        }

        // --- DamageImmunity ---

        [Test]
        public void Immunity_BlocksDamage_RemoveRestoresPreviousFilter()
        {
            Health health = CreateHealth();
            var previous = new AllowAll();
            health.DamageFilter = previous;
            var immunity = new DamageImmunity();

            immunity.Apply(health);
            Assert.IsTrue(immunity.IsApplied);
            Assert.AreSame(immunity, health.DamageFilter);
            health.TakeDamage(Damage(30f));
            Assert.AreEqual(100f, health.Current);

            immunity.Remove();
            Assert.IsFalse(immunity.IsApplied);
            Assert.AreSame(previous, health.DamageFilter);
            health.TakeDamage(Damage(30f));
            Assert.AreEqual(70f, health.Current);
        }

        [Test]
        public void Immunity_Remove_KeepsFilterReplacedMeanwhile()
        {
            Health health = CreateHealth();
            var immunity = new DamageImmunity();
            immunity.Apply(health);
            var other = new AllowAll();
            health.DamageFilter = other;

            immunity.Remove();

            Assert.AreSame(other, health.DamageFilter);
        }

        [Test]
        public void Immunity_ApplyToAnother_MovesIt_AndNullIsIgnored()
        {
            Health first = CreateHealth();
            Health second = CreateHealth();
            var immunity = new DamageImmunity();

            immunity.Apply(first);
            immunity.Apply(second);
            Assert.IsNull(first.DamageFilter, "İlkinin eski (boş) filtresi geri kondu.");
            Assert.AreSame(immunity, second.DamageFilter);

            immunity.Apply(null);
            Assert.AreSame(immunity, second.DamageFilter);
        }

        private Health CreateHealth()
        {
            if (_host == null)
            {
                _host = new GameObject("DebugToolsTest");
            }

            var child = new GameObject("Target");
            child.transform.SetParent(_host.transform);
            return child.AddComponent<Health>();
        }

        private static DamageInfo Damage(float amount)
        {
            return new DamageInfo(amount, DamageType.Bullet, null, Vector3.zero, Vector3.zero);
        }
    }
}
