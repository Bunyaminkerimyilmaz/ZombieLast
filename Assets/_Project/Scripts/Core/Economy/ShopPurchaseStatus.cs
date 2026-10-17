namespace LastHaven.Core
{
    /// <summary>
    /// Bir market öğesinin şu an satın alınıp alınamayacağı ve alınamıyorsa nedeni (<see cref="IShopService.GetStatus"/>).
    /// Kontrol sırası: bilinmeyen → kapalı → maks. seviye → kilitli → uygulanamaz → para yetersiz.
    /// </summary>
    public enum ShopPurchaseStatus
    {
        /// <summary>Satın alınabilir.</summary>
        Available = 0,

        /// <summary>Bu id'de öğe yok.</summary>
        UnknownItem = 1,

        /// <summary>Market kapalı: gündüz değil (GDD §10).</summary>
        Closed = 2,

        /// <summary>En yüksek seviyede (arayüz "MAKS" gösterir).</summary>
        MaxLevel = 3,

        /// <summary>Ön koşul öğesi henüz alınmadı (ör. pompalı alınmadan pompalı yükseltmesi).</summary>
        Locked = 4,

        /// <summary>Etki şu an uygulanamaz: can dolu veya oyuncu ölü, yakıt deposunda yer yok, gerekli servis yok.</summary>
        NotApplicable = 5,

        /// <summary>Para yetmiyor.</summary>
        NotEnoughMoney = 6,
    }
}
