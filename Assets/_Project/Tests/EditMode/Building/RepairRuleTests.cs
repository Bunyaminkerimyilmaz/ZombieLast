using LastHaven.Building;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>RepairRule (GDD §11): 50 can/sn, 5 can başına 1 para; turlar, tam onarım fiyatı, para yetmezliği.</summary>
    public class RepairRuleTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly RepairSettings Gdd = new RepairSettings(50f, 5f);

        [Test]
        public void CycleDuration_IsHealthPerMoneyOverRate()
        {
            Assert.AreEqual(0.1f, Gdd.CycleDuration, Tolerance, "5 can / 50 can/sn.");
            Assert.AreEqual(0f, new RepairSettings(0f, 5f).CycleDuration, "Geçersiz hızda 0.");
        }

        [TestCase(300f, 300f, 0)]
        [TestCase(295f, 300f, 1)]
        [TestCase(288f, 300f, 3)]
        [TestCase(290f, 300f, 2)]
        [TestCase(0.5f, 300f, 60)]
        [TestCase(299.9999f, 300f, 0)]
        public void CostToFull_RoundsUpPerFiveHealth(float current, float max, int expected)
        {
            Assert.AreEqual(expected, RepairRule.CostToFull(current, max, Gdd));
        }

        [Test]
        public void Evaluate_Damaged_HealsUpToFiveForOneMoney()
        {
            Assert.AreEqual(RepairStepResult.Repaired, RepairRule.Evaluate(200f, 300f, true, 10, Gdd, out float heal, out int cost));
            Assert.AreEqual(5f, heal, Tolerance);
            Assert.AreEqual(1, cost);

            RepairRule.Evaluate(298f, 300f, true, 10, Gdd, out heal, out cost);
            Assert.AreEqual(2f, heal, Tolerance, "Son tur eksik kadar onarır.");
            Assert.AreEqual(1, cost, "Eksik can azsa da 1 para.");
        }

        [Test]
        public void Evaluate_NoMoney_StopsWithoutChange()
        {
            Assert.AreEqual(RepairStepResult.NotEnoughMoney, RepairRule.Evaluate(100f, 300f, true, 0, Gdd, out float heal, out int cost));
            Assert.AreEqual(0f, heal);
            Assert.AreEqual(0, cost);
        }

        [Test]
        public void Evaluate_FullOrDestroyed_DoesNothing()
        {
            Assert.AreEqual(RepairStepResult.AlreadyFull, RepairRule.Evaluate(300f, 300f, true, 10, Gdd, out _, out int cost));
            Assert.AreEqual(0, cost);
            Assert.AreEqual(RepairStepResult.Destroyed, RepairRule.Evaluate(0f, 300f, false, 10, Gdd, out _, out cost));
            Assert.AreEqual(0, cost);
        }

        [Test]
        public void NeedsRepair_IgnoresFloatLeftovers()
        {
            Assert.IsTrue(RepairRule.NeedsRepair(299f, 300f));
            Assert.IsFalse(RepairRule.NeedsRepair(299.9999f, 300f));
            Assert.IsFalse(RepairRule.NeedsRepair(300f, 300f));
        }
    }
}
