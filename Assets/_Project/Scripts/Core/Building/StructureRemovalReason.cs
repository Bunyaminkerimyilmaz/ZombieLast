namespace LastHaven.Core
{
    /// <summary>Yapının ızgaradan neden kalktığı (<see cref="StructureRemovedEvent"/>).</summary>
    /// <remarks>Değerler açıkça numaralı: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum StructureRemovalReason
    {
        /// <summary>Oyuncu sattı (GDD §11: harcananın %50'si iade).</summary>
        Sold = 0,

        /// <summary>Canı bitti (düşmanlar yıktı).</summary>
        Destroyed = 1,
    }
}
