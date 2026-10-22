using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>Bir nadirliğin perk kartı çekilişindeki ağırlığı (GDD §13: Sıradan 60, Nadir 30, Epik 10).</summary>
    [Serializable]
    public struct RarityWeight
    {
        [SerializeField, Tooltip("Nadirlik.")]
        private PerkRarity _rarity;

        [SerializeField, Min(0), Tooltip("Çekiliş ağırlığı; oran diğer nadirliklerin ağırlığına göredir. 0 = hiç çıkmaz.")]
        private int _weight;

        public RarityWeight(PerkRarity rarity, int weight)
        {
            _rarity = rarity;
            _weight = weight;
        }

        public PerkRarity Rarity => _rarity;

        public int Weight => _weight;
    }
}
