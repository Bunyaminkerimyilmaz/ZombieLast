using LastHaven.UI;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// EdgeIndicatorMath (11.1): ekran içi kontrolü; ekran dışındaki hedefin göstergesi ortadan o yöne, kenardan inset içeride;
    /// açı panel uzayında (sağ 0°, aşağı 90°); kameranın arkasındaki hedef ters yönde.
    /// </summary>
    public class EdgeIndicatorMathTests
    {
        private const float Tolerance = 0.01f;

        private static readonly Vector2 ScreenSize = new Vector2(1920f, 1080f);
        private static readonly Vector2 Inset = new Vector2(50f, 40f);

        [TestCase(0.5f, 0.5f, 10f, true)]
        [TestCase(0f, 1f, 10f, true)]
        [TestCase(1.01f, 0.5f, 10f, false)]
        [TestCase(0.5f, -0.2f, 10f, false)]
        [TestCase(0.5f, 0.5f, -10f, false)]
        public void IsOnScreen(float x, float y, float z, bool expected)
        {
            Assert.AreEqual(expected, EdgeIndicatorMath.IsOnScreen(new Vector3(x, y, z)));
        }

        [Test]
        public void Right_ClampsToRightEdge_PointsRight()
        {
            Vector2 point = EdgeIndicatorMath.ToEdge(new Vector3(1.5f, 0.5f, 10f), ScreenSize, Inset, out float angle);

            AssertVector(new Vector2(1920f - 50f, 540f), point);
            Assert.AreEqual(0f, angle, Tolerance);
        }

        [Test]
        public void Above_ClampsToTopEdge_PointsUp()
        {
            Vector2 point = EdgeIndicatorMath.ToEdge(new Vector3(0.5f, 3f, 10f), ScreenSize, Inset, out float angle);

            AssertVector(new Vector2(960f, 40f), point);
            Assert.AreEqual(-90f, angle, Tolerance, "Panelde yukarı = -y.");
        }

        [Test]
        public void LeftSlightlyDown_ClampsToLeftEdge_KeepsDirection()
        {
            // Ortadan (-2880, +324) yönü: önce sol kenara değer.
            Vector2 point = EdgeIndicatorMath.ToEdge(new Vector3(-1f, 0.2f, 10f), ScreenSize, Inset, out float angle);

            Assert.AreEqual(50f, point.x, Tolerance);
            Assert.That(point.y, Is.InRange(540f, 1080f - 40f), "Ortanın biraz altında.");
            Vector2 direction = point - ScreenSize * 0.5f;
            Assert.AreEqual(Mathf.Atan2(324f, -2880f), Mathf.Atan2(direction.y, direction.x), Tolerance, "Yön korunur.");
            Assert.AreEqual(Mathf.Atan2(324f, -2880f) * Mathf.Rad2Deg, angle, Tolerance);
        }

        [Test]
        public void SteepDownLeft_ClampsToBottomEdgeFirst()
        {
            // Ortadan (-960, +1080) yönü: alt kenar (500 px) sol kenardan (910 px) önce gelir.
            Vector2 point = EdgeIndicatorMath.ToEdge(new Vector3(0f, -0.5f, 10f), ScreenSize, Inset, out _);

            Assert.AreEqual(1080f - 40f, point.y, Tolerance);
            Assert.AreEqual(960f - 960f * 500f / 1080f, point.x, Tolerance);
        }

        [Test]
        public void BehindCamera_DirectionIsMirrored()
        {
            // Kameranın arkasında sağda görünen (viewport x > 0.5) nokta aslında solda.
            Vector2 point = EdgeIndicatorMath.ToEdge(new Vector3(0.8f, 0.5f, -5f), ScreenSize, Inset, out float angle);

            AssertVector(new Vector2(50f, 540f), point);
            Assert.AreEqual(180f, Mathf.Abs(angle), Tolerance);
        }

        [Test]
        public void ExactlyBehindCenter_GoesToBottomEdge()
        {
            Vector2 point = EdgeIndicatorMath.ToEdge(new Vector3(0.5f, 0.5f, -5f), ScreenSize, Inset, out float angle);

            AssertVector(new Vector2(960f, 1080f - 40f), point);
            Assert.AreEqual(90f, angle, Tolerance);
        }

        [Test]
        public void InsetLargerThanScreen_StaysAtCenterAxis()
        {
            Vector2 point = EdgeIndicatorMath.ToEdge(new Vector3(2f, 0.5f, 10f), new Vector2(80f, 60f), Inset, out _);

            AssertVector(new Vector2(40f, 30f), point);
        }

        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, $"x ({actual})");
            Assert.AreEqual(expected.y, actual.y, Tolerance, $"y ({actual})");
        }
    }
}
