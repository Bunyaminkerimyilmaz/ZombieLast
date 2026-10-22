using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// "değer/toplam" metinleri önbellekli (<see cref="HudFormat.OfTotal"/>; can barları, XP): toplam başına bir dizi, değer ilk
    /// görünüşte üretilir ve sonra aynı string döner. Her vuruşta değişen jeneratör ve oyuncu barları bağlanırken
    /// <see cref="Prefill"/> ile baştan hazırlanır; oynanışta hiç ayırma olmaz. <see cref="MaxCachedTotal"/>'ın üstü önbelleğe girmez.
    /// </summary>
    public static class FractionText
    {
        /// <summary>Önbelleğe alınan en büyük toplam (boss 6000 dahil).</summary>
        public const int MaxCachedTotal = 10000;

        /// <summary>Bundan büyük toplamlar baştan doldurulmaz (bir karede binlerce string üretmesin).</summary>
        public const int MaxPrefillTotal = 2000;

        private static Dictionary<int, string[]> _byTotal = new Dictionary<int, string[]>();

        /// <summary>"<paramref name="value"/>/<paramref name="total"/>"; değer 0..toplam dışındaysa veya toplam çok büyükse önbelleksiz.</summary>
        public static string Get(int value, int total)
        {
            if (total < 0 || total > MaxCachedTotal || value < 0 || value > total)
            {
                return HudFormat.OfTotal(value, total);
            }

            string[] texts = GetTexts(total);
            return texts[value] ??= HudFormat.OfTotal(value, total);
        }

        /// <summary>Toplamın tüm değerlerini şimdi üretir (en çok <see cref="MaxPrefillTotal"/>; büyükse ilk görünüşte üretilir).</summary>
        public static void Prefill(int total)
        {
            if (total < 0 || total > MaxPrefillTotal)
            {
                return;
            }

            string[] texts = GetTexts(total);
            for (int i = 0; i <= total; i++)
            {
                texts[i] ??= HudFormat.OfTotal(i, total);
            }
        }

        private static string[] GetTexts(int total)
        {
            if (!_byTotal.TryGetValue(total, out string[] texts))
            {
                texts = new string[total + 1];
                _byTotal.Add(total, texts);
            }

            return texts;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _byTotal = new Dictionary<int, string[]>();
        }
    }
}
