namespace LastHaven.Core
{
    /// <summary>
    /// Run boyunca değiştirilebilen statlar (GDD §5, §6, §11, §13). Değerler perk'lerden, marketten ve gece olaylarından
    /// gelen <see cref="StatModifier"/>'larla değişir. Taban değeri her zaman tüketicinin data'sından gelir.
    /// </summary>
    /// <remarks>Değerler SO'larda int olarak saklanır: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum StatType
    {
        /// <summary>Oyuncu silah hasarı (Ağır Mermi).</summary>
        PlayerDamage = 0,

        /// <summary>Oyuncu atış hızı (Hızlı Tetik).</summary>
        PlayerFireRate = 1,

        /// <summary>Oyuncu şarjör kapasitesi.</summary>
        PlayerMagSize = 2,

        /// <summary>Oyuncu maksimum canı (Kalın Deri).</summary>
        PlayerMaxHealth = 3,

        /// <summary>Atılma bekleme süresi (Adrenalin).</summary>
        DashCooldown = 4,

        /// <summary>Jeton mıknatısı yarıçapı (market Mıknatıs, Mıknatıs perk'i).</summary>
        PickupRadius = 5,

        /// <summary>Jeton değeri (Açgözlü, Kan Ayı).</summary>
        CoinValue = 6,

        /// <summary>Taret hasarı.</summary>
        TurretDamage = 7,

        /// <summary>Taret atış hızı (Taret Ustası).</summary>
        TurretFireRate = 8,

        /// <summary>Taret menzili.</summary>
        TurretRange = 9,

        /// <summary>Barikat maksimum canı (Sağlam Duvar).</summary>
        BarricadeMaxHealth = 10,
    }
}
