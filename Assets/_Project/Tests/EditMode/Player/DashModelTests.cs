using System;
using LastHaven.Player;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>DashModel: GDD §5 atılma (5 m / 0.18 sn, bekleme 2 sn).</summary>
    public class DashModelTests
    {
        private const float Distance = 5f;
        private const float Duration = 0.18f;
        private const float Cooldown = 2f;
        private const float Tolerance = 0.0001f;

        private DashModel _dash;

        [SetUp]
        public void SetUp()
        {
            _dash = new DashModel();
        }

        [Test]
        public void NewModel_IsReadyAndNotDashing()
        {
            Assert.IsTrue(_dash.IsReady);
            Assert.IsFalse(_dash.IsDashing);
            Assert.AreEqual(0f, _dash.CooldownRemaining);
        }

        [Test]
        public void TryStart_StartsDashWithFlatNormalizedDirection()
        {
            Assert.IsTrue(_dash.TryStart(new Vector3(0f, 3f, 2f), Distance, Duration, Cooldown));

            Assert.IsTrue(_dash.IsDashing);
            Assert.IsFalse(_dash.IsReady);
            Assert.AreEqual(Vector3.forward, _dash.Direction);
            Assert.AreEqual(Cooldown, _dash.CooldownRemaining, Tolerance);
        }

        [Test]
        public void TryStart_ZeroDirection_DoesNotStart()
        {
            Assert.IsFalse(_dash.TryStart(Vector3.up, Distance, Duration, Cooldown));
            Assert.IsFalse(_dash.IsDashing);
            Assert.IsTrue(_dash.IsReady);
        }

        [Test]
        public void TryStart_NonPositiveDuration_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _dash.TryStart(Vector3.forward, Distance, 0f, Cooldown));
        }

        [Test]
        public void Advance_CoversExactDistanceOverDuration()
        {
            _dash.TryStart(Vector3.right, Distance, Duration, Cooldown);

            Vector3 total = Vector3.zero;
            for (int i = 0; i < 10; i++)
            {
                total += _dash.Advance(0.05f); // Son karede süre aşılır; fazlası kırpılır.
            }

            Assert.AreEqual(Distance, total.x, Tolerance);
            Assert.AreEqual(0f, total.z, Tolerance);
            Assert.IsFalse(_dash.IsDashing);
        }

        [Test]
        public void Advance_StopsDashingAfterDuration()
        {
            _dash.TryStart(Vector3.forward, Distance, Duration, Cooldown);

            _dash.Advance(Duration * 0.5f);
            Assert.IsTrue(_dash.IsDashing);

            _dash.Advance(Duration * 0.5f);
            Assert.IsFalse(_dash.IsDashing);
            Assert.AreEqual(Vector3.zero, _dash.Advance(0.1f));
        }

        [Test]
        public void Cooldown_StartsWithDashAndBlocksUntilElapsed()
        {
            _dash.TryStart(Vector3.forward, Distance, Duration, Cooldown);
            _dash.Advance(Duration);

            Assert.IsFalse(_dash.TryStart(Vector3.forward, Distance, Duration, Cooldown));

            _dash.Advance(Cooldown - Duration - 0.01f);
            Assert.IsFalse(_dash.IsReady);

            _dash.Advance(0.02f);
            Assert.IsTrue(_dash.IsReady);
            Assert.IsTrue(_dash.TryStart(Vector3.back, Distance, Duration, Cooldown));
        }

        [Test]
        public void Cooldown_ShorterThanDuration_StillWaitsForDashToEnd()
        {
            _dash.TryStart(Vector3.forward, Distance, Duration, 0f);

            Assert.IsFalse(_dash.TryStart(Vector3.forward, Distance, Duration, 0f));

            _dash.Advance(Duration);
            Assert.IsTrue(_dash.IsReady);
        }

        [Test]
        public void Reset_ClearsDashAndCooldown()
        {
            _dash.TryStart(Vector3.forward, Distance, Duration, Cooldown);

            _dash.Reset();

            Assert.IsFalse(_dash.IsDashing);
            Assert.IsTrue(_dash.IsReady);
        }
    }
}
