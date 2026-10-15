using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>CoinValue (7.2): ödül × kombo × jeton değeri çarpanı, yuvarlama ve sınırlar.</summary>
    public class CoinValueTests
    {
        [TestCase(10, 1f, 1f, 10)]
        [TestCase(10, 1.5f, 1f, 15)]    // kombo ×1.5 (7.3)
        [TestCase(10, 1f, 1.25f, 13)]   // Açgözlü: 12.5 → 13
        [TestCase(12, 1f, 1.25f, 15)]
        [TestCase(10, 3f, 2f, 60)]      // kombo ×3 + Kan Ayı ×2
        [TestCase(10, 1.15f, 1f, 12)]   // 11.5 → 12 (float'ta 1.15f biraz küçük; decimal ile doğru)
        [TestCase(10, 1.1f, 1f, 11)]
        [TestCase(500, 1f, 1f, 500)]
        public void Calculate_MultipliesAndRounds(int reward, float combo, float value, int expected)
        {
            Assert.AreEqual(expected, CoinValue.Calculate(reward, combo, value));
        }

        [TestCase(0, 1f, 1f)]
        [TestCase(-10, 1f, 1f)]
        [TestCase(10, 0f, 1f)]
        [TestCase(10, 1f, 0f)]
        [TestCase(10, -1f, -1f)]
        [TestCase(10, float.NaN, 1f)]
        public void Calculate_InvalidInputs_Zero(int reward, float combo, float value)
        {
            Assert.AreEqual(0, CoinValue.Calculate(reward, combo, value));
        }

        [Test]
        public void Calculate_Huge_Saturates()
        {
            Assert.AreEqual(int.MaxValue, CoinValue.Calculate(int.MaxValue, 3f, 2f));
            Assert.AreEqual(int.MaxValue, CoinValue.Calculate(10, float.PositiveInfinity, 1f));
        }

        [Test]
        public void Calculate_WithRunModifiers_CoinValueStat()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.CoinValue, 0.25f, "greedy"));
            modifiers.AddModifier(StatModifier.Percent(StatType.CoinValue, 1f, "blood_moon"));

            float multiplier = modifiers.Get(StatType.CoinValue, 1f);

            Assert.AreEqual(23, CoinValue.Calculate(10, 1f, multiplier), "Yüzdeler toplanır: 10 × 2.25 = 22.5 → 23.");
        }
    }
}
