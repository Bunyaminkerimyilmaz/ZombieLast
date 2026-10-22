using System;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD perk listesindeki tek rozet (kodla kurulur; stil <c>UI/HUD/HudPerks.uss</c>): ikon ya da baş harfler, nadirlik rengi
    /// (<see cref="PerkStyle.SetRarity"/>) ve yığın 2+ ise köşede yığın sayısı. Üzerine gelince / çıkınca çağıranı haberdar eder
    /// (ipucu kutusu). Alt elementler fareyi yakalamaz; olaylar rozetin kendisine gelir.
    /// </summary>
    public sealed class PerkBadgeView
    {
        public const string BadgeClass = "hud-perk";
        public const string IconClass = "hud-perk__icon";
        public const string InitialsClass = "hud-perk__initials";
        public const string StacksClass = "hud-perk__stacks";

        private readonly Label _stacks;

        private PerkBadgeView(IPerkInfo perk, VisualElement root, Label stacks)
        {
            Perk = perk;
            Root = root;
            _stacks = stacks;
        }

        public IPerkInfo Perk { get; }

        public VisualElement Root { get; }

        public int Stacks { get; private set; }

        public int MaxStacks { get; private set; }

        /// <param name="entered">Fare rozetin üstüne geldi.</param>
        /// <param name="left">Fare rozetten çıktı.</param>
        public static PerkBadgeView Create(IPerkInfo perk, Action<PerkBadgeView> entered, Action<PerkBadgeView> left)
        {
            var root = new VisualElement();
            root.AddToClassList(BadgeClass);
            PerkStyle.SetRarity(root, BadgeClass, perk.Rarity);

            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList(IconClass);
            var initials = new Label { pickingMode = PickingMode.Ignore };
            initials.AddToClassList("label");
            initials.AddToClassList(InitialsClass);
            var stacks = new Label { pickingMode = PickingMode.Ignore };
            stacks.AddToClassList("label-tiny");
            stacks.AddToClassList(StacksClass);
            root.Add(icon);
            root.Add(initials);
            root.Add(stacks);
            PerkStyle.ApplyIcon(icon, initials, perk);

            var view = new PerkBadgeView(perk, root, stacks);
            view.SetStacks(0, perk.MaxStacks);
            root.RegisterCallback<PointerEnterEvent>(_ => entered?.Invoke(view));
            root.RegisterCallback<PointerLeaveEvent>(_ => left?.Invoke(view));
            return view;
        }

        /// <summary>Yığını yazar; köşedeki sayı sadece 2 ve üstünde görünür.</summary>
        public void SetStacks(int stacks, int maxStacks)
        {
            Stacks = stacks;
            MaxStacks = maxStacks;
            _stacks.text = HudFormat.Count(stacks);
            _stacks.SetShown(stacks > 1);
        }
    }
}
