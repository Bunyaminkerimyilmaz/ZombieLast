using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// [Systems]/Shop: <see cref="IShopService"/>. Awake'te kaydolur; <see cref="ShopCatalog"/>'taki öğelerle saf <see cref="Shop"/>'u
    /// ilk kullanımda (en geç Start'ta) cüzdan, run modifier'ları, oyun akışı ve oyuncu servisleriyle kurar ve ona devreder.
    /// Satın almaları loglar. Sahne yeniden yüklenince (yeni run) seviyeler sıfırlanır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopService : MonoBehaviour, IShopService
    {
        private const string LogPrefix = "[Economy] ";

        [SerializeField, Tooltip("Market öğeleri. Üretici bağlar.")]
        private ShopCatalog _catalog;

        [Header("Debug")]
        [SerializeField, Tooltip("Sağ tık → Debug/Satın al ile alınacak öğenin id'si (Play Mode; market arayüzü gelene kadar).")]
        private string _debugItemId = "unlock_shotgun";

        private Shop _shop;
        private bool _initialized;

        private void Awake()
        {
            if (_catalog == null)
            {
                Debug.LogError("ShopService: ShopCatalog atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            ServiceLocator.Register<IShopService>(this);
        }

        private void Start()
        {
            GetShop();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IShopService>(this);
        }

        public IReadOnlyList<IShopItem> GetItems()
        {
            return GetShop()?.GetItems() ?? Array.Empty<IShopItem>();
        }

        public int GetLevel(string itemId)
        {
            return GetShop()?.GetLevel(itemId) ?? 0;
        }

        public int GetPrice(string itemId)
        {
            return GetShop()?.GetPrice(itemId) ?? 0;
        }

        public ShopPurchaseStatus GetStatus(string itemId)
        {
            return GetShop()?.GetStatus(itemId) ?? ShopPurchaseStatus.Closed;
        }

        public bool CanBuy(string itemId)
        {
            return GetStatus(itemId) == ShopPurchaseStatus.Available;
        }

        public bool Buy(string itemId)
        {
            Shop shop = GetShop();
            if (shop == null)
            {
                return false;
            }

            ShopPurchaseStatus status = shop.GetStatus(itemId);
            int price = shop.GetPrice(itemId);
            if (!shop.Buy(itemId))
            {
                Debug.Log($"{LogPrefix}Satın alınamadı: '{itemId}' ({status}).", this);
                return false;
            }

            IShopItem item = shop.GetItem(itemId);
            string level = item.IsRepeatable ? string.Empty : $" ({shop.GetLevel(itemId)}/{item.MaxLevel})";
            ServiceLocator.TryGet(out IWallet wallet);
            Debug.Log($"{LogPrefix}Satın alındı: {item.DisplayName}{level}, −{price} para (kalan {wallet?.Money}).", this);
            return true;
        }

        // Servisler Awake'te kaydolur; ilk çağrı Start'ta veya sonrasında gelir.
        private Shop GetShop()
        {
            if (_initialized || !enabled)
            {
                return _shop;
            }

            _initialized = true;
            if (!ServiceLocator.TryGet(out IWallet wallet))
            {
                Debug.LogWarning("ShopService: IWallet bulunamadı; market satış yapmayacak. Sahnede [Systems]/Economy var mı?", this);
                return null;
            }

            if (!ServiceLocator.TryGet(out IRunModifiers modifiers))
            {
                Debug.LogWarning("ShopService: IRunModifiers bulunamadı; mıknatıs satılamayacak.", this);
            }

            if (!ServiceLocator.TryGet(out IGameStateService gameState))
            {
                Debug.LogWarning("ShopService: IGameStateService bulunamadı; market evreden bağımsız hep açık.", this);
            }

            if (!ServiceLocator.TryGet(out IPlayer player))
            {
                Debug.LogWarning("ShopService: IPlayer bulunamadı; can paketi satılamayacak.", this);
            }

            try
            {
                _shop = new Shop(_catalog.Items, wallet, modifiers, gameState, player);
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"ShopService: {exception.Message} ShopCatalog'u düzelt.", this);
            }

            return _shop;
        }

        [ContextMenu("Debug/Satın al")]
        private void BuyForDebug()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("ShopService: Önce Play Mode'a gir.", this);
                return;
            }

            Buy(_debugItemId);
        }

        [ContextMenu("Debug/Market durumunu logla")]
        private void LogStateForDebug()
        {
            IReadOnlyList<IShopItem> items = GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                IShopItem item = items[i];
                Debug.Log($"{LogPrefix}{item.Id}: seviye {GetLevel(item.Id)}/{(item.IsRepeatable ? "∞" : item.MaxLevel.ToString())}, " +
                          $"fiyat {GetPrice(item.Id)}, {GetStatus(item.Id)}", this);
            }
        }
    }
}
