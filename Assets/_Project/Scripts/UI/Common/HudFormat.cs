using System.Globalization;
using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'daki sayıların metne çevrilmesi (saf). Sadece sayı ve sembol üretir; Türkçe başlıklar UXML'dedir.
    /// Kültürden bağımsızdır (tr-TR'de de aynı çıktı).
    /// </summary>
    public static class HudFormat
    {
        /// <summary>Sonsuz yedek mermi sembolü.</summary>
        public const string Infinite = "∞";

        /// <summary>Çarpan sembolü.</summary>
        public const string MultiplierSign = "×";

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>Kalan saniyeyi "d:ss" yapar (125 → "2:05", 9 → "0:09"). Negatif 0 sayılır.</summary>
        public static string Timer(int totalSeconds)
        {
            if (totalSeconds < 0)
            {
                totalSeconds = 0;
            }

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return minutes.ToString(Invariant) + ":" + seconds.ToString("00", Invariant);
        }

        /// <summary>
        /// Tahmini süreyi (sn) ekranda gösterilecek tam saniyeye çevirir: yukarı yuvarlı, negatif 0; sonsuz veya NaN → -1.
        /// Metin sadece bu değer değişince yeniden kurulur.
        /// </summary>
        public static int DisplaySeconds(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds >= int.MaxValue)
            {
                return -1;
            }

            return seconds > 0f ? Mathf.CeilToInt(seconds) : 0;
        }

        /// <summary>Tahmini süre metni: <see cref="DisplaySeconds"/> değeri "d:ss" (250 → "4:10"); -1 (sonsuz) → "∞".</summary>
        public static string Estimate(int displaySeconds)
        {
            return displaySeconds < 0 ? Infinite : Timer(displaySeconds);
        }

        /// <summary>"değer/toplam" (gece 3/10).</summary>
        public static string OfTotal(int value, int total)
        {
            return value.ToString(Invariant) + "/" + total.ToString(Invariant);
        }

        /// <summary>Tam sayı (öldürme, kalan düşman).</summary>
        public static string Count(int value)
        {
            return value.ToString(Invariant);
        }

        /// <summary>Çarpan: "×1.5", "×2" (en fazla iki ondalık, gereksiz sıfırsız).</summary>
        public static string Multiplier(float multiplier)
        {
            return MultiplierSign + multiplier.ToString("0.##", Invariant);
        }

        /// <summary>Kazanç: "+35" (0 → "+0"; negatif işaretiyle).</summary>
        public static string Gain(int value)
        {
            return value >= 0 ? "+" + value.ToString(Invariant) : value.ToString(Invariant);
        }

        /// <summary>Bar doluluğu: current / max, 0..1 arasına kırpılmış; max ≤ 0 ise 0.</summary>
        public static float Ratio(float current, float max)
        {
            return max > 0f ? Mathf.Clamp01(current / max) : 0f;
        }

        /// <summary>
        /// Ekranda görünen can: yukarı yuvarlanır (0.4 can kalan canlı hedef "0" görünmesin), 0 ve altı 0.
        /// </summary>
        public static int DisplayHealth(float health)
        {
            return health > 0f ? Mathf.CeilToInt(health) : 0;
        }

        /// <summary>"mevcut/maks" can metni (yukarı yuvarlı).</summary>
        public static string Health(float current, float max)
        {
            return OfTotal(DisplayHealth(current), DisplayHealth(max));
        }
    }
}
