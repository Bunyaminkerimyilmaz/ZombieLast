using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Run sonu ekranındaki bir perk (kodla kurulur; stil <c>UI/Panels/Panels.uss</c>): nadirlik renginde çerçeve
    /// (<see cref="PerkStyle.SetRarity"/>), perk adı (veriden) ve yığın "2/3".
    /// </summary>
    public static class RunEndPerkChip
    {
        public const string ChipClass = "run-end-perk";
        public const string NameClass = "run-end-perk__name";
        public const string StacksClass = "run-end-perk__stacks";

        public static VisualElement Create(IPerkInfo perk, int stacks)
        {
            var root = new VisualElement();
            root.AddToClassList(ChipClass);
            PerkStyle.SetRarity(root, ChipClass, perk.Rarity);

            var name = new Label(perk.DisplayName);
            name.AddToClassList("label");
            name.AddToClassList(NameClass);
            var stackLabel = new Label(HudFormat.OfTotal(stacks, perk.MaxStacks));
            stackLabel.AddToClassList("label-small");
            stackLabel.AddToClassList(StacksClass);
            root.Add(name);
            root.Add(stackLabel);
            return root;
        }
    }
}
