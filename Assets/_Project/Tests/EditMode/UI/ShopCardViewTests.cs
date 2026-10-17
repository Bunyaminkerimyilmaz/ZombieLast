using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Market kartı ve sekmeleri (10.2), gerçek <c>ShopCard.uxml</c> şablonuyla: metinler, seviye noktaları, durum sınıfları
    /// (soluk, MAKS, ALINDI, uyarılar), Satın Al geri çağrısı, animasyon sınıfları; sekme seçimi sayfaları değiştirir.
    /// </summary>
    public class ShopCardViewTests
    {
        private TestShopItems _items;
        private VisualTreeAsset _template;

        [SetUp]
        public void SetUp()
        {
            _items = new TestShopItems();
            _template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIGenerator.ShopCardPath);
            Assert.IsNotNull(_template, UIGenerator.ShopCardPath);
        }

        [TearDown]
        public void TearDown()
        {
            _items.DestroyAll();
        }

        [Test]
        public void Create_WritesTexts_AndOneDotPerLevel()
        {
            var item = _items.Create("pistol_damage", ShopEffectType.WeaponUpgrade, new[] { 150, 300, 500, 800, 1200 }, 5, "pistol");

            ShopCardView card = ShopCardView.Create(_template, item, "Pompalı", null);

            Assert.IsNotNull(card);
            Assert.IsTrue(card.Root.ClassListContains(ShopCardView.CardClass));
            Assert.IsNull(card.Root.parent, "Kart şablon sarmalayıcısından ayrılmalı.");
            Assert.AreEqual("pistol_damage", card.Root.Q<Label>(ShopElementNames.CardName).text);
            Assert.AreEqual("Pompalı", card.Root.Q<Label>(ShopElementNames.CardRequirement).text);
            Assert.AreEqual(5, card.Root.Query(className: ShopCardView.DotClass).ToList().Count);
            Assert.IsFalse(card.Root.Q(ShopElementNames.CardDots).ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void Create_SingleLevelItem_HidesDots()
        {
            var item = _items.Create("unlock_shotgun", ShopEffectType.UnlockWeapon, new[] { 400 }, 1, "shotgun");

            ShopCardView card = ShopCardView.Create(_template, item, string.Empty, null);

            Assert.AreEqual(0, card.Root.Query(className: ShopCardView.DotClass).ToList().Count);
            Assert.IsTrue(card.Root.Q(ShopElementNames.CardDots).ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void Apply_NotEnoughMoney_DimsCard_AndMarksPrice()
        {
            ShopCardView card = CreateUpgradeCard();

            card.Apply(ShopCardState.From(ShopPurchaseStatus.NotEnoughMoney, 2, 3, 800));

            Assert.IsTrue(card.Root.ClassListContains(ShopCardView.DimmedClass));
            Label price = card.Root.Q<Label>(ShopElementNames.CardPrice);
            Assert.AreEqual("800", price.text);
            Assert.IsTrue(price.ClassListContains(ShopCardView.PriceShortClass));
            Assert.IsFalse(price.ClassListContains(UIClassNames.Hidden));
            Assert.AreEqual(2, card.Root.Query(className: ShopCardView.DotFilledClass).ToList().Count);
        }

        [Test]
        public void Apply_MaxLevel_ShowsMax_HidesPrice()
        {
            ShopCardView card = CreateUpgradeCard();

            card.Apply(ShopCardState.From(ShopPurchaseStatus.MaxLevel, 3, 3, 0));

            Assert.IsTrue(card.Root.Q(ShopElementNames.CardPrice).ClassListContains(UIClassNames.Hidden));
            Assert.IsFalse(card.Root.Q(ShopElementNames.CardMax).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(card.Root.Q(ShopElementNames.CardOwned).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(card.Root.ClassListContains(ShopCardView.DoneClass));
            Assert.IsFalse(card.Root.ClassListContains(ShopCardView.DimmedClass));
            Assert.AreEqual(3, card.Root.Query(className: ShopCardView.DotFilledClass).ToList().Count);
        }

        [Test]
        public void Apply_BackToAvailable_RestoresPriceAndClearsNotices()
        {
            ShopCardView card = CreateUpgradeCard();
            card.Apply(ShopCardState.From(ShopPurchaseStatus.Locked, 0, 3, 200));
            Assert.IsFalse(card.Root.Q(ShopElementNames.CardLocked).ClassListContains(UIClassNames.Hidden), "Kilitliyken uyarı görünür.");

            card.Apply(ShopCardState.From(ShopPurchaseStatus.Available, 1, 3, 450));

            Assert.IsTrue(card.Root.Q(ShopElementNames.CardLocked).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(card.Root.Q(ShopElementNames.CardUnavailable).ClassListContains(UIClassNames.Hidden));
            Assert.IsFalse(card.Root.ClassListContains(ShopCardView.DimmedClass));
            Assert.AreEqual("450", card.Root.Q<Label>(ShopElementNames.CardPrice).text);
            Assert.AreEqual(1, card.Root.Query(className: ShopCardView.DotFilledClass).ToList().Count);
        }

        [Test]
        public void Apply_NotApplicable_ShowsUnavailableNotice()
        {
            var item = _items.Create("health_kit", ShopEffectType.HealthKit, new[] { 60 }, 0, value: 50f);
            ShopCardView card = ShopCardView.Create(_template, item, string.Empty, null);

            card.Apply(ShopCardState.From(ShopPurchaseStatus.NotApplicable, 0, 0, 60));

            Assert.IsFalse(card.Root.Q(ShopElementNames.CardUnavailable).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(card.Root.ClassListContains(ShopCardView.DimmedClass));
        }

        [Test]
        public void Animations_AddAndStop()
        {
            ShopCardView card = CreateUpgradeCard();

            card.PlayDenied();
            Assert.IsTrue(card.Root.ClassListContains(ShopCardView.DeniedClass));
            card.PlayPurchased();
            Assert.IsTrue(card.Root.ClassListContains(ShopCardView.PurchasedClass));
            Assert.IsFalse(card.Root.ClassListContains(ShopCardView.DeniedClass), "Satın alma reddi geçersiz kılar.");

            card.StopAnimations();
            Assert.IsFalse(card.Root.ClassListContains(ShopCardView.PurchasedClass));
            Assert.IsFalse(card.Root.ClassListContains(ShopCardView.DeniedClass));
        }

        [Test]
        public void TabBar_Select_ShowsOnlySelectedPage()
        {
            var tabs = new[] { new Button(), new Button(), new Button() };
            var pages = new[] { new VisualElement(), new VisualElement(), new VisualElement() };
            var bar = new ShopTabBar(tabs, pages);
            Assert.AreEqual(ShopCategory.Weapons, bar.Selected);

            bar.Select(ShopCategory.Supplies);

            Assert.AreEqual(ShopCategory.Supplies, bar.Selected);
            Assert.AreSame(tabs[2], bar.SelectedTab);
            Assert.AreSame(pages[1], bar.GetPage(ShopCategory.Upgrades));
            for (int i = 0; i < tabs.Length; i++)
            {
                bool selected = i == 2;
                Assert.AreEqual(selected, tabs[i].ClassListContains(ShopTabBar.SelectedClass));
                Assert.AreEqual(!selected, pages[i].ClassListContains(UIClassNames.Hidden));
            }
        }

        private ShopCardView CreateUpgradeCard()
        {
            var item = _items.Create("shotgun_fire_rate", ShopEffectType.WeaponUpgrade, new[] { 200, 450, 800 }, 3, "shotgun",
                requiredItemId: "unlock_shotgun", upgradeType: WeaponUpgradeType.FireRate);
            return ShopCardView.Create(_template, item, "Pompalı", null);
        }
    }
}
