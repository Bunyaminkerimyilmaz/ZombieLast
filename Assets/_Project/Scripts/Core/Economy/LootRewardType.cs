namespace LastHaven.Core
{
    /// <summary>Ganimet sandığından çıkan ödülün türü (GDD §9).</summary>
    /// <remarks>Değerler açıkça numaralı (asset'lerde int saklanır): sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum LootRewardType
    {
        /// <summary>Para: jeton olarak saçılır.</summary>
        Money = 0,

        /// <summary>Yakıt (birim): yakıt bidonu olarak düşer, toplanınca cüzdana eklenir.</summary>
        Fuel = 1,

        /// <summary>Mermi paketi (adet): mermi kutusu olarak düşer, toplanınca silahların yedeğine eklenir.</summary>
        AmmoPack = 2,
    }
}
