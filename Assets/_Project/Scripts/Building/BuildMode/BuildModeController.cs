using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa modu (GDD §11): sadece gündüz, B ile açılır/kapanır (<see cref="InputMode.BuildMode"/>). 1–5 (gamepad: D-pad)
    /// yapı seçer, Q döndürür; imlecin altındaki hücrede yarı saydam önizleme (yeşil/kırmızı, para dahil). Sol tık parayı
    /// ödeyip yerleştirir; sağ tık seçimi, seçim yoksa modu iptal eder. Var olan yapının üzerine gelince vurgulanır; U yükseltir,
    /// X satar. Taret seçiliyken ve üzerine gelince menzil dairesi görünür. Gündüz bitince (alacakaranlık) kendiliğinden kapanır.
    /// Bu bileşen mod, evre, B ve iptali yönetir; yapı tuşları mod açıkken <see cref="BuildPlacementTool"/>'a bağlanır, ızgara
    /// <see cref="BuildSite"/>'ta.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BuildModeController : MonoBehaviour
    {
        private const string LogPrefix = "[Building] ";

        [SerializeField, Tooltip("Izgara ve yerleştirme. Üretici bağlar (aynı obje).")]
        private BuildSite _site;

        [SerializeField, Tooltip("Önizlemenin kökü (başta kapalı). Üretici kurar.")]
        private Transform _ghostRoot;

        [SerializeField, Tooltip("Önizlemenin zemin plakası (yatırılmış 1×1 quad). Boş olabilir.")]
        private Renderer _ghostFootprint;

        [SerializeField, Tooltip("Üzerine gelinen yapının vurgu kabuğunun kökü (başta kapalı). Boşsa vurgu yok. Üretici kurar.")]
        private Transform _highlightRoot;

        [SerializeField, Tooltip("Menzil dairesi (taret; başta kapalı, X'te 90° yatırılmış LineRenderer). Boşsa daire yok. Üretici kurar.")]
        private LineRenderer _rangeIndicator;

        private BuildPlacementTool _tool;
        private IInputReader _input;
        private IGameStateService _gameState;
        private bool _started;

        /// <summary>İnşa modu açık mı.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Seçili yapı ve dönüş; Start'a kadar null.</summary>
        public BuildSelection Selection => _tool?.Selection;

        /// <summary>Üzerine gelinen yapı (vurgu, U/X); Start'a kadar null.</summary>
        public StructureHoverTool Hover => _tool?.Hover;

        private bool IsInputAlive => _input is Object unityObject ? unityObject != null : _input != null;

        private void Awake()
        {
            if (_site == null || _ghostRoot == null)
            {
                Debug.LogError("BuildModeController: BuildSite veya önizleme kökü atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (!_site.IsReady || !ServiceLocator.TryGet(out _input))
            {
                Debug.LogWarning("BuildModeController: inşa alanı hazır değil veya IInputReader yok; inşa modu çalışmayacak.", this);
                enabled = false;
                return;
            }

            ServiceLocator.TryGet(out _gameState);
            ServiceLocator.TryGet(out IPlayer player);
            if (!ServiceLocator.TryGet(out ICameraService camera))
            {
                Debug.LogWarning("BuildModeController: ICameraService bulunamadı; fareyle yerleşim çalışmayacak.", this);
            }

            BuildConfig config = _site.Config;
            float cellSize = _site.Grid.CellSize;
            var ghost = new BuildGhost(_ghostRoot, _ghostFootprint, config.GhostMaterial, cellSize);
            BuildGhost highlight = _highlightRoot != null
                ? new BuildGhost(_highlightRoot, null, config.GhostMaterial, cellSize, config.HighlightScale)
                : null;
            for (int i = 0; i < config.Slots.Count; i++)
            {
                ghost.Prepare(config.Slots[i]);
                highlight?.Prepare(config.Slots[i]);
            }

            ServiceLocator.TryGet(out IRunModifiers modifiers);
            var range = new BuildRangePreview(_rangeIndicator, modifiers, config.RangeColor, config.RangeLineWidth, config.RangeSegments);
            var pointer = new BuildPointer(_input, camera, player, config.GamepadPlacementDistance);
            var hover = new StructureHoverTool(_site, highlight, pointer, config.HighlightColor);
            _tool = new BuildPlacementTool(_site, new BuildSelection(config.Slots), ghost, pointer, hover, range);

            _started = true;
            SubscribeInput();
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);

            // Servis Start'ta alınır; ilk aboneliği Start yapar.
            if (_started)
            {
                SubscribeInput();
            }
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            if (IsActive)
            {
                Exit();
            }

            if (_input != null)
            {
                _input.BuildModePressed -= OnBuildModePressed;
                _input.CancelPressed -= OnCancelPressed;
            }
        }

        private void Update()
        {
            if (IsActive && _input.Mode == InputMode.BuildMode) // Duraklatma menüsü açıkken (UI) önizleme beklemede.
            {
                _tool.Tick();
            }
        }

        private void SubscribeInput()
        {
            _input.BuildModePressed += OnBuildModePressed;
            _input.CancelPressed += OnCancelPressed;
        }

        private void OnBuildModePressed()
        {
            if (IsActive)
            {
                Exit();
            }
            else if (_input.Mode == InputMode.Gameplay)
            {
                TryEnter();
            }
        }

        private void TryEnter()
        {
            if (_gameState != null && _gameState.CurrentPhase != GamePhase.Day)
            {
                Debug.Log($"{LogPrefix}İnşa modu sadece gündüz açılır.", this);
                return;
            }

            IsActive = true;
            _tool.Bind(_input);
            _input.SetMode(InputMode.BuildMode);
            EventBus<BuildModeChangedEvent>.Raise(new BuildModeChangedEvent(true));
            Debug.Log($"{LogPrefix}İnşa modu açıldı.", this);
        }

        private void Exit()
        {
            if (_tool != null)
            {
                _tool.Unbind(_input);
                _tool.Reset();
            }

            IsActive = false;
            if (IsInputAlive && _input.Mode == InputMode.BuildMode)
            {
                _input.SetMode(InputMode.Gameplay);
            }

            EventBus<BuildModeChangedEvent>.Raise(new BuildModeChangedEvent(false));
            Debug.Log($"{LogPrefix}İnşa modu kapandı.", this);
        }

        /// <summary>Sağ tık / East: önce seçimi bırakır; seçim yoksa inşa modundan çıkar. UI'daki iptal (Esc) menünün.</summary>
        private void OnCancelPressed()
        {
            if (IsActive && _input.Mode == InputMode.BuildMode && !_tool.ClearSelection())
            {
                Exit();
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (IsActive && changed.Phase != GamePhase.Day)
            {
                Debug.Log($"{LogPrefix}Gündüz bitti ({changed.Phase}); inşa modu kapanıyor.", this);
                Exit();
            }
        }
    }
}
