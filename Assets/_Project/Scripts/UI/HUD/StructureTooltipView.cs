using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Yapı ipucu kutusunun görünümü (HUD.uxml <c>structure-tooltip</c>): ad, seviye adı, seviye X/Y, can barı, yükseltme fiyatı
    /// (U) ya da "En üst seviye", satış iadesi (X). Para yetmiyorsa yükseltme fiyatı kırmızı (<see cref="CostShortClass"/>).
    /// Metin başlıkları UXML'de; bu sınıf sadece veri yazar.
    /// </summary>
    public sealed class StructureTooltipView
    {
        public const string CostShortClass = "hud-tooltip__cost--short";

        private readonly Label _name;
        private readonly Label _levelName;
        private readonly Label _level;
        private readonly VisualElement _upgradeRow;
        private readonly Label _upgradeCost;
        private readonly VisualElement _maxLevelRow;
        private readonly Label _sellRefund;
        private int _shownUpgradeCost;
        private bool _canUpgrade;

        public StructureTooltipView(VisualElement root, Label name, Label levelName, Label level, BarView healthBar,
            VisualElement upgradeRow, Label upgradeCost, VisualElement maxLevelRow, Label sellRefund)
        {
            Root = root;
            HealthBar = healthBar;
            _name = name;
            _levelName = levelName;
            _level = level;
            _upgradeRow = upgradeRow;
            _upgradeCost = upgradeCost;
            _maxLevelRow = maxLevelRow;
            _sellRefund = sellRefund;
        }

        public VisualElement Root { get; }

        /// <summary>Can barı (<see cref="HealthBarBinding"/> bağlanır).</summary>
        public BarView HealthBar { get; }

        /// <summary>Yapının değerlerini yazar. En üst seviyede yükseltme satırı yerine "En üst seviye" görünür.</summary>
        /// <param name="level">1 tabanlı seviye.</param>
        public void Show(string displayName, string levelName, int level, int maxLevel, int upgradeCost, int sellRefund)
        {
            _name.text = displayName ?? string.Empty;
            _levelName.text = levelName ?? string.Empty;
            _level.text = HudFormat.OfTotal(level, maxLevel);
            _canUpgrade = level < maxLevel;
            _shownUpgradeCost = upgradeCost;
            _upgradeCost.text = HudFormat.Count(upgradeCost);
            _upgradeRow.SetShown(_canUpgrade);
            _maxLevelRow.SetShown(!_canUpgrade);
            _sellRefund.text = HudFormat.Gain(sellRefund);
        }

        /// <summary>Yükseltme fiyatını paraya göre işaretler (yetmiyorsa kırmızı).</summary>
        public void SetMoney(int money)
        {
            _upgradeCost.EnableInClassList(CostShortClass, _canUpgrade && _shownUpgradeCost > money);
        }
    }
}
