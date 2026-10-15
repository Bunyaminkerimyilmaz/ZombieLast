using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa modunda var olan yapının üzerine gelme (GDD §11, §16): imlecin altındaki yapıyı bulur, vurgular (yarı saydam kabuk,
    /// <see cref="BuildGhost"/>) ve değişince <see cref="StructureHoveredEvent"/> gönderir. U yükseltir, X satar (%50 iade).
    /// Fare: önce yapı collider'ına ışın (yüksek yapının görünen yüzü), değmezse imlecin yerdeki hücresi. Gamepad: yerleşim
    /// noktasının hücresi. Metotlar sadece inşa modu açıkken çağrılır.
    /// </summary>
    public sealed class StructureHoverTool
    {
        private const string LogPrefix = "[Building] ";

        private readonly BuildSite _site;
        private readonly BuildGhost _highlight;
        private readonly BuildPointer _pointer;
        private readonly Color _color;
        private Structure _hovered;

        /// <param name="highlight">Vurgu kabuğu; null ise vurgu yok (olay ve U/X yine çalışır).</param>
        public StructureHoverTool(BuildSite site, BuildGhost highlight, BuildPointer pointer, Color color)
        {
            _site = site;
            _highlight = highlight;
            _pointer = pointer;
            _color = color;
        }

        /// <summary>Üzerine gelinen yapı; yoksa null.</summary>
        public Structure Hovered => _hovered;

        public bool HasHover => _hovered != null;

        /// <summary>Her kare: imlecin altındaki yapıyı bulur; değiştiyse vurguyu taşır ve olayı gönderir.</summary>
        public void Tick()
        {
            Structure found = Find();
            if (found != _hovered)
            {
                SetHovered(found);
            }
        }

        /// <summary>Üzerine gelinen yapıyı yükseltir (U). Sonuç loglanır; başarılıysa olay yeni değerlerle tekrar gönderilir.</summary>
        public void Upgrade()
        {
            if (_hovered == null)
            {
                Debug.Log($"{LogPrefix}Yükseltmek için bir yapının üzerine gel.", _site);
                return;
            }

            switch (_hovered.Upgrade())
            {
                case StructureUpgradeResult.Upgraded:
                    RaiseHovered();
                    break;
                case StructureUpgradeResult.MaxLevel:
                    Debug.Log($"{LogPrefix}{_hovered.Data.DisplayName} zaten en üst seviyede ({_hovered.Level}/{_hovered.MaxLevel}).", _hovered);
                    break;
                case StructureUpgradeResult.NotEnoughMoney:
                    Debug.Log($"{LogPrefix}{_hovered.Data.DisplayName} yükseltmesi için para yetmiyor ({_hovered.UpgradeCost}).", _hovered);
                    break;
            }
        }

        /// <summary>Üzerine gelinen yapıyı satar (X): iade cüzdana, yapı havuza; vurgu kalkar.</summary>
        public void Sell()
        {
            if (_hovered == null)
            {
                Debug.Log($"{LogPrefix}Satmak için bir yapının üzerine gel.", _site);
                return;
            }

            if (_hovered.Sell())
            {
                SetHovered(null);
            }
        }

        /// <summary>Vurguyu kaldırır (mod kapanınca). Bir yapı vurgulanıyorduysa boş olay gönderir.</summary>
        public void Clear()
        {
            if (_hovered != null)
            {
                SetHovered(null);
            }
            else
            {
                _highlight?.Hide();
            }
        }

        private Structure Find()
        {
            if (!_site.IsReady)
            {
                return null;
            }

            Structure structure;
            if (_pointer.TryGetScreenRay(out Ray ray)
                && Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, GameLayers.StructureMask, QueryTriggerInteraction.Ignore)
                && _site.TryFindStructure(hit.collider.transform, out structure))
            {
                return structure;
            }

            return _pointer.TryGetPoint(_site.Grid.Origin.y, out Vector3 point) && _site.TryGetStructureAt(point, out structure)
                ? structure
                : null;
        }

        private void SetHovered(Structure structure)
        {
            _hovered = structure;
            if (structure != null)
            {
                Transform target = structure.transform;
                _highlight?.Show(structure.Data, target.position, target.rotation, _color);
                string upgrade = structure.CanUpgrade ? FormattableString.Invariant($"{structure.UpgradeCost}") : "en üst seviye";
                Debug.Log(FormattableString.Invariant(
                    $"{LogPrefix}Üzerinde: {structure.Data.DisplayName} {structure.CurrentLevel?.DisplayName} ({structure.Level}/{structure.MaxLevel}), can {structure.Health.Current:F0}/{structure.Health.Max:F0}. U: yükselt ({upgrade}), X: sat (+{structure.SellRefund})."),
                    structure);
            }
            else
            {
                _highlight?.Hide();
            }

            RaiseHovered();
        }

        private void RaiseHovered()
        {
            Structure structure = _hovered;
            EventBus<StructureHoveredEvent>.Raise(structure != null
                ? new StructureHoveredEvent(
                    structure.gameObject, structure.Data.Id, structure.Level, structure.MaxLevel, structure.UpgradeCost, structure.SellRefund)
                : new StructureHoveredEvent(null, null, 0, 0, 0, 0));
        }
    }
}
