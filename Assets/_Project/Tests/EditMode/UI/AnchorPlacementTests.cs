using LastHaven.UI;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// AnchorPlacement (10.3): kutu nesnenin üstünde ortalanır ve boşluk bırakır; üstte yer yoksa altına geçer; her durumda ekranın
    /// içinde kalır.
    /// </summary>
    public class AnchorPlacementTests
    {
        private const float Tolerance = 0.001f;
        private static readonly Vector2 Screen = new Vector2(1920f, 1080f);
        private static readonly Vector2 Size = new Vector2(200f, 100f);

        [Test]
        public void Above_CentersBoxAboveAnchor_WithGap()
        {
            Vector2 topLeft = AnchorPlacement.Above(new Vector2(500f, 400f), Size, Screen, 12f, 12f);

            AssertVector(new Vector2(400f, 288f), topLeft);
        }

        [Test]
        public void Above_NearLeftOrRightEdge_StaysInsideWithInset()
        {
            Vector2 left = AnchorPlacement.Above(new Vector2(30f, 400f), Size, Screen, 12f, 12f);
            Vector2 right = AnchorPlacement.Above(new Vector2(1910f, 400f), Size, Screen, 12f, 12f);

            Assert.AreEqual(12f, left.x, Tolerance);
            Assert.AreEqual(1920f - 200f - 12f, right.x, Tolerance);
        }

        [Test]
        public void Above_NearTopEdge_FlipsBelowPointInsteadOfLeavingScreen()
        {
            Vector2 topLeft = AnchorPlacement.Above(new Vector2(500f, 40f), Size, Screen, 12f, 12f);

            Assert.AreEqual(40f + 12f, topLeft.y, Tolerance);
        }

        [Test]
        public void AboveOrBelow_NoRoomAbove_FlipsBelowObjectBottom()
        {
            Vector2 topLeft = AnchorPlacement.AboveOrBelow(new Vector2(500f, 60f), new Vector2(520f, 140f), Size, Screen, 12f, 12f);

            AssertVector(new Vector2(420f, 152f), topLeft);
        }

        [Test]
        public void AboveOrBelow_RoomAbove_StaysAbove()
        {
            Vector2 topLeft = AnchorPlacement.AboveOrBelow(new Vector2(500f, 400f), new Vector2(520f, 480f), Size, Screen, 12f, 12f);

            AssertVector(new Vector2(400f, 288f), topLeft);
        }

        [Test]
        public void AboveOrBelow_NoRoomEitherSide_ClampsAbovePosition()
        {
            var shortScreen = new Vector2(1920f, 200f);

            Vector2 topLeft = AnchorPlacement.AboveOrBelow(new Vector2(500f, 60f), new Vector2(500f, 120f), Size, shortScreen, 12f, 12f);

            AssertVector(new Vector2(400f, 12f), topLeft);
        }

        [Test]
        public void Above_AnchorBelowScreen_ClampsToBottomEdge()
        {
            Vector2 topLeft = AnchorPlacement.Above(new Vector2(500f, 1500f), Size, Screen, 12f, 12f);

            Assert.AreEqual(1080f - 100f - 12f, topLeft.y, Tolerance);
        }

        [Test]
        public void Above_UnmeasuredBox_TreatsSizeAsZero()
        {
            Vector2 topLeft = AnchorPlacement.Above(new Vector2(500f, 400f), new Vector2(float.NaN, float.NaN), Screen, 12f, 12f);

            AssertVector(new Vector2(500f, 388f), topLeft);
        }

        [Test]
        public void Above_BoxLargerThanScreen_PinsToTopLeftInset()
        {
            Vector2 topLeft = AnchorPlacement.Above(new Vector2(500f, 400f), new Vector2(3000f, 2000f), Screen, 12f, 12f);

            AssertVector(new Vector2(12f, 12f), topLeft);
        }

        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
        }
    }
}
