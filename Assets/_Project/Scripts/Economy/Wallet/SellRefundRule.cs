using System;

namespace LastHaven.Economy
{
    /// <summary>
    /// Satış iadesi (GDD §10, saf): yapıya harcanan toplam paranın <see cref="Rate"/> oranı, aşağı yuvarlı.
    /// Oran 0..1 aralığına kırpılır.
    /// </summary>
    public readonly struct SellRefundRule
    {
        public SellRefundRule(float rate)
        {
            Rate = float.IsNaN(rate) ? 0f : Math.Max(0f, Math.Min(1f, rate));
        }

        /// <summary>İade oranı (0.5 = %50).</summary>
        public float Rate { get; }

        /// <summary>
        /// İade: 50 → 25, 170 → 85, 25 → 12. Harcama 0 veya negatifse 0. Oran decimal'e çevrilerek çarpılır
        /// (<see cref="InterestRule"/> ile aynı: float hatası bir eksik vermez).
        /// </summary>
        public int Calculate(int totalSpent)
        {
            if (totalSpent <= 0 || Rate <= 0f)
            {
                return 0;
            }

            return (int)Math.Floor(totalSpent * (decimal)Rate);
        }
    }
}
