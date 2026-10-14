using LastHaven.Player;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>PlanarMath: kameraya göre hareket yönü, nişan düzlemi kesişimi ve yatay yön.</summary>
    public class PlanarMathTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void CameraRelative_YawZero_UpIsNorth()
        {
            AssertVector(new Vector3(0f, 0f, 1f), PlanarMath.CameraRelative(Vector2.up, 0f));
            AssertVector(new Vector3(1f, 0f, 0f), PlanarMath.CameraRelative(Vector2.right, 0f));
        }

        [Test]
        public void CameraRelative_Yaw90_UpIsEastRightIsSouth()
        {
            AssertVector(new Vector3(1f, 0f, 0f), PlanarMath.CameraRelative(Vector2.up, 90f));
            AssertVector(new Vector3(0f, 0f, -1f), PlanarMath.CameraRelative(Vector2.right, 90f));
        }

        [Test]
        public void CameraRelative_MatchesCameraForwardOnGround()
        {
            // W, kameranın yere izdüşen ileri yönü olmalı (izometrik preset: yaw 45).
            Vector3 cameraForward = Quaternion.Euler(50f, 45f, 0f) * Vector3.forward;
            cameraForward.y = 0f;

            AssertVector(cameraForward.normalized, PlanarMath.CameraRelative(Vector2.up, 45f));
        }

        [Test]
        public void CameraRelative_PreservesMagnitude()
        {
            var input = new Vector2(0.3f, -0.4f);

            Assert.AreEqual(0.5f, PlanarMath.CameraRelative(input, 37f).magnitude, Tolerance);
        }

        [Test]
        public void TryIntersectHorizontalPlane_DownwardRay_HitsAtHeight()
        {
            Vector3 direction = new Vector3(1f, -1f, 0f).normalized;

            Assert.IsTrue(PlanarMath.TryIntersectHorizontalPlane(new Vector3(0f, 10f, 0f), direction, 2f, out Vector3 point));
            AssertVector(new Vector3(8f, 2f, 0f), point);
        }

        [Test]
        public void TryIntersectHorizontalPlane_ParallelOrAwayRay_ReturnsFalse()
        {
            Assert.IsFalse(PlanarMath.TryIntersectHorizontalPlane(new Vector3(0f, 10f, 0f), Vector3.forward, 2f, out _));
            Assert.IsFalse(PlanarMath.TryIntersectHorizontalPlane(new Vector3(0f, 10f, 0f), Vector3.up, 2f, out _));
        }

        [Test]
        public void FlatDirection_IgnoresHeightAndNormalizes()
        {
            Vector3 direction = PlanarMath.FlatDirection(Vector3.zero, new Vector3(3f, 5f, 4f), Vector3.forward, 0.1f);

            AssertVector(new Vector3(0.6f, 0f, 0.8f), direction);
        }

        [Test]
        public void FlatDirection_TooClose_ReturnsFallback()
        {
            Vector3 direction = PlanarMath.FlatDirection(Vector3.zero, new Vector3(0.05f, 2f, 0f), Vector3.left, 0.1f);

            Assert.AreEqual(Vector3.left, direction);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, $"x: beklenen {expected}, gelen {actual}");
            Assert.AreEqual(expected.y, actual.y, Tolerance, $"y: beklenen {expected}, gelen {actual}");
            Assert.AreEqual(expected.z, actual.z, Tolerance, $"z: beklenen {expected}, gelen {actual}");
        }
    }
}
