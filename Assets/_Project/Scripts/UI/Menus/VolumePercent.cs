using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>Ayarlar ekranındaki ses kaydırıcılarının yüzde değeri ↔ ayarların 0..1 düzeyi (saf).</summary>
    public static class VolumePercent
    {
        public const int Min = 0;
        public const int Max = 100;

        /// <summary>0..1 düzey → 0..100 yüzde (en yakın tam sayı).</summary>
        public static int FromVolume(float volume)
        {
            return Mathf.Clamp(Mathf.RoundToInt(volume * Max), Min, Max);
        }

        /// <summary>0..100 yüzde → 0..1 düzey.</summary>
        public static float ToVolume(int percent)
        {
            return Mathf.Clamp(percent, Min, Max) / (float)Max;
        }

        /// <summary>Klavye / gamepad adımı: <paramref name="direction"/> yönünde (+1 / -1) <paramref name="step"/> kadar, 0..100'de kalır.</summary>
        public static int Step(int percent, int direction, int step)
        {
            return Mathf.Clamp(percent + System.Math.Sign(direction) * Mathf.Max(1, step), Min, Max);
        }
    }
}
