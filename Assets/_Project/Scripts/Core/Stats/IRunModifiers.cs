using System;

namespace LastHaven.Core
{
    /// <summary>
    /// Run boyunca geçerli stat modifier'ları ve perk bayrakları. Perk'ler, market ve gece olayları yazar;
    /// oyuncu, silahlar, taretler, barikatlar ve ekonomi okur.
    /// </summary>
    public interface IRunModifiers
    {
        /// <summary>Bir statın modifier'ları değişince, etkilenen her stat için bir kez çağrılır.</summary>
        event Action<StatType> StatChanged;

        /// <summary>Bayrak ilk kez eklenince çağrılır.</summary>
        event Action<PerkFlag> FlagAdded;

        /// <summary>Taban değere bu statın modifier'larını uygular: (taban + Σflat) × (1 + Σyüzde), negatif çarpan 0'a kırpılır.</summary>
        float Get(StatType stat, float baseValue);

        void AddModifier(StatModifier modifier);

        /// <summary>Bu kaynağın tüm modifier'larını kaldırır (ör. gece olayı bitince); kaldırılan sayısını döndürür.</summary>
        int RemoveBySource(string sourceId);

        bool HasFlag(PerkFlag flag);

        /// <summary>Bayrağı ekler. Zaten varsa bir şey yapmaz. Perk'ler run boyunca kalır; kaldırma yok.</summary>
        void AddFlag(PerkFlag flag);
    }
}
