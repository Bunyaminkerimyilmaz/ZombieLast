using LastHaven.Building;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Tuzak statları (GDD §11): seviyeden okuma, tik başına hasar (15 × 0.5), kırpma, eksik stat.</summary>
    public class SpikeTrapStatsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void From_ReadsLevelStats_AndDamagePerTick()
        {
            var level = new BuildableLevel("Standart", 80, 40f,
                new BuildableStatValue(BuildableStat.Damage, 15f),
                new BuildableStatValue(BuildableStat.SlowFraction, 0.4f),
                new BuildableStatValue(BuildableStat.TickInterval, 0.5f));

            SpikeTrapStats stats = SpikeTrapStats.From(level);

            Assert.IsTrue(stats.IsValid);
            Assert.AreEqual(15f, stats.DamagePerSecond, Tolerance);
            Assert.AreEqual(0.4f, stats.SlowFraction, Tolerance);
            Assert.AreEqual(0.5f, stats.TickInterval, Tolerance);
            Assert.AreEqual(7.5f, stats.DamagePerTick, Tolerance, "Saniyede 15 hasar, 0.5 sn'de bir tik.");
        }

        [Test]
        public void MissingTickInterval_IsInvalid()
        {
            var level = new BuildableLevel("Eksik", 80, 40f, new BuildableStatValue(BuildableStat.Damage, 15f));

            Assert.IsFalse(SpikeTrapStats.From(level).IsValid);
            Assert.IsFalse(SpikeTrapStats.From(null).IsValid);
        }

        [Test]
        public void Constructor_ClampsValues()
        {
            var stats = new SpikeTrapStats(-5f, 1.5f, -1f);

            Assert.AreEqual(0f, stats.DamagePerSecond);
            Assert.AreEqual(1f, stats.SlowFraction);
            Assert.AreEqual(0f, stats.TickInterval);
        }
    }
}
