using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/GameOverPanel ve VictoryPanel: run sonu ekranı (GDD §15). <see cref="_showOnPhase"/> evresine geçilince ulaşılan
    /// geceyi, toplam öldürmeyi, kazanılan parayı, en yüksek komboyu ve alınan perk'leri gösterir (<see cref="RunEndStatsView"/>);
    /// oyun durur (<see cref="IPauseService"/>, kaynak bu panel) ve girdi UI'a geçer (gamepad / klavye ile butonlar gezilir).
    /// "Tekrar Oyna" etkin sahneyi yeniden yükler (yeni run), "Ana Menü" ana menüye döner; duraklatmayı sahne değişimi kaldırır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunEndPanelController : UIScreen
    {
        private const string LogPrefix = "[UI] ";

        [SerializeField, Tooltip("Panelin açıldığı evre: GameOver (jeneratör yıkıldı) veya Victory (son gece atlatıldı).")]
        private GamePhase _showOnPhase = GamePhase.GameOver;

        private RunEndStatsView _view;
        private Button _restartButton;
        private Button _mainMenuButton;

        private IRunStats _stats;
        private IPerkService _perks;
        private IPauseService _pause;
        private ISettingsService _settings;
        private MenuInputScope _menuInput;
        private int _totalNights;
        private bool _leaving;

        public GamePhase ShowOnPhase => _showOnPhase;

        protected override void BindElements()
        {
            _view = new RunEndStatsView(Require<Label>(PanelElementNames.RunEndNightValue), Require<Label>(PanelElementNames.TotalKillsValue),
                Require<Label>(PanelElementNames.TotalMoneyValue), Require<Label>(PanelElementNames.BestComboValue),
                Require<VisualElement>(PanelElementNames.RunEndPerks), Require<VisualElement>(PanelElementNames.RunEndNoPerks));
            _restartButton = Require<Button>(PanelElementNames.RestartButton);
            _mainMenuButton = Require<Button>(PanelElementNames.RunEndMainMenuButton);
        }

        private void Awake()
        {
            SetShown(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _restartButton.clicked += OnRestartClicked;
            _mainMenuButton.clicked += OnMainMenuClicked;
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
        }

        private void OnDisable()
        {
            if (_restartButton != null)
            {
                _restartButton.clicked -= OnRestartClicked;
                _mainMenuButton.clicked -= OnMainMenuClicked;
            }

            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            if (IsShown)
            {
                _menuInput?.Exit();
                if (_pause is Object pauseObject ? pauseObject != null : _pause != null)
                {
                    _pause.Resume(this);
                }
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _stats))
            {
                Debug.LogWarning($"{name}: IRunStats bulunamadı; istatistikler 0 görünecek.", this);
            }

            ServiceLocator.TryGet(out _perks);
            ServiceLocator.TryGet(out _pause);
            ServiceLocator.TryGet(out _settings);
            ServiceLocator.TryGet(out IInputReader input);
            _menuInput = new MenuInputScope(input);
            if (ServiceLocator.TryGet(out IWaveService waves))
            {
                _totalNights = waves.TotalNights;
            }
        }

        // Aynı evre değişiminde başka bir menü (market) kapanırken girdiyi oyuna döndürebilir: panel açıkken UI modu korunur.
        private void Update()
        {
            if (IsShown && _menuInput != null && _menuInput.Mode != InputMode.UI)
            {
                _menuInput.Enter();
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (changed.Phase != _showOnPhase || IsShown)
            {
                return;
            }

            _view.Show(changed.NightIndex + 1, _totalNights, _stats, _perks);
            SetShown(true);
            _pause?.Pause(this);
            _menuInput?.Enter();
            ScreenFocus.FocusLater(ScreenElement, _restartButton);
            Debug.Log($"{LogPrefix}{_showOnPhase} ekranı açıldı (gece {changed.NightIndex + 1}, {_view.PerkCount} perk).", this);
        }

        private void OnRestartClicked()
        {
            if (!_leaving)
            {
                _settings?.Save();
                _leaving = SceneLoader.TryReloadActive(this);
            }
        }

        private void OnMainMenuClicked()
        {
            if (!_leaving)
            {
                _settings?.Save();
                _leaving = SceneLoader.TryLoad(SceneNames.MainMenu, this);
            }
        }
    }
}
