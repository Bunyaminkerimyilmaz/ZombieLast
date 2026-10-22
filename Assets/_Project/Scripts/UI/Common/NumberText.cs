using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Sık yazılan sayıların önbellekli metni (hasar sayıları, "+12" jeton yazıları): aynı sayı için her seferinde yeni string
    /// oluşmaz. 0..<see cref="CacheSize"/>-1 önbelleğe alınır, dışı her çağrıda <see cref="HudFormat"/> ile üretilir.
    /// </summary>
    public static class NumberText
    {
        /// <summary>Önbelleğe alınan sayı aralığının üst sınırı (hariç).</summary>
        public const int CacheSize = 1000;

        private static string[] _counts = new string[CacheSize];
        private static string[] _gains = new string[CacheSize];

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _counts = new string[CacheSize];
            _gains = new string[CacheSize];
        }
    }
}
