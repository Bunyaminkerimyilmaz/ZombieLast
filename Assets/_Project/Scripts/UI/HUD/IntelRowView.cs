using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// İstihbarat panelinin bir satırı (<c>HUD.uxml</c>, <see cref="RowClass"/>): adet + düşman adı ("20 Yürüyen"), boss satırında
    /// "BOSS" etiketi. Etiket metni UXML'de; kod sadece sayı ve veri adını yazar.
    /// </summary>
    public sealed class IntelRowView
    {
        public const string RowClass = "hud-intel-row";
        public const string CountClass = "hud-intel-row__count";
        public const string NameClass = "hud-intel-row__name";
        public const string BossClass = "hud-intel-row__boss";

        private readonly VisualElement _root;
        private readonly Label _count;
        private readonly Label _name;
        private readonly VisualElement _boss;

        public IntelRowView(VisualElement root)
        {
            _root = root;
            _count = root.Q<Label>(className: CountClass);
            _name = root.Q<Label>(className: NameClass);
            _boss = root.Q(className: BossClass);
        }

        /// <summary>Adet, ad ve boss etiketi bulundu.</summary>
        public bool IsValid => _count != null && _name != null && _boss != null;

        public void Show(in NightEnemyInfo enemy)
        {
            _count.text = HudFormat.Count(enemy.Count);
            _name.text = enemy.DisplayName;
            _boss.SetShown(enemy.IsBoss);
            _root.SetShown(true);
        }

        public void Hide()
        {
            _root.SetShown(false);
        }
    }
}
