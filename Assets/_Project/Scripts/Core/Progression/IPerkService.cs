using System.Collections.Generic;

namespace LastHaven.Core
{
    /// <summary>
    /// Run boyunca alınan perk'ler (GDD §13; Progression'daki <c>PerkService</c>, [Systems]/Progression, Awake'te kaydolur).
    /// Perk'lere string id ile başvurulur (ordinal). Perk'ler run boyunca kalır; kaldırma yok. Alım
    /// <see cref="PerkAcquiredEvent"/> ile duyurulur.
    /// </summary>
    public interface IPerkService
    {
        /// <summary>Seviye atlayınca sunulan kart sayısı (GDD §13: 3).</summary>
        int OfferSize { get; }

        /// <summary>Run boyunca alınan toplam yığın (tüm perk'ler).</summary>
        int TotalStacks { get; }

        /// <summary>En az bir kez alınan perk'lerin id'leri, ilk alım sırasıyla (HUD perk listesi).</summary>
        IReadOnlyList<string> AcquiredPerkIds { get; }

        /// <summary>Katalogdaki perk'in arayüz bilgileri (ad, açıklama, ikon, nadirlik); bilinmeyen id null.</summary>
        IPerkInfo GetPerk(string perkId);

        /// <summary>Perk'in şu anki yığını; alınmamış veya bilinmeyen perk 0.</summary>
        int GetStacks(string perkId);

        /// <summary>Perk'in en fazla yığını; bilinmeyen perk 0.</summary>
        int GetMaxStacks(string perkId);

        /// <summary>Perk katalogda var ve en fazla yığınına ulaşmamış mı.</summary>
        bool CanAcquire(string perkId);

        /// <summary>
        /// Perk'i alır: yığın artar, etkileri uygulanır (<see cref="IRunModifiers"/>, iyileşme), <see cref="PerkAcquiredEvent"/>
        /// gönderilir. Alınamazsa (bilinmeyen, maks. yığında) hiçbir şey değişmez ve false döner.
        /// </summary>
        bool ApplyPerk(string perkId);

        /// <summary>
        /// Ağırlıklı rastgele (nadirlik ağırlıkları, GDD §13) en fazla <paramref name="count"/> farklı, alınabilir perk id'si seçer.
        /// Liste önce temizlenir. Maks. yığındaki perk'ler çıkmaz; yeterli aday yoksa daha az döner. Seçilen sayıyı döndürür.
        /// </summary>
        int GenerateOffer(List<string> perkIds, int count);
    }
}
