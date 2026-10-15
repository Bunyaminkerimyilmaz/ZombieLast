using System;
using System.Collections.Generic;

namespace LastHaven.Economy
{
    /// <summary>
    /// Ödülün jetonlara bölünmesi (GDD §10: 1–5 jeton, saf). Jeton sayısı = toplam / <see cref="TargetCoinValue"/>
    /// (yukarı yuvarlı), [<see cref="MinCoins"/>, <see cref="MaxCoins"/>] aralığına kırpılır; her jeton en az 1 değerinde olur.
    /// Kalan, ilk jetonlara birer birer dağıtılır; toplam hiç kaybolmaz.
    /// </summary>
    public readonly struct CoinSplit
    {
        public CoinSplit(int minCoins, int maxCoins, int targetCoinValue)
        {
            MinCoins = Math.Max(1, minCoins);
            MaxCoins = Math.Max(MinCoins, maxCoins);
            TargetCoinValue = Math.Max(1, targetCoinValue);
        }

        /// <summary>Bir ödülün bölündüğü en az jeton sayısı (≥ 1).</summary>
        public int MinCoins { get; }

        /// <summary>Bir ödülün bölündüğü en fazla jeton sayısı.</summary>
        public int MaxCoins { get; }

        /// <summary>Jeton başına hedef değer: 5 → 10 para 2 jeton, 12 para 3 jeton.</summary>
        public int TargetCoinValue { get; }

        /// <summary>Toplamın kaç jetona bölüneceği; toplam 0 veya negatifse 0.</summary>
        public int GetCount(int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            // default(CoinSplit)'te özellikler 0 olabilir; bölme ve kırpma güvenli kalsın.
            int step = Math.Max(1, TargetCoinValue);
            int min = Math.Max(1, MinCoins);
            int max = Math.Max(min, MaxCoins);

            int count = total / step + (total % step > 0 ? 1 : 0);
            count = Math.Max(min, Math.Min(max, count));
            return Math.Min(count, total);
        }

        /// <summary>
        /// Toplamı jeton değerlerine böler: 13 → 3 jeton (5, 4, 4). <paramref name="values"/> temizlenip doldurulur
        /// (bellek ayırmamak için çağıran tekrar kullanır). Jeton sayısını döndürür.
        /// </summary>
        public int Split(int total, List<int> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            values.Clear();
            int count = GetCount(total);
            if (count == 0)
            {
                return 0;
            }

            int baseValue = total / count;
            int remainder = total % count;
            for (int i = 0; i < count; i++)
            {
                values.Add(i < remainder ? baseValue + 1 : baseValue);
            }

            return count;
        }
    }
}
