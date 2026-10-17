using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un saldırı yönü okları (GDD §9 istihbarat): gündüz, bu gecenin her saldırı yönü için ekran kenarında o yönün spawn
    /// bölgesine dönen kırmızı ok + yön harfi (K/D/G/B, <c>HUD.uxml</c>'de). Bölge ekrandaysa (ışık sütunu görünür) ok gizlenir.
    /// Yönler gündüz başında <see cref="TonightIntel"/>'den, bölge merkezleri <see cref="IWaveService.TryGetSpawnZoneCenter"/>'dan;
    /// konum her kare kameradan sonra (<see cref="EdgeIndicatorView.ShowToward"/>). HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(HudExecutionOrder.AfterCamera)]
    public sealed class HudAttackIndicatorsController : UIDocumentController
    {
        private static readonly int DirectionCount = HudElementNames.AttackIndicatorsByDirection.Length;

        private readonly VisualElement[] _elements = new VisualElement[DirectionCount];
        private readonly EdgeIndicatorView[] _views = new EdgeIndicatorView[DirectionCount];
        private readonly Vector3[] _targets = new Vector3[DirectionCount];
        private readonly bool[] _active = new bool[DirectionCount];
        private VisualElement _container;
        private IWaveService _waves;
        private ICameraService _camera;
        private IGameStateService _gameState;
        private bool _anyActive;
        private bool _started;

        protected override void BindElements()
        {
            _container = Require<VisualElement>(HudElementNames.AttackIndicators);
            for (int i = 0; i < DirectionCount; i++)
            {
                _elements[i] = Require<VisualElement>(HudElementNames.AttackIndicatorsByDirection[i]);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            for (int i = 0; i < DirectionCount; i++)
            {
                var view = new EdgeIndicatorView(_elements[i]);
                if (!view.IsValid)
                {
                    Debug.LogError($"HudAttackIndicatorsController: '{HudElementNames.AttackIndicatorsByDirection[i]}' içinde " +
                                   $"'{EdgeIndicatorView.PivotClass}' veya '{EdgeIndicatorView.ArrowClass}' yok.", this);
                    view.Detach();
                    view = null;
                }

                view?.Hide();
                _views[i] = view;
            }

            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            if (_started)
            {
                ShowCurrent();
            }
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            for (int i = 0; i < DirectionCount; i++)
            {
                _views[i]?.Detach();
                _views[i] = null;
            }
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (!ServiceLocator.TryGet(out _waves))
            {
                Debug.LogWarning("HudAttackIndicatorsController: IWaveService bulunamadı; saldırı okları çıkmayacak.", this);
            }

            if (!ServiceLocator.TryGet(out _camera))
            {
                Debug.LogWarning("HudAttackIndicatorsController: ICameraService bulunamadı; saldırı okları çıkmayacak.", this);
            }

            ServiceLocator.TryGet(out _gameState);
            _started = true;
            ShowCurrent();
        }

        private void LateUpdate()
        {
            if (!_anyActive)
            {
                return;
            }

            Camera camera = _camera?.Camera;
            Vector2 screen = _container.layout.size;
            bool canPlace = camera != null && !float.IsNaN(screen.x) && !float.IsNaN(screen.y);
            for (int i = 0; i < DirectionCount; i++)
            {
                EdgeIndicatorView view = _views[i];
                if (view == null || !_active[i])
                {
                    continue;
                }

                Vector3 viewport = canPlace ? camera.WorldToViewportPoint(_targets[i]) : default;
                if (!canPlace || EdgeIndicatorMath.IsOnScreen(viewport))
                {
                    view.Hide();
                    continue;
                }

                view.ShowToward(viewport, screen);
            }
        }

        private void ShowCurrent()
        {
            if (_gameState != null)
            {
                SetTargets(TonightIntel.Get(_waves, _gameState.CurrentPhase, _gameState.NightIndex));
            }
            else
            {
                SetTargets(null);
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            SetTargets(TonightIntel.Get(_waves, changed.Phase, changed.NightIndex));
        }

        private void SetTargets(NightInfo tonight)
        {
            _anyActive = false;
            for (int i = 0; i < DirectionCount; i++)
            {
                var direction = (CompassDirection)i;
                _active[i] = tonight != null && tonight.HasDirection(direction) && _waves.TryGetSpawnZoneCenter(direction, out _targets[i]);
                _anyActive |= _active[i];
                if (!_active[i])
                {
                    _views[i]?.Hide();
                }
            }
        }
    }
}
