using System.Collections.Generic;

namespace LastHaven.Core
{
    /// <summary>
    /// Market (GDD §6, §10). Economy'deki <c>ShopService</c> ([Systems]/Shop) Awake'te kaydeder. Sadece gündüz satar ve öğe
    /// seviyelerini tutar. Satın almada para cüzdandan düşer, etki olaylarla uygulanır (<see cref="WeaponUnlockRequestedEvent"/>,
    /// <see cref="WeaponUpgradePurchasedEvent"/>, <see cref="AmmoPurchasedEvent"/>, <see cref="HealRequestedEvent"/>), ardından
    /// <see cref="PurchaseCompletedEvent"/> gelir. Bilinmeyen id hata değildir (durum <see cref="ShopPurchaseStatus.UnknownItem"/>).
    /// </summary>
    public interface IShopService
    {
        /// <summary>Tüm öğeler, katalog sırasıyla.</summary>
        IReadOnlyList<IShopItem> GetItems();

        /// <summary>Öğenin seviyesi (kaç kez alındığı); bilinmeyen id'de 0.</summary>
        int GetLevel(string itemId);

        /// <summary>Bir sonraki alımın fiyatı; bilinmeyen veya en yüksek seviyedeki öğede 0.</summary>
        int GetPrice(string itemId);

        /// <summary>Şu an satın alınabilir mi, değilse nedeni.</summary>
        ShopPurchaseStatus GetStatus(string itemId);

        /// <summary><see cref="GetStatus"/> <see cref="ShopPurchaseStatus.Available"/> mı.</summary>
        bool CanBuy(string itemId);

        /// <summary>Satın alınabiliyorsa parayı düşer, seviyeyi artırır, etkiyi uygular ve true döner; değilse hiçbir şey değişmez.</summary>
        bool Buy(string itemId);
    }
}
