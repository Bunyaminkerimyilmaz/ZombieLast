using LastHaven.Core;
using LastHaven.GameFlow;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// StatsTracker (6.3, 7.3): gece değerleri (öldürme, toplanan para, en yüksek kombo, son faiz) gece başında sıfırlanır,
    /// toplamlar birikir, oyun bitince sayaçlar donar.
    /// </summary>
    public class StatsTrackerTests
    {
        private StatsTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new StatsTracker();
        }

        [Test]
        public void New_StartsAtZero()
        {
            Assert.AreEqual(0, _tracker.NightKills);
            Assert.AreEqual(0, _tracker.TotalKills);
            Assert.IsFalse(_tracker.IsNightActive);
            Assert.IsFalse(_tracker.IsFinished);
        }

        [Test]
        public void RecordKill_OutsideNight_CountsOnlyTotal()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Day);

            _tracker.RecordKill();
            _tracker.RecordKill();

            Assert.AreEqual(0, _tracker.NightKills);
            Assert.AreEqual(2, _tracker.TotalKills);
        }

        [Test]
        public void RecordKill_DuringNight_CountsNightAndTotal()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);

            _tracker.RecordKill();
            _tracker.RecordKill();
            _tracker.RecordKill();

            Assert.IsTrue(_tracker.IsNightActive);
            Assert.AreEqual(3, _tracker.NightKills);
            Assert.AreEqual(3, _tracker.TotalKills);
        }

        [Test]
        public void Dawn_KeepsFinishedNightKills()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordKill();
            _tracker.RecordKill();

            _tracker.NotifyPhaseChanged(GamePhase.Dawn);
            _tracker.RecordKill();

            Assert.IsFalse(_tracker.IsNightActive);
            Assert.AreEqual(2, _tracker.NightKills, "Şafak ekranı biten gecenin değerini okur.");
            Assert.AreEqual(3, _tracker.TotalKills);
        }

        [Test]
        public void NextNight_ResetsNightKills_KeepsTotal()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordKill();
            _tracker.RecordKill();
            _tracker.NotifyPhaseChanged(GamePhase.Dawn);
            _tracker.NotifyPhaseChanged(GamePhase.Day);
            _tracker.NotifyPhaseChanged(GamePhase.Dusk);

            Assert.AreEqual(2, _tracker.NightKills, "Gece başlayana kadar önceki gecenin değeri durur.");

            _tracker.NotifyPhaseChanged(GamePhase.Night);
            Assert.AreEqual(0, _tracker.NightKills);

            _tracker.RecordKill();
            Assert.AreEqual(1, _tracker.NightKills);
            Assert.AreEqual(3, _tracker.TotalKills);
        }

        [Test]
        public void NightRepeated_DoesNotReset()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordKill();

            _tracker.NotifyPhaseChanged(GamePhase.Night);

            Assert.AreEqual(1, _tracker.NightKills);
        }

        [TestCase(GamePhase.GameOver)]
        [TestCase(GamePhase.Victory)]
        public void RunEnd_FreezesCounters(GamePhase end)
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordKill();

            _tracker.NotifyPhaseChanged(end);
            _tracker.RecordKill();
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordKill();

            Assert.IsTrue(_tracker.IsFinished);
            Assert.IsFalse(_tracker.IsNightActive);
            Assert.AreEqual(1, _tracker.NightKills, "Oyun durmasa da sonuç ekranındaki değer değişmez.");
            Assert.AreEqual(1, _tracker.TotalKills);
        }

        [Test]
        public void RecordCoin_NightAndTotal()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Day);
            _tracker.RecordCoin(5);
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordCoin(5);
            _tracker.RecordCoin(12);
            _tracker.RecordCoin(0);
            _tracker.RecordCoin(-3);

            Assert.AreEqual(17, _tracker.NightMoneyEarned);
            Assert.AreEqual(22, _tracker.TotalMoneyEarned);
        }

        [Test]
        public void RecordCombo_KeepsBestPerNightAndRun()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordCombo(12);
            _tracker.RecordCombo(0);
            _tracker.RecordCombo(7);
            _tracker.NotifyPhaseChanged(GamePhase.Dawn);

            Assert.AreEqual(12, _tracker.NightBestCombo);
            Assert.AreEqual(12, _tracker.BestCombo);

            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordCombo(4);

            Assert.AreEqual(4, _tracker.NightBestCombo, "Yeni gecede baştan.");
            Assert.AreEqual(12, _tracker.BestCombo);
        }

        [Test]
        public void RecordCombo_OutsideNight_OnlyRunBest()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Day);
            _tracker.RecordCombo(9);

            Assert.AreEqual(0, _tracker.NightBestCombo);
            Assert.AreEqual(9, _tracker.BestCombo);
        }

        [Test]
        public void RecordInterest_AtDawn_LastAndTotal_ResetsAtNight()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.NotifyPhaseChanged(GamePhase.Dawn);
            _tracker.RecordInterest(37);

            Assert.AreEqual(37, _tracker.LastInterest, "Faiz şafakta ödenir; gece evresine bakılmaz.");
            Assert.AreEqual(37, _tracker.TotalInterest);

            _tracker.NotifyPhaseChanged(GamePhase.Day);
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            Assert.AreEqual(0, _tracker.LastInterest, "Yeni gecede son faiz 0'lanır.");

            _tracker.NotifyPhaseChanged(GamePhase.Dawn);
            _tracker.RecordInterest(0);
            _tracker.RecordInterest(50);

            Assert.AreEqual(50, _tracker.LastInterest);
            Assert.AreEqual(87, _tracker.TotalInterest);
        }

        [Test]
        public void NextNight_ResetsAllNightValues()
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordCoin(10);
            _tracker.RecordCombo(6);
            _tracker.NotifyPhaseChanged(GamePhase.Dawn);
            _tracker.RecordInterest(31);
            _tracker.NotifyPhaseChanged(GamePhase.Day);

            _tracker.NotifyPhaseChanged(GamePhase.Night);

            Assert.AreEqual(0, _tracker.NightMoneyEarned);
            Assert.AreEqual(0, _tracker.NightBestCombo);
            Assert.AreEqual(0, _tracker.LastInterest);
            Assert.AreEqual(10, _tracker.TotalMoneyEarned);
            Assert.AreEqual(6, _tracker.BestCombo);
            Assert.AreEqual(31, _tracker.TotalInterest);
        }

        [TestCase(GamePhase.GameOver)]
        [TestCase(GamePhase.Victory)]
        public void RunEnd_FreezesMoneyComboInterest(GamePhase end)
        {
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordCoin(5);
            _tracker.RecordCombo(3);

            _tracker.NotifyPhaseChanged(end);
            _tracker.RecordCoin(5);
            _tracker.RecordCombo(40);
            _tracker.RecordInterest(50);

            Assert.AreEqual(5, _tracker.TotalMoneyEarned);
            Assert.AreEqual(3, _tracker.BestCombo);
            Assert.AreEqual(0, _tracker.TotalInterest);
        }

        [Test]
        public void RecordCoin_Saturates()
        {
            _tracker.RecordCoin(int.MaxValue);
            _tracker.RecordCoin(10);

            Assert.AreEqual(int.MaxValue, _tracker.TotalMoneyEarned);
        }

        [Test]
        public void ImplementsIRunStats()
        {
            IRunStats stats = _tracker;
            _tracker.NotifyPhaseChanged(GamePhase.Night);
            _tracker.RecordKill();

            Assert.AreEqual(1, stats.NightKills);
            Assert.AreEqual(1, stats.TotalKills);
        }
    }
}
