using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/DawnPanel: şafak ekranı (GDD §16). <see cref="DawnEvent"/>'te biten gecenin istatistikleriyle
    /// (<see cref="IRunStats"/>: öldürme, toplanan para, en yüksek kombo, faiz) açılır; "Devam" butonu
    /// <see cref="DawnContinueRequestedEvent"/> gönderir. Şafak dışındaki ilk evrede kapanır (Enter ile devam edilse de).
    /// Faiz de <see cref="DawnEvent"/> işlenirken ödendiği için ayrıca <see cref="InterestPaidEvent"/>'ten yazılır
    /// (dinleyici sırası belirsiz).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DawnPanelController : UIScreen
    {
        private Label _nightValue;
        private Label _nightKillsValue;
        private Label _nightMoneyValue;
        private Label _nightBestComboValue;
        private Label _interestValue;
        private Label _totalKillsValue;
        private Button _continueButton;

        private IRunStats _stats;
        private int _totalNights;

        protected override void BindElements()
        {
            _nightValue = Require<Label>(PanelElementNames.DawnNightValue);
            _nightKillsValue = Require<Label>(PanelElementNames.NightKillsValue);
            _nightMoneyValue = Require<Label>(PanelElementNames.NightMoneyValue);
            _nightBestComboValue = Require<Label>(PanelElementNames.NightBestComboValue);
            _interestValue = Require<Label>(PanelElementNames.InterestValue);
            _totalKillsValue = Require<Label>(PanelElementNames.TotalKillsValue);
            _continueButton = Require<Button>(PanelElementNames.ContinueButton);
        }

        private void Awake()
        {
            // Oyun şafakta başlamaz; panel ilk DawnEvent'e kadar gizli.
            SetShown(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _continueButton.clicked += OnContinueClicked;
            EventBus<DawnEvent>.Subscribe(OnDawn);
            EventBus<InterestPaidEvent>.Subscribe(OnInterestPaid);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
        }

        private void OnDisable()
        {
            if (_continueButton != null)
            {
                _continueButton.clicked -= OnContinueClicked;
            }

            EventBus<DawnEvent>.Unsubscribe(OnDawn);
            EventBus<InterestPaidEvent>.Unsubscribe(OnInterestPaid);
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _stats))
            {
                Debug.LogWarning("DawnPanelController: IRunStats bulunamadı; istatistikler 0 görünecek. [Systems]/GameFlow'da StatsTrackerHost var mı?", this);
            }

            if (ServiceLocator.TryGet(out IWaveService waves))
            {
                _totalNights = waves.TotalNights;
            }
        }

        private void OnDawn(DawnEvent dawn)
        {
            _nightValue.text = HudFormat.OfTotal(dawn.NightIndex + 1, _totalNights);
            _nightKillsValue.text = HudFormat.Count(_stats != null ? _stats.NightKills : 0);
            _nightMoneyValue.text = HudFormat.Count(_stats != null ? _stats.NightMoneyEarned : 0);
            _nightBestComboValue.text = HudFormat.Count(_stats != null ? _stats.NightBestCombo : 0);
            _interestValue.text = HudFormat.Gain(_stats != null ? _stats.LastInterest : 0);
            _totalKillsValue.text = HudFormat.Count(_stats != null ? _stats.TotalKills : 0);
            SetShown(true);
        }

        private void OnInterestPaid(InterestPaidEvent paid)
        {
            _interestValue.text = HudFormat.Gain(paid.Amount);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (changed.Phase != GamePhase.Dawn)
            {
                SetShown(false);
            }
        }

        private void OnContinueClicked()
        {
            EventBus<DawnContinueRequestedEvent>.Raise(new DawnContinueRequestedEvent());
        }
    }
}
