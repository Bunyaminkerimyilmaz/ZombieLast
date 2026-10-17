namespace LastHaven.Core
{
    /// <summary>Market sekmesi (GDD §6, §10).</summary>
    /// <remarks>Değerler SO'larda int olarak saklanır: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum ShopCategory
    {
        /// <summary>Silahlar: yeni silah açma.</summary>
        Weapons = 0,

        /// <summary>Yükseltmeler: silah yükseltmeleri, mıknatıs.</summary>
        Upgrades = 1,

        /// <summary>Malzeme: mermi, can paketi, yakıt.</summary>
        Supplies = 2,
    }
}
