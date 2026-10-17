using LastHaven.Building;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un yapı ipucu (GDD §16 "yapı üzerine gelince tooltip"): inşa modunda üzerine gelinen yapının tepesinde ad, seviye, can,
    /// yükseltme fiyatı (U) ve satış iadesi (X). Kaynak <see cref="StructureHoveredEvent"/> (yükseltmeden sonra yeni değerlerle
    /// tekrar gelir); can yapının <see cref="Health"/>'inden canlı (<see cref="HealthBarBinding"/>), para yetmezliği
    /// <see cref="MoneyChangedEvent"/>'ten. Kutu her kare kameradan sonra yapının üstüne taşınır (<see cref="WorldAnchor"/>).
    /// HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(HudExecutionOrder.AfterCamera)]
    public sealed class HudStructureTooltipController : UIDocumentController
    {
        private readonly HealthBarBinding _health = new HealthBarBinding();
        private VisualElement _tooltip;
        private StructureTooltipView _view;
        private WorldAnchor _anchor;
        private Structure _structure;
        private ICameraService _camera;
        private IWallet _wallet;

        protected override void BindElements()
        {
            _tooltip = Require<VisualElement>(HudElementNames.StructureTooltip);
            var healthBar = new BarView(
                Require<VisualElement>(HudElementNames.TooltipHealthFill), Require<Label>(HudElementNames.TooltipHealthValue));
            _view = new StructureTooltipView(
                _tooltip,
                Require<Label>(HudElementNames.TooltipName),
                Require<Label>(HudElementNames.TooltipLevelName),
                Require<Label>(HudElementNames.TooltipLevel),
                healthBar,
                Require<VisualElement>(HudElementNames.TooltipUpgrade),
                Require<Label>(HudElementNames.TooltipUpgradeCost),
                Require<VisualElement>(HudElementNames.TooltipMaxLevel),
                Require<Label>(HudElementNames.TooltipSellRefund));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _anchor = new WorldAnchor(_tooltip);
            Hide();
            EventBus<StructureHoveredEvent>.Subscribe(OnStructureHovered);
            EventBus<BuildModeChangedEvent>.Subscribe(OnBuildModeChanged);
            EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
        }

        private void OnDisable()
        {
            EventBus<StructureHoveredEvent>.Unsubscribe(OnStructureHovered);
            EventBus<BuildModeChangedEvent>.Unsubscribe(OnBuildModeChanged);
            EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
            _health.Detach();
            _structure = null;
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (!ServiceLocator.TryGet(out _camera))
            {
                Debug.LogWarning("HudStructureTooltipController: ICameraService bulunamadı; yapı ipucu konumlanamayacak.", this);
            }

            ServiceLocator.TryGet(out _wallet);
        }

        private void LateUpdate()
        {
            if (_structure == null)
            {
                return;
            }

            if (!_structure.IsPlaced)
            {
                Hide();
                return;
            }

            _anchor.Place(_camera?.Camera, _structure.Bounds);
        }

        private void OnStructureHovered(StructureHoveredEvent hovered)
        {
            Structure structure = null;
            if (!hovered.HasStructure || !hovered.Structure.TryGetComponent(out structure) || structure.Data == null)
            {
                Hide();
                return;
            }

            _structure = structure;
            _view.Show(structure.Data.DisplayName, structure.CurrentLevel?.DisplayName, hovered.Level, hovered.MaxLevel,
                hovered.UpgradeCost, hovered.SellRefund);
            if (_wallet != null)
            {
                _view.SetMoney(_wallet.Money);
            }

            _health.Attach(structure.Health, _view.HealthBar);
            _tooltip.SetShown(true);
        }

        private void OnBuildModeChanged(BuildModeChangedEvent changed)
        {
            if (!changed.IsActive)
            {
                Hide();
            }
        }

        private void OnMoneyChanged(MoneyChangedEvent changed)
        {
            if (_structure != null)
            {
                _view.SetMoney(changed.Money);
            }
        }

        private void Hide()
        {
            _structure = null;
            _health.Detach();
            _tooltip.SetShown(false);
        }
    }
}
