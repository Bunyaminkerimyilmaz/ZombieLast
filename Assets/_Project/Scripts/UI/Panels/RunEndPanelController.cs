using LastHaven.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/GameOverPanel ve VictoryPanel: run sonu ekranı (GDD §15). <see cref="_showOnPhase"/> evresine geçilince
    /// ulaşılan geceyi ve toplam öldürmeyi (<see cref="IRunStats"/>) gösterir. "Yeniden Başlat" etkin sahneyi yeniden yükler (yeni run).
    /// Ayrıntılı istatistikler ve Ana Menü butonu 14.3'te.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunEndPanelController : UIScreen
    {
        [SerializeField, Tooltip("Panelin açıldığı evre: GameOver (jeneratör yıkıldı) veya Victory (son gece atlatıldı).")]
        private GamePhase _showOnPhase = GamePhase.GameOver;

        private Label _nightValue;
        private Label _totalKillsValue;
        private Button _restartButton;

        private IRunStats _stats;
        private int _totalNights;
        private bool _restarting;

        public GamePhase ShowOnPhase => _showOnPhase;

        protected override void BindElements()
        {
            _nightValue = Require<Label>(PanelElementNames.RunEndNightValue);
            _totalKillsValue = Require<Label>(PanelElementNames.TotalKillsValue);
            _restartButton = Require<Button>(PanelElementNames.RestartButton);
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
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
        }

        private void OnDisable()
        {
            if (_restartButton != null)
            {
                _restartButton.clicked -= OnRestartClicked;
            }

            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _stats))
            {
                Debug.LogWarning($"{name}: IRunStats bulunamadı; öldürme sayısı 0 görünecek.", this);
            }

            if (ServiceLocator.TryGet(out IWaveService waves))
            {
                _totalNights = waves.TotalNights;
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (changed.Phase != _showOnPhase)
            {
                return;
            }

            _nightValue.text = HudFormat.OfTotal(changed.NightIndex + 1, _totalNights);
            _totalKillsValue.text = HudFormat.Count(_stats != null ? _stats.TotalKills : 0);
            SetShown(true);
        }

        private void OnRestartClicked()
        {
            if (_restarting)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError($"{name}: '{scene.name}' Build Settings'te değil; yeniden yüklenemiyor. Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            _restarting = true;
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
