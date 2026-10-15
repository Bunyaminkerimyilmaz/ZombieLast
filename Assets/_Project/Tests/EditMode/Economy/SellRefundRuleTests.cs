using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Satış iadesi (GDD §10: harcananın %50'si, aşağı yuvarlı) ve EconomyConfig'in <see cref="IEconomyRules"/>'u.</summary>
    public class SellRefundRuleTests
    {
        [TestCase(50, 25)]
        [TestCase(170, 85)]
        [TestCase(470, 235)]
        [TestCase(25, 12)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        [TestCase(-40, 0)]
        public void Calculate_Half_RoundsDown(int spent, int expected)
        {
            Assert.AreEqual(expected, new SellRefundRule(0.5f).Calculate(spent));
        }

        [Test]
        public void Calculate_FloatRate_HasNoOffByOne()
        {
            Assert.AreEqual(7, new SellRefundRule(0.7f).Calculate(10), "10 × 0.7 = 7 (float 6.9999 değil).");
        }

        [Test]
        public void Rate_IsClampedToZeroOne()
        {
            Assert.AreEqual(100, new SellRefundRule(3f).Calculate(100));
            Assert.AreEqual(0, new SellRefundRule(-1f).Calculate(100));
            Assert.AreEqual(0, new SellRefundRule(float.NaN).Calculate(100));
        }

        [Test]
        public void EconomyConfig_AsEconomyRules_RefundsHalf()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfig>();
            try
            {
                IEconomyRules rules = config;

                Assert.AreEqual(85, rules.GetSellRefund(170));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
