using System;
using LastHaven.Core;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>
    /// Ganimet tablosundaki bir seçenek (GDD §9): ödül türü, miktar aralığı (dahil) ve ağırlık. Sandık açılınca tablodaki
    /// seçeneklerden biri ağırlığına göre seçilir (<see cref="LootTable"/>).
    /// </summary>
    [Serializable]
    public sealed class LootRewardOption
    {
        [SerializeField, Tooltip("Para jeton olarak saçılır; yakıt ve mermi paketi toplanabilir eşya olarak düşer.")]
        private LootRewardType _type = LootRewardType.Money;

        [SerializeField, Min(0), Tooltip("En az miktar (para / yakıt birimi / mermi paketi adedi).")]
        private int _minAmount = 1;

        [SerializeField, Min(0), Tooltip("En çok miktar (dahil). En azdan küçükse en az kullanılır.")]
        private int _maxAmount = 1;

        [SerializeField, Min(0f), Tooltip("Seçilme ağırlığı: şans = ağırlık / tablodaki ağırlıkların toplamı. 0 = hiç seçilmez.")]
        private float _weight = 1f;

        public LootRewardOption()
        {
        }

        public LootRewardOption(LootRewardType type, int minAmount, int maxAmount, float weight)
        {
            _type = type;
            _minAmount = minAmount;
            _maxAmount = maxAmount;
            _weight = weight;
        }

        public LootRewardType Type => _type;

        public int MinAmount => Mathf.Max(0, _minAmount);

        public int MaxAmount => Mathf.Max(MinAmount, _maxAmount);

        public float Weight => Mathf.Max(0f, _weight);

        /// <summary>Aralıktan (dahil) eşit olasılıkla bir miktar.</summary>
        public int RollAmount(Random random)
        {
            int min = MinAmount;
            int max = MaxAmount;
            return max == int.MaxValue ? random.Next(min, max) : random.Next(min, max + 1);
        }
    }
}
