using LastHaven.Building;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Taret kafasının yatay dönüşü: yaw hesabı, sabit hızlı en kısa yoldan dönüş, hizalama toleransı.</summary>
    public class TurretAimTests
    {
        private const float Tolerance = 0.001f;

        [TestCase(0f, 5f, 0f)]
        [TestCase(5f, 0f, 90f)]
        [TestCase(0f, -5f, 180f)]
        [TestCase(-5f, 0f, 270f)]
        public void YawTowards_CompassDirections(float x, float z, float expected)
        {
            Assert.AreEqual(expected, TurretAim.YawTowards(new Vector3(1f, 3f, 1f), new Vector3(1f + x, 0f, 1f + z)), Tolerance);
        }

        [Test]
        public void YawTowards_SamePointIsZero()
        {
            Assert.AreEqual(0f, TurretAim.YawTowards(Vector3.one, new Vector3(1f, 5f, 1f)));
        }

        [Test]
        public void Direction_MatchesYaw()
        {
            Vector3 east = TurretAim.Direction(90f);

            Assert.AreEqual(1f, east.x, Tolerance);
            Assert.AreEqual(0f, east.y, Tolerance);
            Assert.AreEqual(0f, east.z, Tolerance);
        }

        [Test]
        public void Tick_TurnsAtSpeedWithoutOvershoot()
        {
            var aim = new TurretAim();
            aim.Reset(0f);

            aim.Tick(90f, 360f, 0.1f);
            Assert.AreEqual(36f, aim.Yaw, Tolerance);

            aim.Tick(90f, 360f, 1f);
            Assert.AreEqual(90f, aim.Yaw, Tolerance, "Hedefi geçmez.");
        }

        [Test]
        public void Tick_TakesShortestWayAcrossNorth()
        {
            var aim = new TurretAim();
            aim.Reset(350f);

            aim.Tick(10f, 100f, 0.1f);

            Assert.AreEqual(0f, Mathf.DeltaAngle(aim.Yaw, 0f), Tolerance, "350 → 10: kuzeyden geçer (20°), geriye 340° dönmez.");
            Assert.That(aim.Yaw, Is.InRange(0f, 360f));
        }

        [Test]
        public void Tick_ZeroSpeedDoesNotTurn()
        {
            var aim = new TurretAim();
            aim.Reset(45f);

            aim.Tick(180f, 0f, 1f);

            Assert.AreEqual(45f, aim.Yaw, Tolerance);
        }

        [Test]
        public void Reset_NormalizesYaw()
        {
            var aim = new TurretAim();

            aim.Reset(-90f);
            Assert.AreEqual(270f, aim.Yaw, Tolerance);

            aim.Reset(450f);
            Assert.AreEqual(90f, aim.Yaw, Tolerance);
        }

        [Test]
        public void IsAlignedWith_UsesToleranceAcrossWrap()
        {
            var aim = new TurretAim();
            aim.Reset(358f);

            Assert.IsTrue(aim.IsAlignedWith(2f, 5f), "4° fark.");
            Assert.IsTrue(aim.IsAlignedWith(3f, 5f), "Sınır dahil.");
            Assert.IsFalse(aim.IsAlignedWith(10f, 5f));
        }
    }
}
