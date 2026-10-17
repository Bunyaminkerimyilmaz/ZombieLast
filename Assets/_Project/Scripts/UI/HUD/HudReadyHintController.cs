using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un "Hazırım" ipucu (GDD §4, §9; evre kutusunun altında): gündüz boyunca tuş (Enter / Start basılı) + "Hazırım" + şu an
    /// basılırsa kazanılacak erken hazır bonusu ("— +60 bonus"). Bonus <see cref="DayTimerTickEvent.EarlyReadyBonus"/>'tan
    /// her saniye gelir; 0 iken bonus kısmı gizlenir. Sadece gündüzde görünür. HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudReadyHintController : UIDocumentController
    {
        private VisualElement _hint;
        private VisualElement _bonus;
        private Label _bonusValue;
        private IGameStateService _gameState;
        private bool _shown;
        private int _bonusMoney;
        private bool _started;

        protected override void BindElements()
        {
            _hint = Require<VisualElement>(HudElementNames.ReadyHint);
            _bonus = Require<VisualElement>(HudElementNames.ReadyBonus);
            _bonusValue = Require<Label>(HudElementNames.ReadyBonusValue);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<DayTimerTickEvent>.Subscribe(OnDayTimerTick);

            // UIDocument ağacı her açılışta yeniden kurulur: son değerler yeniden yazılsın.
            if (_started)
            {
                Apply();
            }
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<DayTimerTickEvent>.Unsubscribe(OnDayTimerTick);
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (ServiceLocator.TryGet(out _gameState))
            {
                _shown = HudPhaseRules.ShowsReadyHint(_gameState.CurrentPhase);
            }

            _started = true;
            Apply();
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            _shown = HudPhaseRules.ShowsReadyHint(changed.Phase);
            if (!_shown)
            {
                _bonusMoney = 0;
            }

            Apply();
        }

        private void OnDayTimerTick(DayTimerTickEvent tick)
        {
            if (tick.EarlyReadyBonus != _bonusMoney)
            {
                _bonusMoney = tick.EarlyReadyBonus;
                Apply();
            }
        }

        private void Apply()
        {
            _hint.SetShown(_shown);
            _bonus.SetShown(_bonusMoney > 0);
            _bonusValue.text = HudFormat.Gain(_bonusMoney);
        }
    }
}
