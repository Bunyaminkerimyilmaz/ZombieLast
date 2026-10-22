using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un perk listesi (GDD §16 "alınan perk'ler"): sol altta, can kutusunun üstünde alınan her perk için küçük bir rozet
    /// (<see cref="PerkBadgeView"/>: ikon yoksa baş harfler + nadirlik rengi), ilk alım sırasıyla; perk yokken liste gizli. Rozetin
    /// üstüne gelince listenin üstünde ipucu kutusu: ad, nadirlik, yığın "2/3", açıklama. Başlangıç durumu Start'ta
    /// <see cref="IPerkService.AcquiredPerkIds"/>'ten, sonrası <see cref="PerkAcquiredEvent"/> ile; Update yok. HUD'un UIDocument'ını
    /// paylaşır. Gamepad'de üzerine gelme olmadığı için ipucu yalnız fareyle açılır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudPerksController : UIDocumentController
    {
        /// <summary>İpucu kutusunun nadirlik sınıflarının bloğu (<c>hud-perk-tooltip--epic</c>; renk HudPerks.uss'te).</summary>
        public const string TooltipBlock = "hud-perk-tooltip";

        private readonly List<PerkBadgeView> _badges = new List<PerkBadgeView>();
        private VisualElement _list;
        private VisualElement _tooltip;
        private Label _tooltipName;
        private Label _tooltipStacks;
        private Label _tooltipDescription;
        private IPerkService _perks;
        private PerkBadgeView _hovered;
        private Action<PerkBadgeView> _entered;
        private Action<PerkBadgeView> _left;
        private bool _started;

        protected override void BindElements()
        {
            _list = Require<VisualElement>(HudElementNames.PerkList);
            _tooltip = Require<VisualElement>(HudElementNames.PerkTooltip);
            _tooltipName = Require<Label>(HudElementNames.PerkTooltipName);
            _tooltipStacks = Require<Label>(HudElementNames.PerkTooltipStacks);
            _tooltipDescription = Require<Label>(HudElementNames.PerkTooltipDescription);
        }

        private void Awake()
        {
            _entered = ShowTooltip;
            _left = HideTooltip;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            EventBus<PerkAcquiredEvent>.Subscribe(OnPerkAcquired);

            // UIDocument her açılışta ağacı yeniden kurar: rozetler servisten yeniden kurulur (ilk kurulum Start'ta).
            if (_started)
            {
                Rebuild();
            }
        }

        private void OnDisable()
        {
            EventBus<PerkAcquiredEvent>.Unsubscribe(OnPerkAcquired);
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            _started = true;
            if (!ServiceLocator.TryGet(out _perks))
            {
                Debug.LogWarning("HudPerksController: IPerkService bulunamadı; perk listesi boş kalacak. Sahnede [Systems]/Progression var mı?", this);
            }

            Rebuild();
        }

        private void Rebuild()
        {
            _badges.Clear();
            _list.Clear();
            _hovered = null;
            _tooltip.SetShown(false);
            if (_perks != null)
            {
                IReadOnlyList<string> acquired = _perks.AcquiredPerkIds;
                for (int i = 0; i < acquired.Count; i++)
                {
                    string perkId = acquired[i];
                    AddOrUpdate(perkId, _perks.GetStacks(perkId), _perks.GetMaxStacks(perkId));
                }
            }

            _list.SetShown(_badges.Count > 0);
        }

        private void OnPerkAcquired(PerkAcquiredEvent acquired)
        {
            AddOrUpdate(acquired.PerkId, acquired.Stacks, acquired.MaxStacks);
            _list.SetShown(_badges.Count > 0);
        }

        private void AddOrUpdate(string perkId, int stacks, int maxStacks)
        {
            PerkBadgeView badge = Find(perkId);
            if (badge == null)
            {
                IPerkInfo perk = _perks?.GetPerk(perkId);
                if (perk == null)
                {
                    return;
                }

                badge = PerkBadgeView.Create(perk, _entered, _left);
                _list.Add(badge.Root);
                _badges.Add(badge);
            }

            badge.SetStacks(stacks, maxStacks);
            if (_hovered == badge)
            {
                ShowTooltip(badge);
            }
        }

        private PerkBadgeView Find(string perkId)
        {
            for (int i = 0; i < _badges.Count; i++)
            {
                if (string.Equals(_badges[i].Perk.Id, perkId, StringComparison.Ordinal))
                {
                    return _badges[i];
                }
            }

            return null;
        }

        private void ShowTooltip(PerkBadgeView badge)
        {
            _hovered = badge;
            _tooltipName.text = badge.Perk.DisplayName;
            _tooltipStacks.text = HudFormat.OfTotal(badge.Stacks, badge.MaxStacks);
            _tooltipDescription.text = badge.Perk.Description;
            PerkStyle.SetRarity(_tooltip, TooltipBlock, badge.Perk.Rarity);
            _tooltip.SetShown(true);
        }

        private void HideTooltip(PerkBadgeView badge)
        {
            if (_hovered == badge)
            {
                _hovered = null;
                _tooltip.SetShown(false);
            }
        }
    }
}
