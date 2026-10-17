namespace LastHaven.Core
{
    /// <summary>
    /// Marketten silah alındı (GDD §6, §10): oyuncunun bu silahı açılmalı. Economy'deki market gönderir, Player uygular
    /// (yuvayı açar; elde silah yoksa ele alır). Para düşüldükten sonra, <see cref="PurchaseCompletedEvent"/>'ten önce gelir.
    /// </summary>
    public readonly struct WeaponUnlockRequestedEvent : IEvent
    {
        public WeaponUnlockRequestedEvent(string weaponId)
        {
            WeaponId = weaponId;
        }

        /// <summary>Açılacak silahın id'si (<c>WeaponData.Id</c>).</summary>
        public string WeaponId { get; }
    }

    /// <summary>
    /// Marketten silah yükseltmesi alındı (GDD §6): o silahın bu yükseltmesi bir seviye artmalı. Market gönderir, Player uygular.
    /// <see cref="PurchaseCompletedEvent"/>'ten önce gelir.
    /// </summary>
    public readonly struct WeaponUpgradePurchasedEvent : IEvent
    {
        public WeaponUpgradePurchasedEvent(string weaponId, WeaponUpgradeType upgradeType, int level)
        {
            WeaponId = weaponId;
            UpgradeType = upgradeType;
            Level = level;
        }

        public string WeaponId { get; }

        public WeaponUpgradeType UpgradeType { get; }

        /// <summary>Marketteki yeni seviye (1 tabanlı). Uygulayan her olayda bir seviye artırır.</summary>
        public int Level { get; }
    }

    /// <summary>Marketten mermi alındı (GDD §10): silahın yedek mermisine eklenmeli. Market gönderir, Player uygular.</summary>
    public readonly struct AmmoPurchasedEvent : IEvent
    {
        public AmmoPurchasedEvent(string weaponId, int amount)
        {
            WeaponId = weaponId;
            Amount = amount;
        }

        public string WeaponId { get; }

        /// <summary>Eklenecek yedek mermi.</summary>
        public int Amount { get; }
    }

    /// <summary>Oyuncu iyileşmeli (GDD §10: can paketi +50). Market gönderir, Player uygular (ölüyken etkisiz).</summary>
    public readonly struct HealRequestedEvent : IEvent
    {
        public HealRequestedEvent(float amount)
        {
            Amount = amount;
        }

        /// <summary>İyileşecek can (maksimumda kırpılır).</summary>
        public float Amount { get; }
    }

    /// <summary>
    /// Satın alma tamamlandı: para düşüldü, seviye arttı, etki uygulandı (etki olayından sonra). Arayüz animasyonu, ses ve
    /// istatistik dinler.
    /// </summary>
    public readonly struct PurchaseCompletedEvent : IEvent
    {
        public PurchaseCompletedEvent(string itemId, ShopEffectType effect, int level, int price)
        {
            ItemId = itemId;
            Effect = effect;
            Level = level;
            Price = price;
        }

        public string ItemId { get; }

        public ShopEffectType Effect { get; }

        /// <summary>Öğenin yeni seviyesi (sarf malzemesinde toplam alım sayısı).</summary>
        public int Level { get; }

        /// <summary>Ödenen para.</summary>
        public int Price { get; }
    }
}
