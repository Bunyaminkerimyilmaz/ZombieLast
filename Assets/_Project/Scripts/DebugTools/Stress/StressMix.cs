using System;
using System.Collections.Generic;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Stres testinin düşman karışımı (saf C#): ağırlıklar sırayla döner (ör. 16 Yürüyen, 7 Koşucu, 2 Tank, …, sonra baştan), böylece
    /// kaç düşman doğarsa doğsun oranlar korunur ve sonuç her çalıştırmada aynıdır. Ayrıca havuz payı ve kare başı doğum bütçesi.
    /// </summary>
    public sealed class StressMix
    {
        private readonly int[] _weights;

        /// <param name="weights">Tip başına ağırlık; 0 veya negatif olan tip hiç doğmaz.</param>
        public StressMix(IReadOnlyList<int> weights)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            _weights = new int[weights.Count];
            for (int i = 0; i < weights.Count; i++)
            {
                _weights[i] = Math.Max(0, weights[i]);
                TotalWeight += _weights[i];
            }
        }

        /// <summary>Ağırlıkların toplamı; 0 ise hiç tip seçilmez.</summary>
        public int TotalWeight { get; }

        /// <summary><paramref name="spawnIndex"/>. (0 tabanlı) doğumun tip indeksi; toplam ağırlık 0 ise -1.</summary>
        public int PickIndex(int spawnIndex)
        {
            if (TotalWeight <= 0)
            {
                return -1;
            }

            int slot = (int)((uint)spawnIndex % (uint)TotalWeight);
            for (int i = 0; i < _weights.Length; i++)
            {
                if (slot < _weights[i])
                {
                    return i;
                }

                slot -= _weights[i];
            }

            return -1;
        }

        /// <summary><paramref name="total"/> düşmanda tipin en çok adedi (yukarı yuvarlı): havuz ön ısıtması.</summary>
        public int Share(int typeIndex, int total)
        {
            if (TotalWeight <= 0 || typeIndex < 0 || typeIndex >= _weights.Length || total <= 0)
            {
                return 0;
            }

            return (int)((total * (long)_weights[typeIndex] + TotalWeight - 1) / TotalWeight);
        }

        /// <summary>Bu karede doğurulacak sayı: hedefe kalan, en çok <paramref name="perFrame"/>; negatif olmaz.</summary>
        public static int SpawnBudget(int alive, int target, int perFrame)
        {
            return Math.Max(0, Math.Min(target - alive, perFrame));
        }
    }
}
