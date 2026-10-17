using LastHaven.Building;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// FuelBurner ve PowerRule (GDD §12, 9.2): gece tüketimi taban 0.4/sn + taret başına 0.05/sn; kesir birikir, tam birimler
    /// döner; yakıttan fazlası istenmez; tahmini süre; güç = ayakta ve yakıt &gt; 0.
    /// </summary>
    public class FuelBurnerTests
    {
        private const float Tolerance = 0.0001f;

        [TestCase(0, 0.4f)]
        [TestCase(1, 0.45f)]
        [TestCase(4, 0.6f)]
        public void Rate_IsBasePlusPerTurret(int turrets, float expected)
        {
            Assert.AreEqual(expected, FuelBurner.Rate(0.4f, 0.05f, turrets), Tolerance);
        }

        [Test]
        public void Rate_NegativeInputs_CountAsZero()
        {
            Assert.AreEqual(0f, FuelBurner.Rate(-1f, -1f, -3), Tolerance);
            Assert.AreEqual(0.4f, FuelBurner.Rate(0.4f, 0.05f, -2), Tolerance);
        }

        [Test]
        public void Burn_AccumulatesFractionUntilWholeUnit()
        {
            var burner = new FuelBurner();

            Assert.AreEqual(0, burner.Burn(1f, 0.4f, 100));
            Assert.AreEqual(0.4f, burner.Pending, Tolerance);
            Assert.AreEqual(0, burner.Burn(1f, 0.4f, 100));
            Assert.AreEqual(1, burner.Burn(1f, 0.4f, 100), "1.2 birim yakıldı → 1 birim düşülür.");
            Assert.AreEqual(0.2f, burner.Pending, Tolerance);
        }

        [Test]
        public void Burn_OverTime_MatchesRate()
        {
            var burner = new FuelBurner();
            int total = 0;

            // 100 sn, 60 fps, 0.45/sn → 45 birim.
            for (int i = 0; i < 6000; i++)
            {
                total += burner.Burn(1f / 60f, 0.45f, 1000);
            }

            Assert.AreEqual(45, total, 1);
        }

        [Test]
        public void Burn_LongFrame_CanReturnSeveralUnits()
        {
            var burner = new FuelBurner();

            Assert.AreEqual(2, burner.Burn(5f, 0.5f, 100));
            Assert.AreEqual(0.5f, burner.Pending, Tolerance);
        }

        [Test]
        public void Burn_NeverExceedsAvailableFuel()
        {
            var burner = new FuelBurner();

            Assert.AreEqual(3, burner.Burn(20f, 0.4f, 3));
            Assert.AreEqual(0f, burner.Pending, Tolerance, "Yakıt bitti: birikim sıfırlanır.");
        }

        [Test]
        public void Burn_WithoutFuel_ResetsPendingAndReturnsZero()
        {
            var burner = new FuelBurner();
            burner.Burn(1f, 0.6f, 10);

            Assert.AreEqual(0, burner.Burn(1f, 0.6f, 0));
            Assert.AreEqual(0f, burner.Pending, Tolerance);
        }

        [TestCase(0f, 0.4f)]
        [TestCase(1f, 0f)]
        [TestCase(-1f, 0.4f)]
        public void Burn_NoTimeOrRate_BurnsNothing(float deltaTime, float rate)
        {
            var burner = new FuelBurner();

            Assert.AreEqual(0, burner.Burn(deltaTime, rate, 10));
            Assert.AreEqual(0f, burner.Pending, Tolerance);
        }

        [Test]
        public void EstimateSeconds_SubtractsPendingFraction()
        {
            var burner = new FuelBurner();

            Assert.AreEqual(250f, burner.EstimateSeconds(100, 0.4f), Tolerance);
            burner.Burn(1f, 0.4f, 100);
            Assert.AreEqual(249f, burner.EstimateSeconds(100, 0.4f), Tolerance, "0.4 birim yakıldı ama düşülmedi.");
        }

        [Test]
        public void EstimateSeconds_EdgeCases()
        {
            var burner = new FuelBurner();

            Assert.AreEqual(0f, burner.EstimateSeconds(0, 0.4f), Tolerance);
            Assert.IsTrue(float.IsPositiveInfinity(burner.EstimateSeconds(10, 0f)));
        }

        [Test]
        public void Reset_ClearsPending()
        {
            var burner = new FuelBurner();
            burner.Burn(1f, 0.4f, 10);

            burner.Reset();

            Assert.AreEqual(0f, burner.Pending, Tolerance);
        }

        [TestCase(true, 1, true)]
        [TestCase(true, 0, false)]
        [TestCase(false, 50, false)]
        [TestCase(false, 0, false)]
        public void PowerRule_RequiresAliveGeneratorAndFuel(bool alive, int fuel, bool expected)
        {
            Assert.AreEqual(expected, PowerRule.IsPowered(alive, fuel));
        }
    }
}
