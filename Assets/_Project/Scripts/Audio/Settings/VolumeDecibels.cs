using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>Ayarlardaki doğrusal ses düzeyinin (0..1) mixer'a yazılan desibel karşılığı (saf).</summary>
    public static class VolumeDecibels
    {
        /// <summary>
        /// 20·log10(düzey), <paramref name="minDecibels"/>'in altına inmez (0 → sessiz); 1 → 0 dB (grubun kendi düzeyi). 1'in üstü 1 sayılır.
        /// </summary>
        public static float FromLinear(float linear, float minDecibels)
        {
            float clamped = Mathf.Clamp01(linear);
            if (clamped <= 0f)
            {
                return minDecibels;
            }

            return Mathf.Max(minDecibels, 20f * Mathf.Log10(clamped));
        }
    }
}
