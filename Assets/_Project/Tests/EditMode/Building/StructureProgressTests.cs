using LastHaven.Building;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Yapı seviyesi ve toplam harcama (GDD §11 barikat: 50 → +120 → +300).</summary>
    public class StructureProgressTests
    {
        private static readonly BuildableLevel[] BarricadeLevels =
        {
            new BuildableLevel("Ahşap", 50, 300f),
            new BuildableLevel("Metal", 120, 800f),
            new BuildableLevel("Beton", 300, 2000f),
        };

        [Test]
        public void New_IsEmpty()
        {
            var progress = new StructureProgress();

            Assert.AreEqual(0, progress.LevelCount);
            Assert.IsNull(progress.Current);
            Assert.IsFalse(progress.CanUpgrade);
            Assert.AreEqual(0, progress.UpgradeCost);
            Assert.IsFalse(progress.Advance());
        }

        [Test]
        public void Reset_StartsAtFirstLevelWithBuildCostSpent()
        {
            var progress = new StructureProgress();

            progress.Reset(BarricadeLevels, 50);

            Assert.AreEqual(0, progress.LevelIndex);
            Assert.AreEqual("Ahşap", progress.Current.DisplayName);
            Assert.AreEqual(50, progress.TotalSpent);
            Assert.IsTrue(progress.CanUpgrade);
            Assert.AreEqual(120, progress.UpgradeCost, "Metal: +120.");
        }

        [Test]
        public void Advance_AddsEachUpgradeCostUntilMaxLevel()
        {
            var progress = new StructureProgress();
            progress.Reset(BarricadeLevels, 50);

            Assert.IsTrue(progress.Advance());
            Assert.AreEqual("Metal", progress.Current.DisplayName);
            Assert.AreEqual(170, progress.TotalSpent);
            Assert.AreEqual(300, progress.UpgradeCost, "Beton: +300.");

            Assert.IsTrue(progress.Advance());
            Assert.AreEqual(2, progress.LevelIndex);
            Assert.AreEqual(470, progress.TotalSpent);
            Assert.IsFalse(progress.CanUpgrade);
            Assert.AreEqual(0, progress.UpgradeCost);
            Assert.IsNull(progress.Next);
        }

        [Test]
        public void Advance_AtMaxLevel_ChangesNothing()
        {
            var progress = new StructureProgress();
            progress.Reset(new[] { new BuildableLevel("Tek", 80, 100f) }, 80);

            Assert.IsFalse(progress.Advance());
            Assert.AreEqual(0, progress.LevelIndex);
            Assert.AreEqual(80, progress.TotalSpent);
        }

        [Test]
        public void Reset_AfterUpgrades_StartsOver()
        {
            var progress = new StructureProgress();
            progress.Reset(BarricadeLevels, 50);
            progress.Advance();

            progress.Reset(BarricadeLevels, 50);

            Assert.AreEqual(0, progress.LevelIndex);
            Assert.AreEqual(50, progress.TotalSpent);
        }

        [Test]
        public void Reset_NegativeBuildCost_CountsAsZero()
        {
            var progress = new StructureProgress();

            progress.Reset(BarricadeLevels, -10);

            Assert.AreEqual(0, progress.TotalSpent);
        }

        [Test]
        public void Reset_NullLevels_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new StructureProgress().Reset(null, 0));
        }
    }
}
