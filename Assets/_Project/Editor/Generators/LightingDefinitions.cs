using LastHaven.GameFlow;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Gündüz ve gece ışığının üretici değerleri (GDD §18: gündüz sıcak beyaz; gece koyu mavi ambiyans, fener ve üs ışıkları).
    /// Sayılar GDD'de yok; oyun kamerasından ekran görüntüsüyle seçildi. Asset'lere sadece ilk oluşturmada yazılır.
    /// </summary>
    public static class LightingDefinitions
    {
        // Gündüz = LightingProfile varsayılanları. Çok hafif pus (~%2): sahne sis açık kaydedilir; böylece build'de
        // "Automatic" fog stripping sis shader varyantlarını silmez (gece sisi ve 12.3'teki Sis olayı çalışır).
        public static readonly LightingDefinition Day = new LightingDefinition(
            sunRotation: new Vector3(50f, -30f, 0f),
            sunColor: new Color(1f, 0.96f, 0.88f),
            sunIntensity: 1.2f,
            ambientColor: new Color(0.5f, 0.5f, 0.52f),
            fogEnabled: true,
            fogColor: new Color(0.75f, 0.76f, 0.78f),
            fogDensity: 0.006f);

        // Ay: soluk mavi, gündüzden 60° farklı yönden (alacakaranlıkta gölgeler döner). Ortam koyu mavi, kenarlar sise gömülür.
        // Işık dışındaki zombiler de seçilebilsin diye ambiyans tamamen karanlık değil (fener ve üs ışıkları yine belirgin).
        public static readonly LightingDefinition Night = new LightingDefinition(
            sunRotation: new Vector3(60f, 30f, 0f),
            sunColor: new Color(0.45f, 0.55f, 0.9f),
            sunIntensity: 0.5f,
            ambientColor: new Color(0.16f, 0.2f, 0.36f),
            fogEnabled: true,
            fogColor: new Color(0.03f, 0.05f, 0.11f),
            fogDensity: 0.02f);

        public static readonly PostProcessDefinition DayPostProcess = new PostProcessDefinition(
            vignetteIntensity: 0.2f,
            vignetteSmoothness: 0.4f,
            vignetteColor: Color.black,
            postExposure: 0f,
            contrast: 5f,
            saturation: 5f,
            colorFilter: new Color(1f, 0.98f, 0.94f),
            bloomThreshold: 1f,
            bloomIntensity: 0.3f,
            bloomScatter: 0.6f);

        // Gece: daha koyu kenarlar, soğuk renk filtresi, düşük doygunluk; ışıklar (fener, namlu, üs) daha çok parlar.
        public static readonly PostProcessDefinition NightPostProcess = new PostProcessDefinition(
            vignetteIntensity: 0.4f,
            vignetteSmoothness: 0.5f,
            vignetteColor: new Color(0f, 0.01f, 0.04f),
            postExposure: 0f,
            contrast: 12f,
            saturation: -20f,
            colorFilter: new Color(0.78f, 0.86f, 1f),
            bloomThreshold: 0.85f,
            bloomIntensity: 1.2f,
            bloomScatter: 0.65f);
    }

    /// <summary>Bir <see cref="LightingProfile"/>'in üretici değerleri.</summary>
    public readonly struct LightingDefinition
    {
        public LightingDefinition(
            Vector3 sunRotation,
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

        /// <summary>Euler (derece).</summary>
        public Vector3 SunRotation { get; }

        public Color SunColor { get; }

        public float SunIntensity { get; }

        public Color AmbientColor { get; }

        public bool FogEnabled { get; }

        public Color FogColor { get; }

        public float FogDensity { get; }

        /// <summary>Değerleri profile yazar.</summary>
        public void WriteTo(LightingProfile profile)
        {
            var serialized = new SerializedObject(profile);
            serialized.FindProperty("_sunRotation").vector3Value = SunRotation;
            serialized.FindProperty("_sunColor").colorValue = SunColor;
            serialized.FindProperty("_sunIntensity").floatValue = SunIntensity;
            serialized.FindProperty("_ambientColor").colorValue = AmbientColor;
            serialized.FindProperty("_fogEnabled").boolValue = FogEnabled;
            serialized.FindProperty("_fogColor").colorValue = FogColor;
            serialized.FindProperty("_fogDensity").floatValue = FogDensity;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
