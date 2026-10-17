using LastHaven.Editor;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Yapı ipucu görünümü (10.3), gerçek HUD.uxml ile: ad, seviye adı ve X/Y, yükseltme fiyatı ya da "En üst seviye", satış iadesi,
    /// para yetmeyince kırmızı yükseltme fiyatı, can barı.
    /// </summary>
    public class StructureTooltipViewTests
    {
        private TemplateContainer _root;
        private StructureTooltipView _view;

        [SetUp]
        public void SetUp()
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIGenerator.HudPath);
            Assert.IsNotNull(layout, UIGenerator.HudPath);
            _root = layout.Instantiate();
            _view = new StructureTooltipView(
                _root.Q(HudElementNames.StructureTooltip),
                _root.Q<Label>(HudElementNames.TooltipName),
                _root.Q<Label>(HudElementNames.TooltipLevelName),
                _root.Q<Label>(HudElementNames.TooltipLevel),
                new BarView(_root.Q(HudElementNames.TooltipHealthFill), _root.Q<Label>(HudElementNames.TooltipHealthValue)),
                _root.Q(HudElementNames.TooltipUpgrade),
                _root.Q<Label>(HudElementNames.TooltipUpgradeCost),
                _root.Q(HudElementNames.TooltipMaxLevel),
                _root.Q<Label>(HudElementNames.TooltipSellRefund));
        }

        [Test]
        public void Show_WritesValues_AndShowsUpgradeRow()
        {
            _view.Show("Barikat", "Ahşap", 1, 3, 120, 25);

            Assert.AreEqual("Barikat", Text(HudElementNames.TooltipName));
            Assert.AreEqual("Ahşap", Text(HudElementNames.TooltipLevelName));
            Assert.AreEqual("1/3", Text(HudElementNames.TooltipLevel));
            Assert.AreEqual("120", Text(HudElementNames.TooltipUpgradeCost));
            Assert.AreEqual("+25", Text(HudElementNames.TooltipSellRefund));
            Assert.IsFalse(IsHidden(HudElementNames.TooltipUpgrade));
            Assert.IsTrue(IsHidden(HudElementNames.TooltipMaxLevel));
        }

        [Test]
        public void Show_MaxLevel_ReplacesUpgradeRow()
        {
            _view.Show("Barikat", "Beton", 3, 3, 0, 235);

            Assert.AreEqual("3/3", Text(HudElementNames.TooltipLevel));
            Assert.IsTrue(IsHidden(HudElementNames.TooltipUpgrade));
            Assert.IsFalse(IsHidden(HudElementNames.TooltipMaxLevel));
        }

        [Test]
        public void SetMoney_MarksUpgradeCost_OnlyWhenShortAndUpgradable()
        {
            Label cost = _root.Q<Label>(HudElementNames.TooltipUpgradeCost);
            _view.Show("Taret", "Standart", 1, 3, 300, 125);

            _view.SetMoney(299);
            Assert.IsTrue(cost.ClassListContains(StructureTooltipView.CostShortClass));

            _view.SetMoney(300);
            Assert.IsFalse(cost.ClassListContains(StructureTooltipView.CostShortClass));

            _view.Show("Taret", "Gelişmiş", 3, 3, 0, 525);
            _view.SetMoney(0);
            Assert.IsFalse(cost.ClassListContains(StructureTooltipView.CostShortClass), "En üst seviyede fiyat yok.");
        }

        [Test]
        public void HealthBar_WritesCurrentAndMax()
        {
            _view.HealthBar.Set(150f, 300f);

            Assert.AreEqual("150/300", Text(HudElementNames.TooltipHealthValue));
            Assert.AreEqual(50f, _root.Q(HudElementNames.TooltipHealthFill).style.width.value.value, 0.001f);
        }

        private string Text(string name)
        {
            return _root.Q<Label>(name).text;
        }

        private bool IsHidden(string name)
        {
            return _root.Q(name).ClassListContains(UIClassNames.Hidden);
        }
    }
}
