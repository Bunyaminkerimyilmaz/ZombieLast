using System.Globalization;
using System.Threading;
using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>HudFormat (6.3): zamanlayıcı, oranlar, can (yukarı yuvarlı), mermi; tr-TR kültüründe de aynı çıktı.</summary>
    public class HudFormatTests
    {
        private const float Tolerance = 0.0001f;

        [TestCase(120, "2:00")]
        [TestCase(125, "2:05")]
        [TestCase(60, "1:00")]
        [TestCase(59, "0:59")]
        [TestCase(9, "0:09")]
        [TestCase(1, "0:01")]
        [TestCase(0, "0:00")]
        [TestCase(-3, "0:00")]
        [TestCase(600, "10:00")]
        public void Timer_FormatsMinutesAndSeconds(int seconds, string expected)
        {
            Assert.AreEqual(expected, HudFormat.Timer(seconds));
        }

        [Test]
        public void OfTotal_And_Count()
        {
            Assert.AreEqual("3/10", HudFormat.OfTotal(3, 10));
            Assert.AreEqual("1500", HudFormat.Count(1500));
        }

        [TestCase(50f, 100f, 0.5f)]
        [TestCase(0f, 100f, 0f)]
        [TestCase(150f, 100f, 1f)]
        [TestCase(-5f, 100f, 0f)]
        [TestCase(10f, 0f, 0f)]
        public void Ratio_IsClampedAndSafe(float current, float max, float expected)
        {
            Assert.AreEqual(expected, HudFormat.Ratio(current, max), Tolerance);
        }

        [TestCase(99.2f, 100)]
        [TestCase(0.4f, 1)]
        [TestCase(0f, 0)]
        [TestCase(-3f, 0)]
        [TestCase(1500f, 1500)]
        public void DisplayHealth_RoundsUp(float health, int expected)
        {
            Assert.AreEqual(expected, HudFormat.DisplayHealth(health));
        }

        [Test]
        public void Health_FormatsCurrentOfMax()
        {
            Assert.AreEqual("73/100", HudFormat.Health(72.5f, 100f));
            Assert.AreEqual("0/1500", HudFormat.Health(0f, 1500f));
        }

        [TestCase(1.5f, "×1.5")]
        [TestCase(2f, "×2")]
        [TestCase(3f, "×3")]
        [TestCase(1.25f, "×1.25")]
        public void Multiplier_TrimsZeros(float multiplier, string expected)
        {
            Assert.AreEqual(expected, HudFormat.Multiplier(multiplier));
        }

        [TestCase(35, "+35")]
        [TestCase(0, "+0")]
        [TestCase(-5, "-5")]
        public void Gain_HasSign(int value, string expected)
        {
            Assert.AreEqual(expected, HudFormat.Gain(value));
        }

        [Test]
        public void Ammo_ShowsReserveOrInfinite()
        {
            Assert.AreEqual("8 / 24", HudFormat.Ammo(8, 24, false));
            Assert.AreEqual("12 / " + HudFormat.Infinite, HudFormat.Ammo(12, 0, true));
        }

        [Test]
        public void Output_IsCultureIndependent()
        {
            CultureInfo previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Assert.AreEqual("12:05", HudFormat.Timer(725));
                Assert.AreEqual("1500/1500", HudFormat.Health(1500f, 1500f));
                Assert.AreEqual("30 / 120", HudFormat.Ammo(30, 120, false));
                Assert.AreEqual("×1.5", HudFormat.Multiplier(1.5f), "tr-TR'de ondalık ayırıcı virgül olmamalı.");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }
    }
}
