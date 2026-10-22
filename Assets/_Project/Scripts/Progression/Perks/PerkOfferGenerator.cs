using System;
using System.Collections.Generic;
using LastHaven.Core;

namespace LastHaven.Progression
{
    /// <summary>Çekiliş adayı: nadirliği ve alınabilir olup olmadığı (maks. yığındaki perk aday değildir).</summary>
    public readonly struct PerkOfferCandidate
    {
        public PerkOfferCandidate(PerkRarity rarity, bool isAvailable)
        {
            Rarity = rarity;
            IsAvailable = isAvailable;
        }

        public PerkRarity Rarity { get; }

        public bool IsAvailable { get; }
    }

    /// <summary>
    /// Perk kartı çekilişi (saf C#, tohumlanabilir; GDD §13). Her kart için önce nadirlik ağırlıkla seçilir (Sıradan 60, Nadir 30,
    /// Epik 10; sadece hâlâ adayı olan nadirlikler yarışır), sonra o nadirliğin kalan adaylarından biri eşit olasılıkla. Aynı
    /// çekilişte bir perk iki kez çıkmaz; alınamayan (maks. yığın) adaylar hiç çıkmaz. Ağırlığı 0 olan nadirlik çıkmaz.
    /// Böylece "Epik %10" perk sayısından bağımsızdır. Bellek ayırmaz (tamponlar yeniden kullanılır).
    /// </summary>
    public sealed class PerkOfferGenerator
    {
        private static readonly int RarityCount = Enum.GetValues(typeof(PerkRarity)).Length;

        private readonly Random _random;
        private readonly int[] _rarityCounts = new int[RarityCount];
        private readonly List<int> _remaining = new List<int>();
        private readonly List<PerkOfferCandidate> _candidates = new List<PerkOfferCandidate>();
        private readonly List<int> _picked = new List<int>();

        /// <param name="seed">Aynı tohum ve aynı girdiler aynı kartları verir.</param>
        public PerkOfferGenerator(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// En fazla <paramref name="count"/> farklı adayın indeksini seçim sırasıyla <paramref name="results"/>'a yazar (liste önce
        /// temizlenir). Yeterli aday yoksa daha az. Seçilen sayıyı döndürür.
        /// </summary>
        /// <param name="rarityWeight">Nadirliğin ağırlığı; negatif 0 sayılır.</param>
        public int Generate(IReadOnlyList<PerkOfferCandidate> candidates, Func<PerkRarity, int> rarityWeight, int count, List<int> results)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (rarityWeight == null)
            {
                throw new ArgumentNullException(nameof(rarityWeight));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();
            _remaining.Clear();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].IsAvailable && IsKnown(candidates[i].Rarity))
                {
                    _remaining.Add(i);
                }
            }

            while (results.Count < count && TryPickRarity(candidates, rarityWeight, out PerkRarity rarity))
            {
                int position = FindRemaining(candidates, rarity, _random.Next(_rarityCounts[(int)rarity]));
                results.Add(_remaining[position]);
                _remaining.RemoveAt(position);
            }

            return results.Count;
        }

        /// <summary>
        /// Perk listesinden çekiliş: alınabilirlik <paramref name="inventory"/>'den (null ise hepsi alınabilir). Seçilen perk'ler
        /// <paramref name="results"/>'a yazılır (liste önce temizlenir); boş elemanlar aday değildir. Seçilen sayıyı döndürür.
        /// </summary>
        public int Generate(IReadOnlyList<PerkData> perks, PerkInventory inventory, Func<PerkRarity, int> rarityWeight, int count,
            List<PerkData> results)
        {
            if (perks == null)
            {
                throw new ArgumentNullException(nameof(perks));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            _candidates.Clear();
            for (int i = 0; i < perks.Count; i++)
            {
                PerkData perk = perks[i];
                bool available = perk != null && (inventory != null ? inventory.CanAcquire(perk) : !string.IsNullOrEmpty(perk.Id));
                _candidates.Add(new PerkOfferCandidate(perk != null ? perk.Rarity : default, available));
            }

            Generate(_candidates, rarityWeight, count, _picked);
            results.Clear();
            for (int i = 0; i < _picked.Count; i++)
            {
                results.Add(perks[_picked[i]]);
            }

            return results.Count;
        }

        private static bool IsKnown(PerkRarity rarity)
        {
            return (int)rarity >= 0 && (int)rarity < RarityCount;
        }

        // Kalan adayların nadirliklerini sayar ve adayı olan nadirlikler arasında ağırlıkla birini seçer.
        private bool TryPickRarity(IReadOnlyList<PerkOfferCandidate> candidates, Func<PerkRarity, int> rarityWeight, out PerkRarity rarity)
        {
            Array.Clear(_rarityCounts, 0, _rarityCounts.Length);
            for (int i = 0; i < _remaining.Count; i++)
            {
                _rarityCounts[(int)candidates[_remaining[i]].Rarity]++;
            }

            int total = 0;
            for (int r = 0; r < RarityCount; r++)
            {
                total += GetWeight(r, rarityWeight);
            }

            rarity = default;
            if (total <= 0)
            {
                return false;
            }

            int roll = _random.Next(total);
            for (int r = 0; r < RarityCount; r++)
            {
                int weight = GetWeight(r, rarityWeight);
                if (roll < weight)
                {
                    rarity = (PerkRarity)r;
                    return true;
                }

                roll -= weight;
            }

            return false;
        }

        // Adayı kalmamış nadirlik yarışmaz.
        private int GetWeight(int rarity, Func<PerkRarity, int> rarityWeight)
        {
            return _rarityCounts[rarity] > 0 ? Math.Max(0, rarityWeight((PerkRarity)rarity)) : 0;
        }

        // Kalanlar arasında bu nadirliğin n. adayının listedeki konumu.
        private int FindRemaining(IReadOnlyList<PerkOfferCandidate> candidates, PerkRarity rarity, int nth)
        {
            for (int i = 0; i < _remaining.Count; i++)
            {
                if (candidates[_remaining[i]].Rarity == rarity && nth-- == 0)
                {
                    return i;
                }
            }

            throw new InvalidOperationException("Nadirlik sayımı kalan adaylarla uyuşmuyor.");
        }
    }
}
