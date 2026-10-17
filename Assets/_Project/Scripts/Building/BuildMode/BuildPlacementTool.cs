using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa modunun araçları: "seç → önizle → yerleştir" (yapı seçimi <see cref="BuildSelection"/>, önizleme
    /// <see cref="BuildGhost"/>, yerleşim noktası <see cref="BuildPointer"/>, <see cref="BuildSite"/>), var olan yapının
    /// üzerine gelme / yükseltme / satış (<see cref="StructureHoverTool"/>) ve menzil dairesi (<see cref="BuildRangePreview"/>). Mod, evre ve iptal <see cref="BuildModeController"/>'da;
    /// kontrolcü mod açılınca girdiyi <see cref="Bind"/> ile bağlar, kapanınca <see cref="Unbind"/> ile çözer. Seçim değişince
    /// <see cref="BuildSelectionChangedEvent"/> gönderir.
    /// </summary>
    /// <remarks>İmleç bir yapının üzerindeyken önizleme gizlenir ve sol tık yerleştirmez (U/X o yapıya uygulanır).</remarks>
    public sealed class BuildPlacementTool
    {
        private const string LogPrefix = "[Building] ";

        private readonly BuildSite _site;
        private readonly BuildSelection _selection;
        private readonly BuildGhost _ghost;
        private readonly BuildPointer _pointer;
        private readonly StructureHoverTool _hover;
        private readonly BuildRangePreview _range;
        private readonly Color _validColor;
        private readonly Color _invalidColor;

        public BuildPlacementTool(
            BuildSite site, BuildSelection selection, BuildGhost ghost, BuildPointer pointer, StructureHoverTool hover, BuildRangePreview range)
        {
            _site = site;
            _selection = selection;
            _ghost = ghost;
            _pointer = pointer;
            _hover = hover;
            _range = range;
            _validColor = site.Config.ValidColor;
            _invalidColor = site.Config.InvalidColor;
        }

        public BuildSelection Selection => _selection;

        public StructureHoverTool Hover => _hover;

        /// <summary>İnşa modunun yapı tuşlarını (1–5, D-pad, Q, sol tık, U, X) bu araca bağlar.</summary>
        public void Bind(IInputReader input)
        {
            input.BuildSlotPressed += SelectSlot;
            input.BuildCyclePressed += Cycle;
            input.RotatePressed += Rotate;
            input.ConfirmPressed += Confirm;
            input.UpgradePressed += Upgrade;
            input.SellPressed += Sell;
        }

        public void Unbind(IInputReader input)
        {
            input.BuildSlotPressed -= SelectSlot;
            input.BuildCyclePressed -= Cycle;
            input.RotatePressed -= Rotate;
            input.ConfirmPressed -= Confirm;
            input.UpgradePressed -= Upgrade;
            input.SellPressed -= Sell;
        }

        /// <summary>
        /// Her kare: imlecin altındaki yapıyı günceller; yapı yoksa ve seçim varsa önizlemeyi imlecin altına taşır ve rengini
        /// yerleşim sonucuna göre ayarlar. Menzilli yapıda (taret) önizlemenin veya üzerine gelinen yapının menzil dairesi görünür.
        /// </summary>
        public void Tick()
        {
            _hover.Tick();
            Structure hovered = _hover.Hovered;
            if (hovered != null)
            {
                _ghost.Hide();
                _range.Show(hovered.CurrentLevel, hovered.transform.position);
                return;
            }

            BuildableData data = _selection.Selected;
            if (data != null && TryGetPoint(out Vector3 point))
            {
                BuildPlacement placement = _site.Evaluate(data, point, _selection.RotationSteps);
                _ghost.Show(data, placement.Position, placement.Rotation, placement.IsValid ? _validColor : _invalidColor);
                _range.Show(data.GetLevel(0), placement.Position);
            }
            else
            {
                _ghost.Hide();
                _range.Hide();
            }
        }

        public void SelectSlot(int slotIndex)
        {
            if (_selection.Select(slotIndex))
            {
                RaiseSelectionChanged();
            }
            else if (!_selection.IsAvailable(slotIndex))
            {
                Debug.Log($"{LogPrefix}{slotIndex + 1}. yuvada yapı yok.", _site);
            }
        }

        public void Cycle(int direction)
        {
            if (_selection.Cycle(direction))
            {
                RaiseSelectionChanged();
            }
        }

        public void Rotate()
        {
            if (_selection.HasSelection)
            {
                _selection.Rotate();
            }
        }

        /// <summary>Seçili yapıyı imlecin altına yerleştirmeyi dener (para + yerleşim kuralları <see cref="BuildSite"/>'ta).</summary>
        public void Confirm()
        {
            BuildableData data = _selection.Selected;
            if (data == null || _hover.HasHover || !TryGetPoint(out Vector3 point))
            {
                return;
            }

            if (!_site.TryPlace(data, point, _selection.RotationSteps, out BuildPlacement placement)
                && placement.Status != PlacementStatus.Valid)
            {
                Debug.Log($"{LogPrefix}{data.DisplayName} buraya yerleştirilemez: {placement.Status}.", _site);
            }
        }

        /// <summary>U: imlecin altındaki yapıyı yükseltir.</summary>
        public void Upgrade()
        {
            _hover.Upgrade();
        }

        /// <summary>X: imlecin altındaki yapıyı satar.</summary>
        public void Sell()
        {
            _hover.Sell();
        }

        /// <summary>Seçimi bırakır ve önizlemeyi gizler. Seçim vardıysa true.</summary>
        public bool ClearSelection()
        {
            _ghost.Hide();
            _range.Hide();
            if (!_selection.Clear())
            {
                return false;
            }

            RaiseSelectionChanged();
            return true;
        }

        /// <summary>Mod kapanırken: seçim ve vurgu kalkar.</summary>
        public void Reset()
        {
            ClearSelection();
            _hover.Clear();
        }

        private bool TryGetPoint(out Vector3 point)
        {
            if (!_site.IsReady)
            {
                point = default;
                return false;
            }

            return _pointer.TryGetPoint(_site.Grid.Origin.y, out point);
        }

        private void RaiseSelectionChanged()
        {
            BuildableData data = _selection.Selected;
            EventBus<BuildSelectionChangedEvent>.Raise(
                new BuildSelectionChangedEvent(_selection.SelectedIndex, data != null ? data.Id : null));
        }
    }
}
