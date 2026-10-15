using LastHaven.Enemies;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>HealthBarLayout: dolgu sola yaslı kısalır, değer 0..1'e kırpılır.</summary>
    public class HealthBarLayoutTests
    {
        private const float Tolerance = 0.0001f;
        private const float Width = 0.9f;

        [TestCase(1f, 0.9f, 0f)]
        [TestCase(0.5f, 0.45f, -0.225f)]
        [TestCase(0f, 0f, -0.45f)]
        [TestCase(1.5f, 0.9f, 0f)]
        [TestCase(-0.2f, 0f, -0.45f)]
        public void GetFill_LeftAligned(float normalized, float expectedScale, float expectedOffset)
        {
            HealthBarLayout.GetFill(normalized, Width, out float scaleX, out float offsetX);

            Assert.AreEqual(expectedScale, scaleX, Tolerance);
            Assert.AreEqual(expectedOffset, offsetX, Tolerance);
            // Sol kenar her zaman barın sol kenarında.
            Assert.AreEqual(-Width * 0.5f, offsetX - scaleX * 0.5f, Tolerance);
        }
    }
}
