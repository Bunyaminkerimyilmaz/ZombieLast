using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Market (10.1): seviye takibi, para yetmezse, maks. seviye, gece engeli, ön koşul, sarf malzemesi, etkiler (olaylar, yakıt,
    /// mıknatıs) ve satın almanın olay sırası.
    /// </summary>
    public class ShopTests
    {
        private const float Tolerance = 0.0001f;

        private readonly List<string> _events = new List<string>();
        private TestShopItems _items;
        private Wallet _wallet;
        private RunModifiers _modifiers;
        private FakeGameState _gameState;
        private FakePlayer _player;
        private List<ShopItemData> _catalog;

        [SetUp]
        public void SetUp()
        {
            _items = new TestShopItems();
            _wallet = new Wallet(1000, 100);
            _wallet.SetCapacity(WalletResource.Fuel, 150);
            _modifiers = new RunModifiers();
            _gameState = new FakeGameState();
            _player = new FakePlayer();
            _catalog = new List<ShopItemData>
            {
                _items.Create("unlock_shotgun", ShopEffectType.UnlockWeapon, new[] { 400 }, 1, "shotgun"),
                _items.Create("shotgun_damage", ShopEffectType.WeaponUpgrade, new[] { 150, 300, 500, 800, 1200 }, 5, "shotgun",
                    requiredItemId: "unlock_shotgun", upgradeType: WeaponUpgradeType.Damage),
                _items.Create("magnet", ShopEffectType.PickupRadius, new[] { 100, 200, 350 }, 3, value: 1f),
                _items.Create("ammo_shotgun", ShopEffectType.BuyAmmo, new[] { 40 }, 0, "shotgun", 12f, "unlock_shotgun"),
                _items.Create("health_kit", ShopEffectType.HealthKit, new[] { 60 }, 0, value: 50f),
                _items.Create("fuel", ShopEffectType.BuyFuel, new[] { 15 }, 0, value: 10f),
            };

            _events.Clear();
            EventBus<WeaponUnlockRequestedEvent>.Subscribe(OnUnlock);
            EventBus<WeaponUpgradePurchasedEvent>.Subscribe(OnUpgrade);
            EventBus<AmmoPurchasedEvent>.Subscribe(OnAmmo);
            EventBus<HealRequestedEvent>.Subscribe(OnHeal);
            EventBus<PurchaseCompletedEvent>.Subscribe(OnCompleted);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<WeaponUnlockRequestedEvent>.Unsubscribe(OnUnlock);
            EventBus<WeaponUpgradePurchasedEvent>.Unsubscribe(OnUpgrade);
            EventBus<AmmoPurchasedEvent>.Unsubscribe(OnAmmo);
            EventBus<HealRequestedEvent>.Unsubscribe(OnHeal);
            EventBus<PurchaseCompletedEvent>.Unsubscribe(OnCompleted);
            _items.DestroyAll();
        }

        [Test]
        public void New_KeepsCatalogOrder_AllAtLevelZero()
        {
            Shop shop = CreateShop();

            Assert.AreEqual(_catalog.Count, shop.GetItems().Count);
            for (int i = 0; i < _catalog.Count; i++)
            {
                Assert.AreSame(_catalog[i], shop.GetItems()[i]);
                Assert.AreEqual(0, shop.GetLevel(_catalog[i].Id));
            }

            Assert.AreEqual(400, shop.GetPrice("unlock_shotgun"));
        }

        [Test]
        public void Buy_UnlockWeapon_PaysRaisesLevelAndSendsEffectBeforeCompleted()
        {
            Shop shop = CreateShop();

            Assert.IsTrue(shop.Buy("unlock_shotgun"));

            Assert.AreEqual(600, _wallet.Money);
            Assert.AreEqual(1, shop.GetLevel("unlock_shotgun"));
            CollectionAssert.AreEqual(new[] { "unlock:shotgun", "completed:unlock_shotgun:1:400" }, _events);
        }

        [Test]
        public void NotEnoughMoney_CannotBuy_AndNothingChanges()
        {
            _wallet.TrySpend(WalletResource.Money, 700);
            Shop shop = CreateShop();

            Assert.AreEqual(ShopPurchaseStatus.NotEnoughMoney, shop.GetStatus("unlock_shotgun"));
            Assert.IsFalse(shop.CanBuy("unlock_shotgun"));
            Assert.IsFalse(shop.Buy("unlock_shotgun"));

            Assert.AreEqual(300, _wallet.Money);
            Assert.AreEqual(0, shop.GetLevel("unlock_shotgun"));
            Assert.IsEmpty(_events);
        }

        [Test]
        public void ExactMoney_IsEnough()
        {
            _wallet.TrySpend(WalletResource.Money, 600);
            Shop shop = CreateShop();

            Assert.IsTrue(shop.Buy("unlock_shotgun"));
            Assert.AreEqual(0, _wallet.Money);
        }

        [Test]
        public void MaxLevel_UsesPricePerLevel_ThenStopsSales()
        {
            Shop shop = CreateShop();

            Assert.IsTrue(shop.Buy("magnet"));
            Assert.AreEqual(200, shop.GetPrice("magnet"));
            Assert.IsTrue(shop.Buy("magnet"));
            Assert.AreEqual(350, shop.GetPrice("magnet"));
            Assert.IsTrue(shop.Buy("magnet"));

            Assert.AreEqual(1000 - 100 - 200 - 350, _wallet.Money);
            Assert.AreEqual(3, shop.GetLevel("magnet"));
            Assert.AreEqual(ShopPurchaseStatus.MaxLevel, shop.GetStatus("magnet"));
            Assert.AreEqual(0, shop.GetPrice("magnet"));
            Assert.IsFalse(shop.Buy("magnet"));
            Assert.AreEqual(350, _wallet.Money);
        }

        [Test]
        public void MaxLevel_UnlockedWeaponCannotBeBoughtTwice()
        {
            Shop shop = CreateShop();
            shop.Buy("unlock_shotgun");

            Assert.AreEqual(ShopPurchaseStatus.MaxLevel, shop.GetStatus("unlock_shotgun"));
            Assert.IsFalse(shop.Buy("unlock_shotgun"));
            Assert.AreEqual(600, _wallet.Money);
        }

        [TestCase(GamePhase.Boot)]
        [TestCase(GamePhase.Dusk)]
        [TestCase(GamePhase.Night)]
        [TestCase(GamePhase.Dawn)]
        [TestCase(GamePhase.GameOver)]
        [TestCase(GamePhase.Victory)]
        public void NotDay_ShopClosed_AndNothingChanges(GamePhase phase)
        {
            _gameState.CurrentPhase = phase;
            Shop shop = CreateShop();

            Assert.IsFalse(shop.IsOpen);
            Assert.AreEqual(ShopPurchaseStatus.Closed, shop.GetStatus("fuel"));
            Assert.IsFalse(shop.Buy("fuel"));

            Assert.AreEqual(1000, _wallet.Money);
            Assert.AreEqual(100, _wallet.Fuel);
            Assert.AreEqual(0, shop.GetLevel("fuel"));
            Assert.IsEmpty(_events);
        }

        [Test]
        public void Closed_TakesPrecedenceOverMaxLevel()
        {
            Shop shop = CreateShop();
            shop.Buy("unlock_shotgun");
            _gameState.CurrentPhase = GamePhase.Night;

            Assert.AreEqual(ShopPurchaseStatus.Closed, shop.GetStatus("unlock_shotgun"));
        }

        [Test]
        public void PhaseIsReadOnEveryCall()
        {
            Shop shop = CreateShop();
            _gameState.CurrentPhase = GamePhase.Night;
            Assert.IsFalse(shop.Buy("fuel"));

            _gameState.CurrentPhase = GamePhase.Day;

            Assert.IsTrue(shop.Buy("fuel"));
        }

        [Test]
        public void WithoutGameState_AlwaysOpen()
        {
            var shop = new Shop(_catalog, _wallet, _modifiers, null, _player);

            Assert.IsTrue(shop.IsOpen);
            Assert.IsTrue(shop.Buy("fuel"));
        }

        [Test]
        public void RequiredItem_LocksUntilBought()
        {
            Shop shop = CreateShop();

            Assert.AreEqual(ShopPurchaseStatus.Locked, shop.GetStatus("shotgun_damage"));
            Assert.AreEqual(ShopPurchaseStatus.Locked, shop.GetStatus("ammo_shotgun"));
            Assert.IsFalse(shop.Buy("shotgun_damage"));

            shop.Buy("unlock_shotgun");

            Assert.AreEqual(ShopPurchaseStatus.Available, shop.GetStatus("shotgun_damage"));
            Assert.AreEqual(ShopPurchaseStatus.Available, shop.GetStatus("ammo_shotgun"));
        }

        [Test]
        public void WeaponUpgrade_SendsWeaponTypeAndNewLevel()
        {
            Shop shop = CreateShop();
            shop.Buy("unlock_shotgun");
            _events.Clear();

            shop.Buy("shotgun_damage");
            shop.Buy("shotgun_damage");

            CollectionAssert.AreEqual(new[]
            {
                "upgrade:shotgun:Damage:1", "completed:shotgun_damage:1:150",
                "upgrade:shotgun:Damage:2", "completed:shotgun_damage:2:300",
            }, _events);
            Assert.AreEqual(1000 - 400 - 150 - 300, _wallet.Money);
        }

        [Test]
        public void Ammo_IsRepeatable_WithSamePrice()
        {
            Shop shop = CreateShop();
            shop.Buy("unlock_shotgun");
            _events.Clear();

            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(shop.Buy("ammo_shotgun"));
            }

            Assert.AreEqual(3, shop.GetLevel("ammo_shotgun"), "Sarf malzemesinde seviye = alım sayısı.");
            Assert.AreEqual(40, shop.GetPrice("ammo_shotgun"));
            Assert.AreEqual(1000 - 400 - 3 * 40, _wallet.Money);
            Assert.AreEqual("ammo:shotgun:12", _events[0]);
            Assert.AreEqual("completed:ammo_shotgun:3:40", _events[5]);
        }

        [Test]
        public void Fuel_AddsToWallet_UntilTankHasNoRoom()
        {
            _wallet.Add(WalletResource.Fuel, 30);
            Shop shop = CreateShop();

            Assert.IsTrue(shop.Buy("fuel"));
            Assert.AreEqual(140, _wallet.Fuel);
            Assert.AreEqual(985, _wallet.Money);
            Assert.IsTrue(shop.Buy("fuel"), "140 + 10 = 150 tam sığar.");
            Assert.AreEqual(150, _wallet.Fuel);

            Assert.AreEqual(ShopPurchaseStatus.NotApplicable, shop.GetStatus("fuel"));
            Assert.IsFalse(shop.Buy("fuel"));
            Assert.AreEqual(970, _wallet.Money, "Dolu depoya para harcanmaz.");
        }

        [Test]
        public void Fuel_PartialRoom_IsNotApplicable()
        {
            _wallet.Add(WalletResource.Fuel, 41);
            Shop shop = CreateShop();

            Assert.AreEqual(141, _wallet.Fuel);
            Assert.AreEqual(ShopPurchaseStatus.NotApplicable, shop.GetStatus("fuel"));
        }

        [Test]
        public void HealthKit_OnlyWhenAliveAndHurt()
        {
            Shop shop = CreateShop();
            Assert.AreEqual(ShopPurchaseStatus.NotApplicable, shop.GetStatus("health_kit"), "Can dolu.");

            _player.CurrentHealth = 0f;
            _player.IsAlive = false;
            Assert.AreEqual(ShopPurchaseStatus.NotApplicable, shop.GetStatus("health_kit"), "Ölü.");

            _player.IsAlive = true;
            _player.CurrentHealth = 70f;
            Assert.IsTrue(shop.Buy("health_kit"));

            CollectionAssert.AreEqual(new[] { "heal:50", "completed:health_kit:1:60" }, _events);
            Assert.AreEqual(940, _wallet.Money);
        }

        [Test]
        public void HealthKit_WithoutPlayer_IsNotApplicable()
        {
            var shop = new Shop(_catalog, _wallet, _modifiers, _gameState);

            Assert.AreEqual(ShopPurchaseStatus.NotApplicable, shop.GetStatus("health_kit"));
        }

        [Test]
        public void Magnet_AddsFlatPickupRadiusPerLevel()
        {
            Shop shop = CreateShop();

            shop.Buy("magnet");
            Assert.AreEqual(4f, _modifiers.Get(StatType.PickupRadius, 3f), Tolerance);

            shop.Buy("magnet");
            shop.Buy("magnet");
            Assert.AreEqual(6f, _modifiers.Get(StatType.PickupRadius, 3f), Tolerance);
            CollectionAssert.AreEqual(new[] { "completed:magnet:1:100", "completed:magnet:2:200", "completed:magnet:3:350" }, _events);
        }

        [Test]
        public void Magnet_WithoutRunModifiers_IsNotApplicable()
        {
            var shop = new Shop(_catalog, _wallet, null, _gameState, _player);

            Assert.AreEqual(ShopPurchaseStatus.NotApplicable, shop.GetStatus("magnet"));
            Assert.AreEqual(ShopPurchaseStatus.Available, shop.GetStatus("fuel"));
        }

        [Test]
        public void UnknownItem_IsNotAnError()
        {
            Shop shop = CreateShop();

            Assert.AreEqual(ShopPurchaseStatus.UnknownItem, shop.GetStatus("minigun"));
            Assert.AreEqual(ShopPurchaseStatus.UnknownItem, shop.GetStatus(null));
            Assert.AreEqual(ShopPurchaseStatus.UnknownItem, shop.GetStatus("FUEL"), "Id ordinal karşılaştırılır.");
            Assert.IsFalse(shop.Buy("minigun"));
            Assert.AreEqual(0, shop.GetLevel("minigun"));
            Assert.AreEqual(0, shop.GetPrice(null));
            Assert.IsNull(shop.GetItem("minigun"));
        }

        [Test]
        public void New_DuplicateOrEmptyId_Throws_AndNullItemsAreSkipped()
        {
            var duplicate = new List<IShopItem> { _catalog[0], _items.Create("unlock_shotgun", ShopEffectType.BuyFuel, new[] { 1 }, 0) };
            var empty = new List<IShopItem> { _items.Create(string.Empty, ShopEffectType.BuyFuel, new[] { 1 }, 0) };

            Assert.Throws<ArgumentException>(() => new Shop(duplicate, _wallet));
            Assert.Throws<ArgumentException>(() => new Shop(empty, _wallet));
            Assert.Throws<ArgumentNullException>(() => new Shop(_catalog, null));
            Assert.AreEqual(1, new Shop(new List<IShopItem> { null, _catalog[0] }, _wallet).GetItems().Count);
        }

        [Test]
        public void ItemPrice_OutsideListUsesLastPrice_EmptyListIsFree()
        {
            ShopItemData item = _items.Create("x", ShopEffectType.BuyFuel, new[] { 10, 20 }, 0);
            ShopItemData free = _items.Create("y", ShopEffectType.BuyFuel, Array.Empty<int>(), 0);

            Assert.AreEqual(10, item.GetPrice(-1));
            Assert.AreEqual(20, item.GetPrice(1));
            Assert.AreEqual(20, item.GetPrice(5));
            Assert.AreEqual(0, free.GetPrice(0));
            Assert.IsTrue(item.IsRepeatable);
        }

        private Shop CreateShop()
        {
            return new Shop(_catalog, _wallet, _modifiers, _gameState, _player);
        }

        private void OnUnlock(WeaponUnlockRequestedEvent evt) => _events.Add($"unlock:{evt.WeaponId}");

        private void OnUpgrade(WeaponUpgradePurchasedEvent evt) => _events.Add($"upgrade:{evt.WeaponId}:{evt.UpgradeType}:{evt.Level}");

        private void OnAmmo(AmmoPurchasedEvent evt) => _events.Add($"ammo:{evt.WeaponId}:{evt.Amount}");

        private void OnHeal(HealRequestedEvent evt) => _events.Add($"heal:{evt.Amount:0.#}");

        private void OnCompleted(PurchaseCompletedEvent evt) => _events.Add($"completed:{evt.ItemId}:{evt.Level}:{evt.Price}");
    }
}
