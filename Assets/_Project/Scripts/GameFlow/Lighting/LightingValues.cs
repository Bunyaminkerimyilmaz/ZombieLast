using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Bir andaki sahne ışığı: güneş/ay (directional), düz ambiyans ve sis. <see cref="LightingProfile"/>'den okunur,
    /// iki profil arasında <see cref="Lerp"/> ile harmanlanır, <see cref="ApplyTo"/> ile sahneye yazılır.
    /// </summary>
    public readonly struct LightingValues
    {
        public LightingValues(
            Quaternion sunRotation,
            Color sunColor,
            float sunIntensity,
            Color ambientColor,
            bool fogEnabled,
            Color fogColor,
            float fogDensity)
        {
            SunRotation = sunRotation;
            SunColor = sunColor;
            SunIntensity = sunIntensity;
            AmbientColor = ambientColor;
            FogEnabled = fogEnabled;
            FogColor = fogColor;
            FogDensity = fogDensity;
        }

        public Quaternion SunRotation { get; }

        public Color SunColor { get; }

        public float SunIntensity { get; }

        public Color AmbientColor { get; }

        public bool FogEnabled { get; }

        public Color FogColor { get; }

        /// <summary>Exponential Squared sis yoğunluğu.</summary>
        public float FogDensity { get; }

        /// <summary>
        /// İki ışık durumu arasında harmanlar (t 0–1'e kırpılır; uçlarda değerler aynen döner). Kapalı sis yoğunluğu 0 sayılır:
        /// sis açılırken/kapanırken yoğunluk 0'dan kayar ve açık olan tarafın rengi kullanılır.
        /// </summary>
        public static LightingValues Lerp(in LightingValues from, in LightingValues to, float t)
        {
            if (t <= 0f)
            {
                return from;
            }

            if (t >= 1f)
            {
                return to;
            }

            float fogDensity = Mathf.Lerp(from.FogEnabled ? from.FogDensity : 0f, to.FogEnabled ? to.FogDensity : 0f, t);
            Color fogColor = !from.FogEnabled ? to.FogColor
                : !to.FogEnabled ? from.FogColor
                : Color.Lerp(from.FogColor, to.FogColor, t);

            return new LightingValues(
                Quaternion.Slerp(from.SunRotation, to.SunRotation, t),
                Color.Lerp(from.SunColor, to.SunColor, t),
                Mathf.Lerp(from.SunIntensity, to.SunIntensity, t),
                Color.Lerp(from.AmbientColor, to.AmbientColor, t),
                fogDensity > 0f,
                fogColor,
                fogDensity);
        }

        /// <summary>
        /// Güneşe (null olabilir) ve <see cref="RenderSettings"/>'e yazar. Ambiyans düz (Flat) renk, sis Exponential Squared moduna alınır.
        /// </summary>
        public void ApplyTo(Light sun)
        {
            if (sun != null)
            {
                sun.transform.rotation = SunRotation;
                sun.color = SunColor;
                sun.intensity = SunIntensity;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientColor;
            RenderSettings.fog = FogEnabled;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogDensity = FogDensity;
        }
    }
}
