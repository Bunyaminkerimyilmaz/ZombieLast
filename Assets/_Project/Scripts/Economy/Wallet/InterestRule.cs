using System;

namespace LastHaven.Economy
{
    /// <summary>
    /// Şafak faizi (GDD §10, saf): paranın <see cref="Rate"/> oranı, aşağı yuvarlı, en fazla <see cref="Cap"/>.
    /// Oran 0..1 aralığına kırpılır, tavan negatifse 0 sayılır.
    /// </summary>
    public readonly struct InterestRule
    {
        public InterestRule(float rate, int cap)
        {
            Rate = float.IsNaN(rate) ? 0f : Math.Max(0f, Math.Min(1f, rate));
            Cap = Math.Max(0, cap);
        }

        /// <summary>Faiz oranı (0.1 = %10).</summary>
        public float Rate { get; }

        /// <summary>Tek seferde en fazla faiz (para).</summary>
        public int Cap { get; }

        /// <summary>
        /// Paranın faizi: 300 → 30, 305 → 30, 9 → 0, 1000 → 50 (tavan). Para 0 veya negatifse 0.
        /// Oran decimal'e çevrilerek çarpılır; 0.7f gibi oranlarda float hatası bir eksik vermez (10 × 0.7 = 7).
        /// </summary>
        public int Calculate(int money)
        {
            if (money <= 0 || Rate <= 0f || Cap == 0)
            {
                return 0;
            }

            decimal interest = Math.Floor(money * (decimal)Rate);
            return interest >= Cap ? Cap : (int)interest;
        }
    }
}
