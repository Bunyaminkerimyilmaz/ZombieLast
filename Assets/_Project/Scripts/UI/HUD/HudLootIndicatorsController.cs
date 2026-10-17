using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un ganimet sandığı kenar göstergeleri (GDD §9): ekran dışındaki sandıklardan oyuncuya en yakın olanların yönünü ekran
    /// kenarında ok + sandık simgesiyle gösterir. Gösterge sayısı <c>HUD.uxml</c>'deki <see cref="EdgeIndicatorView.IndicatorClass"/>
    /// sayısıdır (3). Sandıklar <see cref="ILootCrateService"/>'ten her kare kameradan sonra okunur (gece sandık yok: hepsi gizli);
    /// hesap <see cref="EdgeIndicatorMath"/> ve <see cref="NearestPicker"/>'da. HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(HudExecutionOrder.AfterCamera)]
    public sealed class HudLootIndicatorsController : UIDocumentController
    {
        private readonly List<Vector3> _crates = new List<Vector3>();
        private readonly List<EdgeIndicatorView> _views = new List<EdgeIndicatorView>();
        private VisualElement _container;
        private NearestPicker _nearest;
        private ILootCrateService _loot;
        private ICameraService _camera;
        private IPlayer _player;

        protected override void BindElements()
        {
            _container = Require<VisualElement>(HudElementNames.LootIndicators);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _views.Clear();
            foreach (VisualElement element in _container.Query(className: EdgeIndicatorView.IndicatorClass).Build())
            {
                var view = new EdgeIndicatorView(element);
                if (!view.IsValid)
                {
                    Debug.LogError($"HudLootIndicatorsController: '{EdgeIndicatorView.IndicatorClass}' içinde " +
                                   $"'{EdgeIndicatorView.PivotClass}' veya '{EdgeIndicatorView.ArrowClass}' yok.", this);
                    view.Detach();
                    continue;
                }

                view.Hide();
                _views.Add(view);
            }

            _nearest = new NearestPicker(_views.Count);
        }

        private void OnDisable()
        {
            for (int i = 0; i < _views.Count; i++)
            {
                _views[i].Detach();
            }

            _views.Clear();
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (!ServiceLocator.TryGet(out _loot))
            {
                Debug.LogWarning("HudLootIndicatorsController: ILootCrateService bulunamadı; sandık göstergeleri çıkmayacak. Sahnede [Systems]/Loot var mı?", this);
            }

            if (!ServiceLocator.TryGet(out _camera))
            {
                Debug.LogWarning("HudLootIndicatorsController: ICameraService bulunamadı; sandık göstergeleri çıkmayacak.", this);
            }

            ServiceLocator.TryGet(out _player);
        }

        private void LateUpdate()
        {
            if (_views.Count == 0)
            {
                return;
            }

            Camera camera = _camera?.Camera;
            if (_loot == null || camera == null || _loot.CrateCount == 0)
            {
                ShowNearest(null, 0);
                return;
            }

            _loot.GetCratePositions(_crates);
            Vector3 origin = _player != null ? _player.Position : camera.transform.position;
            _nearest.Clear();
            for (int i = 0; i < _crates.Count; i++)
            {
                if (!EdgeIndicatorMath.IsOnScreen(camera.WorldToViewportPoint(_crates[i])))
                {
                    _nearest.Offer(i, PlanarDistance.Between(origin, _crates[i]));
                }
            }

            ShowNearest(camera, _nearest.Count);
        }

        private void ShowNearest(Camera camera, int count)
        {
            Vector2 screen = _container.layout.size;
            for (int rank = 0; rank < _views.Count; rank++)
            {
                EdgeIndicatorView view = _views[rank];
                if (rank >= count || float.IsNaN(screen.x) || float.IsNaN(screen.y))
                {
                    view.Hide();
                    continue;
                }

                view.ShowToward(camera.WorldToViewportPoint(_crates[_nearest.GetIndex(rank)]), screen);
            }
        }
    }
}
