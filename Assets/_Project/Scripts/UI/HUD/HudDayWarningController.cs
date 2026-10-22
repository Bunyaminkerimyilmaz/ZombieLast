using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Gündüzün son uyarısı (GDD §9: son 15 sn): <see cref="DayEndingSoonEvent"/> gelince ekranın üst ortasında büyük geri sayım ve
    /// ekran kenarında kırmızı nabız; gündüz bitince (ya da yeni gündüz başlayınca) kapanır. Sayı <see cref="DayTimerTickEvent"/>
    /// ile her saniye yazılır ve kısa büyür (<see cref="TickClass"/>, transition bitince kalkar, aynı transition geri küçültür).
    /// Nabız USS transition'ıyla: <see cref="PulseDimClass"/> her transition bitişinde açılıp kapanır (süreler USS'te).
    /// HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudDayWarningController : UIDocumentController
    {
        /// <summary>Nabzın sönük yarısı (<c>HudIntel.uss</c>).</summary>
        public const string PulseDimClass = "hud-day-warning__pulse--dim";

        /// <summary>Saniye değişince sayının kısa büyümesi (<c>HudIntel.uss</c>).</summary>
        public const string TickClass = "hud-day-countdown--tick";

        private VisualElement _warning;
        private VisualElement _pulse;
        private Label _countdown;
        private bool _active;
        private int _seconds;

        protected override void BindElements()
        {
            _warning = Require<VisualElement>(HudElementNames.DayWarning);
            _pulse = Require<VisualElement>(HudElementNames.DayWarningPulse);
            _countdown = Require<Label>(HudElementNames.DayCountdown);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _pulse.RegisterCallback<TransitionEndEvent>(OnPulseTransitionEnd);
            _countdown.RegisterCallback<TransitionEndEvent>(OnCountdownTransitionEnd);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<DayEndingSoonEvent>.Subscribe(OnDayEndingSoon);
            EventBus<DayTimerTickEvent>.Subscribe(OnDayTimerTick);

            // UIDocument ağacı her açılışta yeniden kurulur: süren uyarı yeniden yazılsın.
            Apply();
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<DayEndingSoonEvent>.Unsubscribe(OnDayEndingSoon);
            EventBus<DayTimerTickEvent>.Unsubscribe(OnDayTimerTick);
            _pulse?.UnregisterCallback<TransitionEndEvent>(OnPulseTransitionEnd);
            _countdown?.UnregisterCallback<TransitionEndEvent>(OnCountdownTransitionEnd);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            // Uyarı sadece kendi gündüzünde: gündüz biter ya da yeni gündüz başlarsa kapanır.
            if (_active)
            {
                _active = false;
                Apply();
            }
        }

        private void OnDayEndingSoon(DayEndingSoonEvent endingSoon)
        {
            _active = true;
            _seconds = Mathf.CeilToInt(endingSoon.RemainingSeconds);
            Apply();
        }

        private void OnDayTimerTick(DayTimerTickEvent tick)
        {
            if (!_active || tick.RemainingSeconds == _seconds)
            {
                return;
            }

            _seconds = tick.RemainingSeconds;
            _countdown.text = NumberText.Count(_seconds);
            _countdown.AddToClassList(TickClass);
        }

        private void Apply()
        {
            _warning.SetShown(_active);
            _pulse.RemoveFromClassList(PulseDimClass);
            _countdown.RemoveFromClassList(TickClass);
            _countdown.text = NumberText.Count(_seconds);
            if (_active)
            {
                // Az önce görünür oldu: transition'ın başlaması için sönükleşme bir sonraki karede.
                _pulse.schedule.Execute(StartPulse);
            }
        }

        private void StartPulse()
        {
            if (_active)
            {
                _pulse.AddToClassList(PulseDimClass);
            }
        }

        private void OnPulseTransitionEnd(TransitionEndEvent transitionEnd)
        {
            // Sönme/parlama bitti: tersine çevir. Uyarı kapanınca döngü durur.
            if (_active)
            {
                _pulse.ToggleInClassList(PulseDimClass);
            }
        }

        private void OnCountdownTransitionEnd(TransitionEndEvent transitionEnd)
        {
            // Büyüme bitti: sınıf kalkınca aynı transition geri küçültür.
            _countdown.RemoveFromClassList(TickClass);
        }
    }
}
