using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// <see cref="GameStateMachine"/>'in çıktılarını global olaylara çevirir ve loglar: evre değişimi (<see cref="PhaseChangedEvent"/>
    /// + şafak/oyun bitti/zafer olayları), gündüz sayacı, son uyarı, erken hazır. Makineyle aynı ömürdedir
    /// (<see cref="GameFlowController"/> kurar); makinenin olaylarından çıkmaz.
    /// </summary>
    internal sealed class GameFlowAnnouncer
    {
        private const string LogPrefix = "[GameFlow] ";

        private readonly GameStateMachine _machine;
        private readonly Object _logContext;

        private GameFlowAnnouncer(GameStateMachine machine, Object logContext)
        {
            _machine = machine;
            _logContext = logContext;
            _machine.PhaseChanged += OnPhaseChanged;
            _machine.DayTimerTicked += OnDayTimerTicked;
            _machine.DayEndingSoon += OnDayEndingSoon;
            _machine.EarlyReady += OnEarlyReady;
        }

        /// <summary>Makinenin olaylarına bağlanır; duyurucuyu makinenin olay abonelikleri yaşatır.</summary>
        public static void Attach(GameStateMachine machine, Object logContext)
        {
            _ = new GameFlowAnnouncer(machine, logContext);
        }

        private void OnPhaseChanged()
        {
            GamePhase phase = _machine.Phase;
            int night = _machine.NightIndex;
            Debug.Log($"{LogPrefix}PhaseChangedEvent: {phase} — {Describe(phase, night)}", _logContext);
            EventBus<PhaseChangedEvent>.Raise(new PhaseChangedEvent(phase, night, _machine.PhaseDuration));

            switch (phase)
            {
                case GamePhase.Dawn:
                    EventBus<DawnEvent>.Raise(new DawnEvent(night));
                    break;
                case GamePhase.GameOver:
                    EventBus<GameOverEvent>.Raise(new GameOverEvent(night));
                    break;
                case GamePhase.Victory:
                    EventBus<VictoryEvent>.Raise(new VictoryEvent(night));
                    break;
            }
        }

        private void OnDayTimerTicked(int remainingSeconds)
        {
            EventBus<DayTimerTickEvent>.Raise(new DayTimerTickEvent(remainingSeconds, _machine.EarlyReadyBonus));
        }

        private void OnDayEndingSoon(float remainingSeconds)
        {
            Debug.Log($"{LogPrefix}DayEndingSoonEvent: gündüzün son {Mathf.CeilToInt(remainingSeconds)} sn'si.", _logContext);
            EventBus<DayEndingSoonEvent>.Raise(new DayEndingSoonEvent(remainingSeconds));
        }

        private void OnEarlyReady(float remainingSeconds, int bonusMoney)
        {
            Debug.Log($"{LogPrefix}EarlyReadyEvent: gündüz {Mathf.CeilToInt(remainingSeconds)} sn erken bitti, bonus +{bonusMoney} para.", _logContext);
            EventBus<EarlyReadyEvent>.Raise(new EarlyReadyEvent(remainingSeconds, bonusMoney));
        }

        /// <summary>Log açıklaması (sadece evre değişiminde kurulur).</summary>
        private string Describe(GamePhase phase, int night)
        {
            string nightLabel = $"Gece {night + 1}/{_machine.TotalNights}";
            switch (phase)
            {
                case GamePhase.Day:
                    return $"{nightLabel} öncesi gündüz, {_machine.PhaseDuration:0} sn. Enter = Hazırım.";
                case GamePhase.Dusk:
                    return $"alacakaranlık, {_machine.PhaseDuration:0} sn sonra {nightLabel}.";
                case GamePhase.Night:
                    return $"{nightLabel} başlıyor.";
                case GamePhase.Dawn:
                    return $"{nightLabel} atlatıldı. Devam için Enter.";
                case GamePhase.GameOver:
                    return $"jeneratör yıkıldı, oyun bitti ({nightLabel}).";
                case GamePhase.Victory:
                    return $"{nightLabel} atlatıldı, tahliye! Zafer.";
                default:
                    return nightLabel;
            }
        }
    }
}
