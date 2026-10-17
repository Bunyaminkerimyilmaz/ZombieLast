using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Taretin etkili değerleri (GDD §11): seviyenin mutlak statı × run çarpanları (TurretDamage/FireRate/Range).</summary>
    public class TurretStatsTests
    {
        private const float Tolerance = 0.0001f;

        private static readonly BuildableLevel Level1 = new BuildableLevel("Standart", 250, 250f,
            new BuildableStatValue(BuildableStat.Damage, 8f),
            new BuildableStatValue(BuildableStat.FireRate, 5f),
            new BuildableStatValue(BuildableStat.Range, 12f));

        private static readonly BuildableLevel Level3 = new BuildableLevel("Gelişmiş", 500, 250f,
            new BuildableStatValue(BuildableStat.Damage, 12.8f),
            new BuildableStatValue(BuildableStat.FireRate, 6.5f),
            new BuildableStatValue(BuildableStat.Range, 15f));

        private static readonly BuildableLevel NoStats = new BuildableLevel("Ahşap", 50, 300f);

        [Test]
        public void From_WithoutModifiers_UsesLevelValues()
        {
            TurretStats stats = TurretStats.From(Level1, null);

            Assert.AreEqual(8f, stats.Damage, Tolerance);
            Assert.AreEqual(5f, stats.FireRate, Tolerance);
            Assert.AreEqual(12f, stats.Range, Tolerance);
            Assert.AreEqual(0.2f, stats.FireInterval, Tolerance);
        }

        [Test]
        public void From_HigherLevel_HitsHarder()
        {
            Assert.Greater(TurretStats.From(Level3, null).Damage, TurretStats.From(Level1, null).Damage);
            Assert.AreEqual(15f, TurretStats.From(Level3, null).Range, Tolerance);
        }

        [Test]
        public void From_AppliesTurretRunModifiers()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.TurretDamage, 0.5f, "test"));
            modifiers.AddModifier(StatModifier.Percent(StatType.TurretFireRate, 0.25f, "test"));
            modifiers.AddModifier(StatModifier.Flat(StatType.TurretRange, 2f, "test"));
            modifiers.AddModifier(StatModifier.Percent(StatType.PlayerDamage, 1f, "test")); // Oyuncu statı taretleri etkilemez.

            TurretStats stats = TurretStats.From(Level1, modifiers);

            Assert.AreEqual(12f, stats.Damage, Tolerance);
            Assert.AreEqual(6.25f, stats.FireRate, Tolerance);
            Assert.AreEqual(14f, stats.Range, Tolerance);
        }

        [Test]
        public void From_MissingStatsAreZeroAndFireRateIsClamped()
        {
            TurretStats stats = TurretStats.From(NoStats, null);

            Assert.AreEqual(0f, stats.Damage);
            Assert.AreEqual(0f, stats.Range);
            Assert.AreEqual(TurretStats.MinFireRate, stats.FireRate, Tolerance);
        }

        [Test]
        public void From_NullLevel_IsHarmless()
        {
            TurretStats stats = TurretStats.From(null, null);

            Assert.AreEqual(0f, stats.Damage);
            Assert.AreEqual(0f, stats.Range);
            Assert.That(float.IsFinite(stats.FireInterval));
        }

        [Test]
        public void TryGetRange_OnlyForLevelsWithRange()
        {
            Assert.IsFalse(TurretStats.TryGetRange(NoStats, null, out _), "Barikatta menzil dairesi yok.");
            Assert.IsFalse(TurretStats.TryGetRange(null, null, out _));
            Assert.IsTrue(TurretStats.TryGetRange(Level1, null, out float range));
            Assert.AreEqual(12f, range, Tolerance);
        }

        [Test]
        public void TryGetRange_AppliesRangeModifier()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.TurretRange, 0.5f, "test"));

            Assert.IsTrue(TurretStats.TryGetRange(Level1, modifiers, out float range));
            Assert.AreEqual(18f, range, Tolerance);
        }
    }
}
