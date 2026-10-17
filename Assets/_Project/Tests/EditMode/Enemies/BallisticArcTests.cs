using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>BallisticArc: uçuş süresi (yatay hız, en kısa süre), hedefe tam varış, yayın yüksekliği.</summary>
    public class BallisticArcTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void FlightTime_IsPlanarDistanceOverSpeed_WithMinimum()
        {
            Assert.AreEqual(1f, BallisticArc.FlightTime(Vector3.zero, new Vector3(6f, 5f, 8f), 10f, 0.35f), Tolerance, "Yükseklik sayılmaz.");
            Assert.AreEqual(0.35f, BallisticArc.FlightTime(Vector3.zero, new Vector3(1f, 0f, 0f), 10f, 0.35f), Tolerance);
        }

        [Test]
        public void LaunchVelocity_LandsOnTargetAtFlightTime()
        {
            var start = new Vector3(1f, 1.6f, -2f);
            var target = new Vector3(9f, 0.5f, 4f);
            const float time = 1.2f;
            const float gravity = 20f;

            Vector3 velocity = BallisticArc.LaunchVelocity(start, target, time, gravity);
            Vector3 landed = BallisticArc.PositionAt(start, velocity, gravity, time);

            Assert.AreEqual(target.x, landed.x, Tolerance);
            Assert.AreEqual(target.y, landed.y, Tolerance);
            Assert.AreEqual(target.z, landed.z, Tolerance);
        }

        [Test]
        public void Arc_RisesAboveStraightLine_ByMidArcHeight()
        {
            var start = new Vector3(0f, 1f, 0f);
            var target = new Vector3(10f, 1f, 0f);
            const float time = 1f;
            const float gravity = 20f;

            Vector3 velocity = BallisticArc.LaunchVelocity(start, target, time, gravity);
            Vector3 middle = BallisticArc.PositionAt(start, velocity, gravity, time * 0.5f);

            Assert.AreEqual(5f, middle.x, Tolerance);
            Assert.AreEqual(1f + BallisticArc.MidArcHeight(time, gravity), middle.y, Tolerance);
            Assert.AreEqual(2.5f, BallisticArc.MidArcHeight(time, gravity), Tolerance, "10 m / 10 m/sn, g = 20: 2.5 m (barikatın 1.5 m'sini aşar).");
        }
    }
}
