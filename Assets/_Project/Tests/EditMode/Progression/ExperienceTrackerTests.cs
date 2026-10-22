using System;
using LastHaven.Progression;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// ExperienceTracker (13.1): XP birikimi, devreden XP, tek kazançla çoklu seviye atlama ve bekleyen seviye kuyruğu
    /// (GDD §13 eşikleri: 16, 22, 28, 34…).
    /// </summary>
    public class ExperienceTrackerTests
    {
        private const float Tolerance = 0.0001f;

        private ExperienceTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new ExperienceTracker(new LevelCurve(10, 6));
        }

        [Test]
        public void New_StartsAtLevelOneWithNoXp()
        {
            Assert.AreEqual(1, _tracker.Level);
            Assert.AreEqual(0, _tracker.Xp);
            Assert.AreEqual(0, _tracker.TotalXp);
            Assert.AreEqual(16, _tracker.XpToNextLevel);
            Assert.AreEqual(0f, _tracker.Progress, Tolerance);
            Assert.AreEqual(0, _tracker.PendingLevelUps);
        }

        [TestCase(0, 1)]
        [TestCase(-4, 1)]
        [TestCase(3, 3)]
        public void Constructor_StartingLevel_IsAtLeastOne(int startingLevel, int expected)
        {
            var tracker = new ExperienceTracker(new LevelCurve(10, 6), startingLevel);

            Assert.AreEqual(expected, tracker.Level);
            Assert.AreEqual(10 + 6 * expected, tracker.XpToNextLevel);
        }

        [Test]
        public void AddXp_BelowThreshold_Accumulates()
        {
            int gained = _tracker.AddXp(5); // Tank
            gained += _tracker.AddXp(1); // Yürüyen

            Assert.AreEqual(0, gained);
            Assert.AreEqual(1, _tracker.Level);
            Assert.AreEqual(6, _tracker.Xp);
            Assert.AreEqual(6, _tracker.TotalXp);
            Assert.AreEqual(6f / 16f, _tracker.Progress, Tolerance);
            Assert.AreEqual(0, _tracker.PendingLevelUps);
        }

        [Test]
        public void AddXp_ExactThreshold_LevelsUpWithNoCarry()
        {
            int gained = _tracker.AddXp(16);

            Assert.AreEqual(1, gained);
            Assert.AreEqual(2, _tracker.Level);
            Assert.AreEqual(0, _tracker.Xp);
            Assert.AreEqual(22, _tracker.XpToNextLevel);
            Assert.AreEqual(1, _tracker.PendingLevelUps);
        }

        [Test]
        public void AddXp_OneXpAtATime_LevelsUpOnSixteenthKill()
        {
            for (int i = 0; i < 15; i++)
            {
                Assert.AreEqual(0, _tracker.AddXp(1));
            }

            Assert.AreEqual(1, _tracker.AddXp(1), "16. Yürüyen seviye atlatır.");
            Assert.AreEqual(2, _tracker.Level);
        }

        [Test]
        public void AddXp_OverThreshold_CarriesRemainder()
        {
            _tracker.AddXp(14);

            int gained = _tracker.AddXp(5);

            Assert.AreEqual(1, gained);
            Assert.AreEqual(2, _tracker.Level);
            Assert.AreEqual(3, _tracker.Xp, "14 + 5 − 16 = 3 sonraki seviyeye devreder.");
            Assert.AreEqual(19, _tracker.TotalXp);
        }

        [Test]
        public void AddXp_LargeGain_SkipsMultipleLevels_QueuedInOrder()
        {
            // 16 + 22 + 28 = 66 → seviye 4; 70 − 66 = 4 devreder.
            int gained = _tracker.AddXp(70);

            Assert.AreEqual(3, gained);
            Assert.AreEqual(4, _tracker.Level);
            Assert.AreEqual(4, _tracker.Xp);
            Assert.AreEqual(34, _tracker.XpToNextLevel);
            Assert.AreEqual(3, _tracker.PendingLevelUps);

            Assert.IsTrue(_tracker.TryDequeueLevelUp(out int first));
            Assert.IsTrue(_tracker.TryDequeueLevelUp(out int second));
            Assert.IsTrue(_tracker.TryDequeueLevelUp(out int third));
            Assert.AreEqual(new[] { 2, 3, 4 }, new[] { first, second, third });
            Assert.AreEqual(0, _tracker.PendingLevelUps);
        }

        [Test]
        public void AddXp_BossKill_FromLevelOne_SkipsOneLevel()
        {
            int gained = _tracker.AddXp(30); // Brute

            Assert.AreEqual(1, gained, "30 − 16 = 14 < 22.");
            Assert.AreEqual(2, _tracker.Level);
            Assert.AreEqual(14, _tracker.Xp);
        }

        [Test]
        public void PendingLevels_AccumulateAcrossGains_UntilDequeued()
        {
            _tracker.AddXp(16);
            _tracker.AddXp(22);

            Assert.AreEqual(2, _tracker.PendingLevelUps, "Seçilmeyen seviyeler kuyrukta bekler.");
            Assert.IsTrue(_tracker.TryDequeueLevelUp(out int level));
            Assert.AreEqual(2, level);

            _tracker.AddXp(28);

            Assert.AreEqual(2, _tracker.PendingLevelUps);
            Assert.IsTrue(_tracker.TryDequeueLevelUp(out level));
            Assert.AreEqual(3, level);
            Assert.IsTrue(_tracker.TryDequeueLevelUp(out level));
            Assert.AreEqual(4, level);
        }

        [Test]
        public void TryDequeueLevelUp_Empty_ReturnsFalse()
        {
            Assert.IsFalse(_tracker.TryDequeueLevelUp(out int level));
            Assert.AreEqual(0, level);
        }

        [Test]
        public void AddXp_Zero_DoesNothing()
        {
            Assert.AreEqual(0, _tracker.AddXp(0));
            Assert.AreEqual(0, _tracker.TotalXp);
        }

        [Test]
        public void AddXp_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _tracker.AddXp(-1));
            Assert.AreEqual(0, _tracker.Xp);
        }

        [Test]
        public void AddXp_HugeGains_TotalSaturates()
        {
            var flat = new ExperienceTracker(new LevelCurve(int.MaxValue, 0));

            flat.AddXp(int.MaxValue - 1);
            flat.AddXp(int.MaxValue);

            Assert.AreEqual(int.MaxValue, flat.TotalXp);
            Assert.AreEqual(2, flat.Level, "Tek eşik aşıldı (taşma olmadan).");
            Assert.AreEqual(int.MaxValue - 1, flat.Xp);
        }
    }
}
