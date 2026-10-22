using LastHaven.Progression;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>LevelCurve (13.1): GDD §13 eşik formülü, sonraki seviye için 10 + 6 × mevcut seviye.</summary>
    public class LevelCurveTests
    {
        private static readonly LevelCurve GddCurve = new LevelCurve(10, 6);

        [TestCase(1, 16)]
        [TestCase(2, 22)]
        [TestCase(3, 28)]
        [TestCase(5, 40)]
        [TestCase(10, 70)]
        public void XpToNext_MatchesGddFormula(int level, int expected)
        {
            Assert.AreEqual(expected, GddCurve.XpToNext(level));
        }

        [Test]
        public void XpToNext_NegativeLevel_CountsAsZero()
        {
            Assert.AreEqual(10, GddCurve.XpToNext(-3));
        }

        [Test]
        public void XpToNext_IsAtLeastOne()
        {
            var zero = new LevelCurve(0, 0);

            Assert.AreEqual(1, zero.XpToNext(1), "Eşik 0 olursa tek kazançla sonsuz seviye atlanırdı.");
            Assert.AreEqual(1, default(LevelCurve).XpToNext(5));
        }

        [Test]
        public void Constructor_ClampsNegativeValues()
        {
            var curve = new LevelCurve(-5, -2);

            Assert.AreEqual(0, curve.BaseXp);
            Assert.AreEqual(0, curve.XpPerLevel);
        }

        [Test]
        public void XpToNext_HugeLevel_SaturatesAtIntMax()
        {
            var steep = new LevelCurve(int.MaxValue, int.MaxValue);

            Assert.AreEqual(int.MaxValue, steep.XpToNext(int.MaxValue));
        }
    }
}
