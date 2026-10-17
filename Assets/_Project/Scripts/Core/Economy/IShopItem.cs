namespace LastHaven.Core
{
    /// <summary>
    /// Market öğesinin okunur görünümü (GDD §6, §10). Economy'deki <c>ShopItemData</c> uygular; arayüz ve market servisi okur.
    /// Seviye: öğenin kaç kez alındığı (0 = hiç). Seviyeli öğe <see cref="MaxLevel"/>'e kadar alınır; sarf malzemesi
    /// (<see cref="MaxLevel"/> 0) sınırsız alınır.
    /// </summary>
    public interface IShopItem
    {
        /// <summary>Benzersiz id (ordinal karşılaştırılır).</summary>
        string Id { get; }

        /// <summary>Görünen ad (Türkçe).</summary>
        string DisplayName { get; }

        /// <summary>Kısa açıklama (market kartı).</summary>
        string Description { get; }

        ShopCategory Category { get; }

        /// <summary>En yüksek seviye; 0 = sınırsız (sarf malzemesi: mermi, can paketi, yakıt).</summary>
        int MaxLevel { get; }

        /// <summary>Sınırsız alınabilir mi (<see cref="MaxLevel"/> 0).</summary>
        bool IsRepeatable { get; }

        /// <summary>
        /// Seviyeden bir sonrakine geçişin fiyatı (<paramref name="level"/> = mevcut seviye; 0 → ilk alım). Listenin dışında son
        /// fiyat geçerlidir (sarf malzemesi hep aynı fiyat). Fiyat listesi boşsa 0.
        /// </summary>
        int GetPrice(int level);

        ShopEffectType Effect { get; }

        /// <summary>Etkinin hedefi: silah açma, yükseltme ve mermide silah id'si; diğerlerinde boş.</summary>
        string TargetId { get; }

        /// <summary>Sadece <see cref="ShopEffectType.WeaponUpgrade"/>'te anlamlı: hangi yükseltme.</summary>
        WeaponUpgradeType UpgradeType { get; }

        /// <summary>Etkinin miktarı: mermi adedi, can, yakıt birimi, metre. Açma ve yükseltmede kullanılmaz.</summary>
        float Value { get; }

        /// <summary>Satın alınabilmesi için en az bir kez alınmış olması gereken öğenin id'si; boşsa koşulsuz.</summary>
        string RequiredItemId { get; }
    }
}
