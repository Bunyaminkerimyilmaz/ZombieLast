using System;
using System.Collections.Generic;

namespace LastHaven.Progression
{
    /// <summary>
    /// Çekilişe giren geçerli perk'ler ve id ile arama (saf C#; <see cref="PerkService"/> kullanır). <see cref="Collect"/> katalog
    /// sırasını korur; boş, id'siz ve id'si tekrar eden perk'leri atlar. Id'ler ordinal karşılaştırılır.
    /// </summary>
    public sealed class PerkLookup
    {
        private readonly List<PerkData> _perks = new List<PerkData>();

        /// <summary>Geçerli perk'ler (katalog sırası).</summary>
        public IReadOnlyList<PerkData> Perks => _perks;

        /// <summary>
        /// Listeyi baştan kurar. Atlanan (id'si boş veya tekrar eden) perk'ler <paramref name="skipped"/>'e yazılır (verilmişse; önce
        /// temizlenir); null elemanlar sessizce atlanır.
        /// </summary>
        public void Collect(IReadOnlyList<PerkData> perks, List<PerkData> skipped = null)
        {
            _perks.Clear();
            skipped?.Clear();
            if (perks == null)
            {
                return;
            }

            for (int i = 0; i < perks.Count; i++)
            {
                PerkData perk = perks[i];
                if (perk == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(perk.Id) || Find(perk.Id) != null)
                {
                    skipped?.Add(perk);
                    continue;
                }

                _perks.Add(perk);
            }
        }

        /// <summary>Id'si verilen perk; yoksa (veya id boşsa) null.</summary>
        public PerkData Find(string perkId)
        {
            if (string.IsNullOrEmpty(perkId))
            {
                return null;
            }

            for (int i = 0; i < _perks.Count; i++)
            {
                if (string.Equals(_perks[i].Id, perkId, StringComparison.Ordinal))
                {
                    return _perks[i];
                }
            }

            return null;
        }
    }
}
