namespace LastHaven.Core
{
    /// <summary>Market öğesinin etkisi (GDD §6, §10). Hedef id ve değer etkiye göre yorumlanır (<see cref="IShopItem"/>).</summary>
    /// <remarks>Değerler SO'larda int olarak saklanır: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum ShopEffectType
    {
        /// <summary>Silahı açar (<see cref="WeaponUnlockRequestedEvent"/>). Hedef: silah id'si.</summary>
        UnlockWeapon = 0,

        /// <summary>Silahın bir yükseltmesini bir seviye artırır (<see cref="WeaponUpgradePurchasedEvent"/>). Hedef: silah id'si.</summary>
        WeaponUpgrade = 1,

        /// <summary>Yedek mermi ekler (<see cref="AmmoPurchasedEvent"/>). Hedef: silah id'si; değer: mermi adedi.</summary>
        BuyAmmo = 2,

        /// <summary>Oyuncuyu iyileştirir (<see cref="HealRequestedEvent"/>). Değer: can.</summary>
        HealthKit = 3,

        /// <summary>Cüzdana yakıt ekler. Değer: birim.</summary>
        BuyFuel = 4,

        /// <summary>Jeton toplama yarıçapına düz ekleme (<see cref="StatType.PickupRadius"/>). Değer: metre.</summary>
        PickupRadius = 5,
    }
}
