using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Sık yazılan sayıların önbellekli metni (hasar sayıları, "+12" jeton yazıları, kalan düşman, kombo, mermi, sayaçlar): aynı sayı
    /// için her seferinde yeni string oluşmaz. 0..<see cref="CacheSize"/>-1 önbelleğe alınır, dışı her çağrıda <see cref="HudFormat"/>
    /// ile üretilir. <see cref="PrefillCounts"/> HUD açılırken sayıları baştan hazırlar: oynanışta ilk görünüşte de ayırma olmaz.
    /// </summary>
    public static class NumberText
    {
        /// <summary>Önbelleğe alınan sayı aralığının üst sınırı (hariç).</summary>
        public const int CacheSize = 1000;

        private static string[] _counts = new string[CacheSize];
        private static string[] _gains = new string[CacheSize];
        private static string[] _timers = new string[CacheSize];

        /// <summary>"12" (<see cref="HudFormat.Count"/> ile aynı).</summary>
        public static string Count(int value)
        {
            if (value < 0 || value >= CacheSize)
            {
                return HudFormat.Count(value);
            }

            return _counts[value] ??= HudFormat.Count(value);
        }

        /// <summary>"+12" (<see cref="HudFormat.Gain"/> ile aynı).</summary>
        public static string Gain(int value)
        {
            if (value < 0 || value >= CacheSize)
            {
                return HudFormat.Gain(value);
            }

            return _gains[value] ??= HudFormat.Gain(value);
        }

        /// <summary>"d:ss" (<see cref="HudFormat.Timer"/> ile aynı; 0..999 sn önbellekte: gündüz sayacı, yakıt tahmini).</summary>
        public static string Timer(int seconds)
        {
            if (seconds < 0 || seconds >= CacheSize)
            {
                return HudFormat.Timer(seconds);
            }

            return _timers[seconds] ??= HudFormat.Timer(seconds);
        }

        /// <summary>Tahmini süre (<see cref="HudFormat.Estimate"/> ile aynı): -1 → "∞", değilse önbellekli <see cref="Timer"/>.</summary>
        public static string Estimate(int displaySeconds)
        {
            return displaySeconds < 0 ? HudFormat.Infinite : Timer(displaySeconds);
        }

        /// <summary>Tüm <see cref="Count"/> metinlerini şimdi üretir (HUD açılışında bir kez; ~1000 kısa string).</summary>
        public static void PrefillCounts()
        {
            for (int i = 0; i < CacheSize; i++)
            {
                _counts[i] ??= HudFormat.Count(i);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _counts = new string[CacheSize];
            _gains = new string[CacheSize];
            _timers = new string[CacheSize];
        }
    }
}
