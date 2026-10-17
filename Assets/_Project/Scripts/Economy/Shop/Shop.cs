using System;
using System.Collections.Generic;
using LastHaven.Core;

namespace LastHaven.Economy
{
    /// <summary>
    /// Market (saf C#; GDD §6, §10): öğe seviyeleri, satın alınabilirlik ve satın alma. Sadece gündüz satar. Satın almada para
    /// cüzdandan düşer (<see cref="MoneyChangedEvent"/>), seviye artar, etki uygulanır (<see cref="ShopEffects"/>), sonra
    /// <see cref="PurchaseCompletedEvent"/> gönderilir. Run başında seviyeler 0.
    /// </summary>
    public sealed class Shop : IShopService
    {
        private readonly List<IShopItem> _items = new List<IShopItem>();
        private readonly Dictionary<string, IShopItem> _itemsById = new Dictionary<string, IShopItem>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _levels = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly IWallet _wallet;
        private readonly IRunModifiers _modifiers;
        private readonly IGameStateService _gameState;
        private readonly IPlayer _player;

        /// <param name="items">Sırası korunur; null öğeler atlanır. Boş veya tekrarlanan id <see cref="ArgumentException"/>.</param>
        /// <param name="modifiers">Null ise mıknatıs satılamaz.</param>
        /// <param name="gameState">Null ise market hep açık (akışsız sahne).</param>
        /// <param name="player">Null ise can paketi satılamaz.</param>
        public Shop(
            IEnumerable<IShopItem> items,
            IWallet wallet,
            IRunModifiers modifiers = null,
            IGameStateService gameState = null,
            IPlayer player = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _modifiers = modifiers;
            _gameState = gameState;
            _player = player;

            foreach (IShopItem item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(item.Id) || _itemsById.ContainsKey(item.Id))
                {
                    throw new ArgumentException($"Market öğesinin id'si boş veya tekrarlanıyor: '{item.Id}' ({item.DisplayName}).", nameof(items));
                }

                _items.Add(item);
                _itemsById.Add(item.Id, item);
            }
        }

        /// <summary>Market açık mı: gündüz (akış yoksa hep).</summary>
        public bool IsOpen => _gameState == null || _gameState.CurrentPhase == GamePhase.Day;

        public IReadOnlyList<IShopItem> GetItems()
        {
            return _items;
        }

        /// <summary>Id'yle öğe; yoksa null.</summary>
        public IShopItem GetItem(string itemId)
        {
            return itemId != null && _itemsById.TryGetValue(itemId, out IShopItem item) ? item : null;
        }

        public int GetLevel(string itemId)
        {
            return itemId != null && _levels.TryGetValue(itemId, out int level) ? level : 0;
        }

        public int GetPrice(string itemId)
        {
            IShopItem item = GetItem(itemId);
            int level = GetLevel(itemId);
            return item != null && !IsMaxed(item, level) ? item.GetPrice(level) : 0;
        }

        public ShopPurchaseStatus GetStatus(string itemId)
        {
            IShopItem item = GetItem(itemId);
            if (item == null)
            {
                return ShopPurchaseStatus.UnknownItem;
            }

            if (!IsOpen)
            {
                return ShopPurchaseStatus.Closed;
            }

            int level = GetLevel(itemId);
            if (IsMaxed(item, level))
            {
                return ShopPurchaseStatus.MaxLevel;
            }

            if (!string.IsNullOrEmpty(item.RequiredItemId) && GetLevel(item.RequiredItemId) <= 0)
            {
                return ShopPurchaseStatus.Locked;
            }

            if (!ShopEffects.CanApply(item, _wallet, _modifiers, _player))
            {
                return ShopPurchaseStatus.NotApplicable;
            }

            return _wallet.CanAfford(WalletResource.Money, item.GetPrice(level))
                ? ShopPurchaseStatus.Available
                : ShopPurchaseStatus.NotEnoughMoney;
        }

        public bool CanBuy(string itemId)
        {
            return GetStatus(itemId) == ShopPurchaseStatus.Available;
        }

        public bool Buy(string itemId)
        {
            if (!CanBuy(itemId))
            {
                return false;
            }

            IShopItem item = _itemsById[itemId];
            int price = item.GetPrice(GetLevel(itemId));
            if (!_wallet.TrySpend(WalletResource.Money, price))
            {
                return false;
            }

            int level = GetLevel(itemId) + 1;
            _levels[item.Id] = level;
            ShopEffects.Apply(item, level, _wallet, _modifiers);
            EventBus<PurchaseCompletedEvent>.Raise(new PurchaseCompletedEvent(item.Id, item.Effect, level, price));
            return true;
        }

        private static bool IsMaxed(IShopItem item, int level)
        {
            return !item.IsRepeatable && level >= item.MaxLevel;
        }
    }
}
