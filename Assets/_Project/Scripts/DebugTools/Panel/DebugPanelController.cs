using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.DebugTools
{
    /// <summary>
    /// [Systems]/DebugPanel: tüm debug kısayollarını toplayan panel (15.1; F12 aç/kapa, sadece editör ve development build). Düğmelerin
    /// işleri <see cref="DebugPanelCommands"/>'ta (çoğu mevcut kısayol bileşenlerini çağırır; kısayollar panel kapalıyken de çalışır),
    /// durum satırları <see cref="DebugPanelStatus"/>'ta (gerçek zamanla <see cref="RefreshInterval"/>'da bir yenilenir). Bu bileşen
    /// paneli kurar, gösterir/gizler ve açıkken oyuncu girdisini UI'a alır (<see cref="DebugPanelInputScope"/>). Sürüm build'inde obje
    /// kendini kapatır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugPanelController : MonoBehaviour
    {
        [Header("Bağlantılar")]
        [SerializeField, Tooltip("Panelin UIDocument'ı (aynı obje, UI/Debug/DebugPanel.uxml). Sahne adımı bağlar.")]
        private UIDocument _document;

        [SerializeField, Tooltip("F1–F4 ([Systems]/DebugSpawner). Sahne adımı bağlar.")]
        private DebugSpawner _spawner;

        [SerializeField, Tooltip("F5/F6 ve N. geceye git ([Systems]/DebugWaves). Sahne adımı bağlar.")]
        private DebugWaveControls _waveControls;

        [SerializeField, Tooltip("F7/F8 ([Systems]/DebugEconomy). Sahne adımı bağlar.")]
        private DebugEconomyControls _economy;

        [SerializeField, Tooltip("F10 ve perk verme ([Systems]/DebugPerks). Sahne adımı bağlar.")]
        private DebugPerkControls _perkControls;

        [SerializeField, Tooltip("Stres testi ([Systems]/DebugStress; 15.2). Sahne adımı bağlar.")]
        private DebugStressTest _stress;

        [SerializeField, Tooltip("Perk listesinin kaynağı (Data/Progression/PerkCatalog.asset). Sahne adımı bağlar.")]
        private PerkCatalog _perkCatalog;

        [Header("Ayarlar")]
        [SerializeField, Tooltip("Zaman ölçeği düğmeleri (sırasıyla).")]
        private float[] _timeScales = { 0.5f, 1f, 2f, 4f };

        [SerializeField, Min(0.1f), Tooltip("FPS ölçüm penceresi ve durum satırlarının yenilenme aralığı (sn, gerçek zaman).")]
        private float _refreshInterval = 0.5f;

        /// <summary>Zaman ölçeği seçenekleri.</summary>
        public IReadOnlyList<float> TimeScales => _timeScales;

        /// <summary>Durum satırlarının yenilenme aralığı (sn).</summary>
        public float RefreshInterval => _refreshInterval;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly DebugPanelInputScope _inputScope = new DebugPanelInputScope();
        private DebugPanelCommands _commands;
        private DebugPanelStatus _status;
        private DebugPanelView _view;
        private InputAction _toggleAction;
        private bool _started;
        private bool _shown;

        /// <summary>Panel açık mı.</summary>
        public bool IsShown => _shown;

        /// <summary>F12: paneli açar/kapar.</summary>
        public void Toggle()
        {
            if (_view == null)
            {
                Debug.LogWarning("[Debug] Debug paneli bağlanamadı (UIDocument/UXML); Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            SetShown(!_shown);
        }

        private void Awake()
        {
            _status = new DebugPanelStatus(_waveControls, _stress, _refreshInterval);
            _commands = new DebugPanelCommands(_spawner, _waveControls, _economy, _perkControls, _stress, _perkCatalog, this, RefreshStatus);
            _toggleAction = DebugHotkeys.Create("<Keyboard>/f12", Toggle);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out IInputReader input);
            _inputScope.SetInput(input);
            ServiceLocator.TryGet(out IGameStateService gameState);
            ServiceLocator.TryGet(out IWaveService waves);
            if (!ServiceLocator.TryGet(out ITimeScaleService timeScale))
            {
                Debug.LogWarning("DebugPanelController: ITimeScaleService yok; zaman ölçeği düğmeleri çalışmayacak. Sahnede [Systems]/Pause var mı?", this);
            }

            Health playerHealth = ServiceLocator.TryGet(out IPlayer player) && player.Transform != null
                ? player.Transform.GetComponent<Health>()
                : null;
            _status.SetServices(gameState, waves, timeScale);
            _commands.SetServices(timeScale, playerHealth);
            _started = true;
            Populate();
        }

        private void OnEnable()
        {
            _toggleAction.Enable();
            _status.Subscribe();
            BindView();
            if (_started)
            {
                Populate();
            }
        }

        private void OnDisable()
        {
            SetShown(false);
            _toggleAction.Disable();
            _status.Unsubscribe();
            _commands.Unbind();
            _view = null;
        }

        private void OnDestroy()
        {
            _toggleAction.Dispose();
            _commands.Dispose();
            _status.End();
        }

        private void Update()
        {
            if (_shown && _status.Tick(Time.unscaledDeltaTime))
            {
                RefreshStatus();
            }
        }

        private void BindView()
        {
            VisualElement root = _document != null ? _document.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogError("DebugPanelController: UIDocument veya UXML atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            var view = new DebugPanelView(root);
            if (view.MissingElement != null)
            {
                Debug.LogError($"DebugPanelController: UXML'de '{view.MissingElement}' bulunamadı.", this);
                return;
            }

            _view = view;
            _view.SetShown(_shown);
            _commands.Bind(_view);
        }

        /// <summary>Servislere ve veriye bağlı içerik: miktar metinleri, gece/ölçek düğmeleri, perk listesi, ölümsüzlük kutusu.</summary>
        private void Populate()
        {
            if (_view == null)
            {
                return;
            }

            _view.FormatAmounts(
                _economy != null ? _economy.MoneyAmount : 0,
                _economy != null ? _economy.FuelAmount : 0,
                _economy != null ? _economy.LowFuelAmount : 0,
                _spawner != null ? _spawner.EdgeCount : 0);
            _view.FormatStress(_stress != null ? _stress.EnemyTarget : 0, _stress != null ? _stress.TurretTarget : 0);
            _view.BuildNightButtons(_waveControls != null ? _waveControls.TotalNights : 0, _commands.GoToNight);
            _view.BuildTimeScaleButtons(_timeScales, _commands.SetTimeScale);
            _view.SetPerkChoices(_commands.PerkNames);
            _view.Invulnerable.SetValueWithoutNotify(_commands.IsInvulnerable);
            RefreshStatus();
        }

        private void SetShown(bool shown)
        {
            if (_shown == shown)
            {
                return;
            }

            _shown = shown;
            _view?.SetShown(shown);
            if (shown)
            {
                _inputScope.Capture();
                _status.Begin();
                RefreshStatus();
            }
            else
            {
                _inputScope.Release();
                _status.End();
            }
        }

        private void RefreshStatus()
        {
            if (_view != null && _shown)
            {
                _status.WriteTo(_view);
            }
        }
#else
        // Sürüm build'i: panel hiç açılmaz, UIDocument'ı da kurulmasın.
        private void Awake()
        {
            gameObject.SetActive(false);
        }
#endif
    }
}
