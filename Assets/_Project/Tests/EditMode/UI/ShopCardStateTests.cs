using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// ShopCardState (10.2): durum → soluk / fiyat / MAKS / ALINDI / uyarı, seviye noktaları; ShopPanelRules: Tab sadece gündüz ve oyun
    /// modunda açar, gündüz bitince kapanır.
    /// </summary>
    public class ShopCardStateTests
    {
        [Test]
        public void Available_ShowsPrice_NotDimmed_CanBuy()
        {
            ShopCardState state = ShopCardState.From(ShopPurchaseStatus.Available, 0, 1, 400);

            Assert.IsTrue(state.CanBuy);
            Assert.IsFalse(state.IsDimmed);
            Assert.IsFalse(state.IsPriceShort);
            Assert.AreEqual(ShopPriceDisplay.Price, state.PriceDisplay);
            Assert.AreEqual(400, state.Price);
            Assert.AreEqual(ShopCardNotice.None, state.Notice);
        }

        [Test]
        public void NotEnoughMoney_IsDimmed_PriceShort_NoNotice()
        {
            ShopCardState state = ShopCardState.From(ShopPurchaseStatus.NotEnoughMoney, 0, 1, 900);

            Assert.IsFalse(state.CanBuy);
            Assert.IsTrue(state.IsDimmed, "Para yetmiyorsa kart soluk.");
            Assert.IsTrue(state.IsPriceShort);
            Assert.AreEqual(ShopPriceDisplay.Price, state.PriceDisplay);
            Assert.AreEqual(ShopCardNotice.None, state.Notice);
        }

        [Test]
        public void MaxLevel_LeveledItem_ShowsMax_NotDimmed_AllDotsFilled()
        {
            ShopCardState state = ShopCardState.From(ShopPurchaseStatus.MaxLevel, 3, 3, 0);

            Assert.AreEqual(ShopPriceDisplay.Max, state.PriceDisplay);
            Assert.IsFalse(state.IsDimmed);
            Assert.IsFalse(state.CanBuy);
            Assert.AreEqual(3, state.DotCount);
            Assert.AreEqual(3, state.FilledDots);
        }

        [Test]
        public void MaxLevel_SingleLevelItem_ShowsOwned_NoDots()
        {
            ShopCardState state = ShopCardState.From(ShopPurchaseStatus.MaxLevel, 1, 1, 0);

            Assert.AreEqual(ShopPriceDisplay.Owned, state.PriceDisplay);
            Assert.AreEqual(0, state.DotCount);
        }

        [Test]
        public void Locked_IsDimmed_ShowsLockedNotice()
        {
            ShopCardState state = ShopCardState.From(ShopPurchaseStatus.Locked, 0, 5, 150);

            Assert.IsTrue(state.IsDimmed);
            Assert.AreEqual(ShopCardNotice.Locked, state.Notice);
            Assert.AreEqual(ShopPriceDisplay.Price, state.PriceDisplay);
        }

        [TestCase(ShopPurchaseStatus.NotApplicable)]
        [TestCase(ShopPurchaseStatus.Closed)]
        [TestCase(ShopPurchaseStatus.UnknownItem)]
        public void Unavailable_IsDimmed_ShowsUnavailableNotice(ShopPurchaseStatus status)
        {
            ShopCardState state = ShopCardState.From(status, 0, 0, 60);

            Assert.IsTrue(state.IsDimmed);
            Assert.AreEqual(ShopCardNotice.Unavailable, state.Notice);
        }

        [TestCase(0, 5, 5, 0)]
        [TestCase(2, 5, 5, 2)]
        [TestCase(7, 5, 5, 5)]
        [TestCase(-1, 3, 3, 0)]
        [TestCase(4, 0, 0, 0)]
        [TestCase(1, 1, 0, 0)]
        public void Dots_OnlyForMultiLevelItems_FilledClamped(int level, int maxLevel, int expectedDots, int expectedFilled)
        {
            ShopCardState state = ShopCardState.From(ShopPurchaseStatus.Available, level, maxLevel, 10);

            Assert.AreEqual(expectedDots, state.DotCount);
            Assert.AreEqual(expectedFilled, state.FilledDots);
        }

        [Test]
        public void NegativePrice_IsZero()
        {
            Assert.AreEqual(0, ShopCardState.From(ShopPurchaseStatus.Available, 0, 0, -5).Price);
        }

        [Test]
        public void Equality_DependsOnVisibleState()
        {
            ShopCardState a = ShopCardState.From(ShopPurchaseStatus.Available, 1, 3, 450);
            ShopCardState same = ShopCardState.From(ShopPurchaseStatus.Available, 1, 3, 450);

            Assert.IsTrue(a.Equals(same));
            Assert.AreEqual(a.GetHashCode(), same.GetHashCode());
            Assert.IsFalse(a.Equals(ShopCardState.From(ShopPurchaseStatus.NotEnoughMoney, 1, 3, 450)));
            Assert.IsFalse(a.Equals(ShopCardState.From(ShopPurchaseStatus.Available, 2, 3, 450)));
            Assert.IsFalse(a.Equals(ShopCardState.From(ShopPurchaseStatus.Available, 1, 3, 800)));
        }

        [Test]
        public void PanelRules_OpensOnlyInGameplayModeDuringDay()
        {
            Assert.IsTrue(ShopPanelRules.CanOpen(InputMode.Gameplay, true, GamePhase.Day));
            Assert.IsFalse(ShopPanelRules.CanOpen(InputMode.BuildMode, true, GamePhase.Day), "İnşa modunda açılmaz.");
            Assert.IsFalse(ShopPanelRules.CanOpen(InputMode.UI, true, GamePhase.Day), "Başka menü açıkken açılmaz.");
            Assert.IsFalse(ShopPanelRules.CanOpen(InputMode.Gameplay, true, GamePhase.Night), "Gece açılmaz.");
            Assert.IsFalse(ShopPanelRules.CanOpen(InputMode.Gameplay, true, GamePhase.Dusk));
            Assert.IsFalse(ShopPanelRules.CanOpen(InputMode.Gameplay, true, GamePhase.Dawn));
            Assert.IsFalse(ShopPanelRules.CanOpen(InputMode.Gameplay, true, GamePhase.Boot));
        }

        [Test]
        public void PanelRules_WithoutGameFlow_IgnoresPhase()
        {
            Assert.IsTrue(ShopPanelRules.CanOpen(InputMode.Gameplay, false, GamePhase.Night));
            Assert.IsFalse(ShopPanelRules.CanOpen(InputMode.BuildMode, false, GamePhase.Day));
        }

        [TestCase(GamePhase.Day, true)]
        [TestCase(GamePhase.Dusk, false)]
        [TestCase(GamePhase.Night, false)]
        [TestCase(GamePhase.Dawn, false)]
        [TestCase(GamePhase.GameOver, false)]
        [TestCase(GamePhase.Victory, false)]
        public void PanelRules_StaysOpenOnlyDuringDay(GamePhase phase, bool expected)
        {
            Assert.AreEqual(expected, ShopPanelRules.StaysOpen(phase));
        }
    }
}
