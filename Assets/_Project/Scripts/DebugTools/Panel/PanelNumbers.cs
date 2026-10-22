using System.Globalization;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug panelinin küçük tam sayı metinleri, önbellekli (0..<see cref="CacheSize"/>−1): debug panelinin her yenilemede yazdığı sayılar (FPS,
    /// düşman, GC) string ayırmasın; panel kendi ölçtüğü GC'yi kirletmesin. Aralık dışı sayılar her seferinde çevrilir.
    /// </summary>
    public static class PanelNumbers
    {
        public const int CacheSize = 1024;

        private static string[] _cache = new string[CacheSize];

        public static string Get(int value)
        {
            if (value < 0 || value >= CacheSize)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            return _cache[value] ??= value.ToString(CultureInfo.InvariantCulture);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _cache = new string[CacheSize];
        }
    }
}
