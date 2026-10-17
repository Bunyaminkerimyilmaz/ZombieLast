using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.GameFlow;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>GameStateMachine (GDD §3, §9, §15): evre geçişleri, gündüz sayacı, son uyarı, erken hazır, zafer ve oyun bitti.</summary>
    public class GameStateMachineTests
    {
        private const float Tolerance = 0.0001f;

        // GDD: gündüz 120, alacakaranlık 4, son uyarı 15, kalan her 10 sn için +5 para.
        private static readonly GameFlowSettings DefaultSettings = new GameFlowSettings(120f, 4f, 15f, 5, 10f);

        private readonly List<GamePhase> _phases = new List<GamePhase>();
        private readonly List<int> _startedNights = new List<int>();
        private readonly List<int> _timerTicks = new List<int>();
        private readonly List<float> _endingSoon = new List<float>();
        private readonly List<(float Remaining, int Bonus, GamePhase PhaseAtEvent)> _earlyReady =
            new List<(float, int, GamePhase)>();

        private GameStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _phases.Clear();
            _startedNights.Clear();
            _timerTicks.Clear();
            _endingSoon.Clear();
            _earlyReady.Clear();
            _machine = Create(3);
        }

        // --- Kurulum ---

        [Test]
        public void Constructor_StartsInBoot()
        {
            Assert.AreEqual(GamePhase.Boot, _machine.Phase);
            Assert.AreEqual(0, _machine.NightIndex);
            Assert.AreEqual(3, _machine.TotalNights);
            Assert.AreEqual(0f, _machine.PhaseTimeRemaining, Tolerance);
            Assert.IsFalse(_machine.IsOver);
            Assert.IsEmpty(_phases);
        }

        [Test]
        public void Constructor_RejectsInvalidArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameStateMachine(DefaultSettings, 0, _ => { }));
            Assert.Throws<ArgumentNullException>(() => new GameStateMachine(DefaultSettings, 1, null));
        }

        [Test]
        public void Tick_InBoot_DoesNothing()
        {
            _machine.Tick(500f);

            Assert.AreEqual(GamePhase.Boot, _machine.Phase);
            Assert.IsEmpty(_phases);
        }

        [Test]
        public void Begin_EntersFirstDay()
        {
            Assert.IsTrue(_machine.Begin());

            Assert.AreEqual(GamePhase.Day, _machine.Phase);
            Assert.AreEqual(0, _machine.NightIndex);
            Assert.AreEqual(120f, _machine.PhaseDuration, Tolerance);
            Assert.AreEqual(120f, _machine.PhaseTimeRemaining, Tolerance);
            CollectionAssert.AreEqual(new[] { GamePhase.Day }, _phases);
            CollectionAssert.AreEqual(new[] { 120 }, _timerTicks);
        }

        [Test]
        public void Begin_Twice_IsIgnored()
        {
            _machine.Begin();

            Assert.IsFalse(_machine.Begin());
            Assert.AreEqual(1, _phases.Count);
        }

        // --- Gündüz ---

        [Test]
        public void DayTimer_TicksOncePerWholeSecond()
        {
            _machine.Begin();

            _machine.Tick(0.5f); // 119.5 → hâlâ 120
            CollectionAssert.AreEqual(new[] { 120 }, _timerTicks);

            _machine.Tick(0.5f); // 119
            CollectionAssert.AreEqual(new[] { 120, 119 }, _timerTicks);

            for (int i = 0; i < 4; i++)
            {
                _machine.Tick(0.25f); // 118.75, 118.5, 118.25, 118
            }

            CollectionAssert.AreEqual(new[] { 120, 119, 118 }, _timerTicks);

            _machine.Tick(3.5f); // 114.5 → uzun kare: tek olay, ara saniyeler atlanır
            CollectionAssert.AreEqual(new[] { 120, 119, 118, 115 }, _timerTicks);
        }

        [Test]
        public void Day_EndsWhenTimeRunsOut_EntersDusk()
        {
            _machine.Begin();

            _machine.Tick(119.5f);
            Assert.AreEqual(GamePhase.Day, _machine.Phase);
            Assert.AreEqual(1, _timerTicks[_timerTicks.Count - 1], "Son saniye 1 gösterilir.");

            _machine.Tick(0.5f);
            Assert.AreEqual(GamePhase.Dusk, _machine.Phase);
            Assert.AreEqual(4f, _machine.PhaseDuration, Tolerance);
            Assert.AreEqual(4f, _machine.PhaseTimeRemaining, Tolerance);
            Assert.AreEqual(0, _machine.NightIndex);
            CollectionAssert.DoesNotContain(_timerTicks, 0);
            Assert.IsEmpty(_earlyReady, "Süre bitince erken hazır bonusu yok.");
            Assert.IsEmpty(_startedNights);
        }

        [Test]
        public void DayEndingSoon_RaisedOnceAtFinalWarning()
        {
            _machine.Begin();

            _machine.Tick(104f); // 16 sn
            Assert.IsEmpty(_endingSoon);

            _machine.Tick(1f); // 15 sn
            Assert.AreEqual(1, _endingSoon.Count);
            Assert.AreEqual(15f, _endingSoon[0], Tolerance);

            _machine.Tick(1f);
            _machine.Tick(10f);
            Assert.AreEqual(1, _endingSoon.Count);
        }

        [Test]
        public void DayEndingSoon_NotRaised_WhenDayEndsEarly()
        {
            _machine.Begin();
            _machine.RequestReady();
            _machine.Tick(10f);

            Assert.IsEmpty(_endingSoon);
        }

        [Test]
        public void DayEndingSoon_RaisedAtBegin_WhenDayIsShorterThanWarning()
        {
            _machine = Create(3, new GameFlowSettings(10f, 4f, 15f, 5, 10f));

            _machine.Begin();

            Assert.AreEqual(1, _endingSoon.Count);
            Assert.AreEqual(10f, _endingSoon[0], Tolerance);
        }

        [Test]
        public void DayEndingSoon_Disabled_WhenWarningIsZero()
        {
            _machine = Create(3, new GameFlowSettings(120f, 4f, 0f, 5, 10f));

            _machine.Begin();
            _machine.Tick(119f);

            Assert.IsEmpty(_endingSoon);
        }

        [Test]
        public void DayEndingSoon_RaisedAgainOnNextDay()
        {
            _machine.Begin();
            _machine.Tick(110f);
            DriveDayToDawn();
            _machine.RequestDawnContinue();

            _machine.Tick(110f);

            Assert.AreEqual(2, _endingSoon.Count);
        }

        // --- Erken hazır ---

        [Test]
        public void Ready_InDay_RaisesEarlyReadyBeforeDusk()
        {
            _machine.Begin();
            _machine.Tick(33f); // 87 sn kaldı

            Assert.IsTrue(_machine.RequestReady());

            Assert.AreEqual(1, _earlyReady.Count);
            Assert.AreEqual(87f, _earlyReady[0].Remaining, Tolerance);
            Assert.AreEqual(40, _earlyReady[0].Bonus, "87 sn → 8 tam adım × 5 para.");
            Assert.AreEqual(GamePhase.Day, _earlyReady[0].PhaseAtEvent, "Bonus olayı alacakaranlıktan önce gelir.");
            Assert.AreEqual(GamePhase.Dusk, _machine.Phase);
            CollectionAssert.AreEqual(new[] { GamePhase.Day, GamePhase.Dusk }, _phases);
        }

        [Test]
        public void Ready_AtStartOfDay_GivesFullBonus()
        {
            _machine.Begin();
            _machine.RequestReady();

            Assert.AreEqual(60, _earlyReady[0].Bonus);
        }

        [Test]
        public void Ready_WithLessThanOneStepLeft_GivesNoBonus_ButEndsDay()
        {
            _machine.Begin();
            _machine.Tick(111f); // 9 sn

            Assert.IsTrue(_machine.RequestReady());

            Assert.AreEqual(0, _earlyReady[0].Bonus);
            Assert.AreEqual(GamePhase.Dusk, _machine.Phase);
        }

        [Test]
        public void EarlyReadyBonus_MatchesWhatReadyWouldGive_ZeroOutsideDay()
        {
            Assert.AreEqual(0, _machine.EarlyReadyBonus, "Boot");

            _machine.Begin();
            Assert.AreEqual(60, _machine.EarlyReadyBonus, "120 sn → 12 adım × 5.");

            _machine.Tick(10.5f); // 109.5 sn, HUD'da 110
            Assert.AreEqual(55, _machine.EarlyReadyBonus);

            _machine.Tick(99.6f); // 9.9 sn, HUD'da 10
            Assert.AreEqual(5, _machine.EarlyReadyBonus);
            int shown = _machine.EarlyReadyBonus;
            _machine.RequestReady();
            Assert.AreEqual(shown, _earlyReady[0].Bonus, "İpucundaki bonus basınca verilenle aynı.");
            Assert.AreEqual(0, _machine.EarlyReadyBonus, "Dusk");
        }

        [Test]
        public void Ready_OutsideDay_IsIgnored()
        {
            Assert.IsFalse(_machine.RequestReady(), "Boot");

            _machine.Begin();
            _machine.RequestReady();
            Assert.IsFalse(_machine.RequestReady(), "Dusk");

            _machine.Tick(4f);
            Assert.IsFalse(_machine.RequestReady(), "Night");

            _machine.NotifyNightCompleted();
            Assert.IsFalse(_machine.RequestReady(), "Dawn");

            Assert.AreEqual(1, _earlyReady.Count);
            Assert.AreEqual(GamePhase.Dawn, _machine.Phase);
        }

        // --- Alacakaranlık ve gece ---

        [Test]
        public void Dusk_EndsAfterDuration_StartsNight()
        {
            _machine.Begin();
            _machine.RequestReady();

            _machine.Tick(3f);
            Assert.AreEqual(GamePhase.Dusk, _machine.Phase);
            Assert.AreEqual(1f, _machine.PhaseTimeRemaining, Tolerance);
            Assert.IsEmpty(_startedNights);

            _machine.Tick(1f);
            Assert.AreEqual(GamePhase.Night, _machine.Phase);
            Assert.AreEqual(0f, _machine.PhaseDuration, Tolerance);
            Assert.AreEqual(0f, _machine.PhaseTimeRemaining, Tolerance);
            CollectionAssert.AreEqual(new[] { 0 }, _startedNights);
        }

        [Test]
        public void LongFrame_MakesOnlyOneTimedTransitionPerTick()
        {
            _machine.Begin();

            _machine.Tick(500f);
            Assert.AreEqual(GamePhase.Dusk, _machine.Phase);
            Assert.AreEqual(4f, _machine.PhaseTimeRemaining, Tolerance, "Artan süre alacakaranlığa devretmez.");

            _machine.Tick(500f);
            Assert.AreEqual(GamePhase.Night, _machine.Phase);
        }

        [Test]
        public void Night_DoesNotEndByTime()
        {
            DriveToNight();

            _machine.Tick(1000f);

            Assert.AreEqual(GamePhase.Night, _machine.Phase);
            Assert.AreEqual(1, _startedNights.Count);
        }

        [Test]
        public void NightCompleted_InNight_EntersDawn()
        {
            DriveToNight();

            Assert.IsTrue(_machine.NotifyNightCompleted());

            Assert.AreEqual(GamePhase.Dawn, _machine.Phase);
            Assert.AreEqual(0, _machine.NightIndex, "Şafakta indeks biten gecededir.");
            Assert.AreEqual(0f, _machine.PhaseDuration, Tolerance);
        }

        [Test]
        public void NightCompleted_OutsideNight_IsIgnored()
        {
            Assert.IsFalse(_machine.NotifyNightCompleted(), "Boot");

            _machine.Begin();
            Assert.IsFalse(_machine.NotifyNightCompleted(), "Day");

            _machine.RequestReady();
            Assert.IsFalse(_machine.NotifyNightCompleted(), "Dusk");

            _machine.Tick(4f);
            _machine.NotifyNightCompleted();
            Assert.IsFalse(_machine.NotifyNightCompleted(), "Dawn");

            CollectionAssert.AreEqual(
                new[] { GamePhase.Day, GamePhase.Dusk, GamePhase.Night, GamePhase.Dawn }, _phases);
        }

        // --- Şafak ---

        [Test]
        public void DawnContinue_EntersNextDay()
        {
            DriveToNight();
            _machine.NotifyNightCompleted();

            Assert.IsTrue(_machine.RequestDawnContinue());

            Assert.AreEqual(GamePhase.Day, _machine.Phase);
            Assert.AreEqual(1, _machine.NightIndex);
            Assert.AreEqual(120f, _machine.PhaseTimeRemaining, Tolerance);
            Assert.AreEqual(120, _timerTicks[_timerTicks.Count - 1], "Sayaç yeni gündüzde baştan başlar.");
        }

        [Test]
        public void DawnContinue_OutsideDawn_IsIgnored()
        {
            Assert.IsFalse(_machine.RequestDawnContinue(), "Boot");

            _machine.Begin();
            Assert.IsFalse(_machine.RequestDawnContinue(), "Day");

            _machine.RequestReady();
            Assert.IsFalse(_machine.RequestDawnContinue(), "Dusk");

            _machine.Tick(4f);
            Assert.IsFalse(_machine.RequestDawnContinue(), "Night");

            Assert.AreEqual(0, _machine.NightIndex);
        }

        [Test]
        public void FullCycle_FollowsDayDuskNightDawnDay()
        {
            _machine.Begin();
            _machine.Tick(120f);
            _machine.Tick(4f);
            _machine.NotifyNightCompleted();
            _machine.RequestDawnContinue();
            _machine.RequestReady();
            _machine.Tick(4f);

            CollectionAssert.AreEqual(
                new[]
                {
                    GamePhase.Day, GamePhase.Dusk, GamePhase.Night, GamePhase.Dawn,
                    GamePhase.Day, GamePhase.Dusk, GamePhase.Night,
                },
                _phases);
            CollectionAssert.AreEqual(new[] { 0, 1 }, _startedNights);
            Assert.AreEqual(1, _machine.NightIndex);
        }

        // --- Zafer ---

        [Test]
        public void LastNightCompleted_EntersVictory_InsteadOfDawn()
        {
            _machine = Create(2);
            DriveToNight();
            _machine.NotifyNightCompleted();
            _machine.RequestDawnContinue();
            _machine.RequestReady();
            _machine.Tick(4f);

            Assert.IsTrue(_machine.NotifyNightCompleted());

            Assert.AreEqual(GamePhase.Victory, _machine.Phase);
            Assert.AreEqual(1, _machine.NightIndex);
            Assert.IsTrue(_machine.IsOver);
            Assert.AreEqual(GamePhase.Victory, _phases[_phases.Count - 1]);
            Assert.AreEqual(1, _phases.FindAll(p => p == GamePhase.Dawn).Count, "Son geceden sonra şafak yok.");
        }

        [Test]
        public void SingleNight_FirstCompletion_IsVictory()
        {
            _machine = Create(1);
            DriveToNight();

            _machine.NotifyNightCompleted();

            Assert.AreEqual(GamePhase.Victory, _machine.Phase);
        }

        [Test]
        public void Victory_IsFinal()
        {
            _machine = Create(1);
            DriveToNight();
            _machine.NotifyNightCompleted();
            int phaseCount = _phases.Count;

            Assert.IsFalse(_machine.NotifyGeneratorDestroyed());
            Assert.IsFalse(_machine.RequestDawnContinue());
            Assert.IsFalse(_machine.RequestReady());
            _machine.Tick(1000f);

            Assert.AreEqual(GamePhase.Victory, _machine.Phase);
            Assert.AreEqual(phaseCount, _phases.Count);
        }

        // --- Oyun bitti ---

        [TestCase(GamePhase.Boot)]
        [TestCase(GamePhase.Day)]
        [TestCase(GamePhase.Dusk)]
        [TestCase(GamePhase.Night)]
        [TestCase(GamePhase.Dawn)]
        public void GeneratorDestroyed_EntersGameOver_FromAnyPlayingPhase(GamePhase phase)
        {
            DriveTo(phase);
            Assert.AreEqual(phase, _machine.Phase);

            Assert.IsTrue(_machine.NotifyGeneratorDestroyed());

            Assert.AreEqual(GamePhase.GameOver, _machine.Phase);
            Assert.IsTrue(_machine.IsOver);
            Assert.AreEqual(0f, _machine.PhaseTimeRemaining, Tolerance);
        }

        [Test]
        public void GameOver_IsFinal()
        {
            DriveToNight();
            _machine.NotifyGeneratorDestroyed();
            int phaseCount = _phases.Count;

            Assert.IsFalse(_machine.NotifyGeneratorDestroyed());
            Assert.IsFalse(_machine.NotifyNightCompleted());
            Assert.IsFalse(_machine.RequestDawnContinue());
            Assert.IsFalse(_machine.RequestReady());
            Assert.IsFalse(_machine.Begin());
            _machine.Tick(1000f);

            Assert.AreEqual(GamePhase.GameOver, _machine.Phase);
            Assert.AreEqual(phaseCount, _phases.Count);
            Assert.AreEqual(1, _startedNights.Count);
        }

        [Test]
        public void GameOver_DuringDay_StopsDayTimer()
        {
            _machine.Begin();
            _machine.NotifyGeneratorDestroyed();

            _machine.Tick(200f);

            Assert.AreEqual(1, _timerTicks.Count);
            Assert.IsEmpty(_endingSoon);
        }

        [Test]
        public void GameOver_RaisedWhileEnteringNight_DoesNotStartNight()
        {
            _machine.PhaseChanged += () =>
            {
                if (_machine.Phase == GamePhase.Night)
                {
                    _machine.NotifyGeneratorDestroyed();
                }
            };

            DriveToNight();

            Assert.AreEqual(GamePhase.GameOver, _machine.Phase);
            Assert.IsEmpty(_startedNights);
        }

        // --- Yardımcılar ---

        private GameStateMachine Create(int totalNights, GameFlowSettings? settings = null)
        {
            var machine = new GameStateMachine(settings ?? DefaultSettings, totalNights, index => _startedNights.Add(index));
            machine.PhaseChanged += () => _phases.Add(machine.Phase);
            machine.DayTimerTicked += seconds => _timerTicks.Add(seconds);
            machine.DayEndingSoon += remaining => _endingSoon.Add(remaining);
            machine.EarlyReady += (remaining, bonus) => _earlyReady.Add((remaining, bonus, machine.Phase));
            return machine;
        }

        private void DriveToNight()
        {
            _machine.Begin();
            _machine.RequestReady();
            _machine.Tick(4f);
        }

        /// <summary>Gündüzden (Begin sonrası) şafağa.</summary>
        private void DriveDayToDawn()
        {
            _machine.RequestReady();
            _machine.Tick(4f);
            _machine.NotifyNightCompleted();
        }

        private void DriveTo(GamePhase phase)
        {
            if (phase == GamePhase.Boot)
            {
                return;
            }

            _machine.Begin();
            if (phase == GamePhase.Day)
            {
                return;
            }

            _machine.RequestReady();
            if (phase == GamePhase.Dusk)
            {
                return;
            }

            _machine.Tick(4f);
            if (phase == GamePhase.Night)
            {
                return;
            }

            _machine.NotifyNightCompleted();
        }
    }
}
