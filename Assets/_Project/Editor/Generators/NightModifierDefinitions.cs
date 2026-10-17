using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// GDD §14 gece olayları: Kan Ayı, Sis, Fırtına. Id'ler <see cref="NightDefinitions"/>'taki sabitler (geceler bunlarla bağlanır).
    /// Değerler asset'lere sadece ilk oluşturmada yazılır. Renk ve sis yoğunluğu GDD'de yok; ekran görüntüsüyle seçildi.
    /// </summary>
    public static class NightModifierDefinitions
    {
        // Kan Ayı: düşman ×1.5, jeton ×2, kırmızı ay ışığı.
        public static readonly NightModifierDefinition BloodMoon = new NightModifierDefinition(
            "BloodMoon", NightDefinitions.BloodMoonId, "Kan Ayı", "Düşmanlar 1.5 kat kalabalık, jetonlar 2 kat değerli.",
            enemyCountMultiplier: 1.5f, coinMultiplier: 2f,
            overrideLight: true, lightColor: new Color(0.95f, 0.2f, 0.14f), ambientColor: new Color(0.32f, 0.08f, 0.08f));

        // Sis: görüş ~12 m, düşman hızı +%10. Kamera oyuncudan ~22 m uzakta; yoğunluk 12 m ötesini neredeyse gizler.
        public static readonly NightModifierDefinition Fog = new NightModifierDefinition(
            "Fog", NightDefinitions.FogId, "Sis", "Görüş azalır (~12 m), düşmanlar %10 hızlı.",
            enemySpeedMultiplier: 1.1f,
            overrideFog: true, fogColor: new Color(0.14f, 0.16f, 0.2f), fogDensity: 0.035f);

        // Fırtına: her 25 sn'de 5 sn güç kesintisi (şimşek ışık tarafında).
        public static readonly NightModifierDefinition Storm = new NightModifierDefinition(
            "Storm", NightDefinitions.StormId, "Fırtına", "Her 25 sn'de şimşek: 5 sn güç kesintisi (taretler ve üs ışıkları durur).",
            powerOutages: true, outageInterval: 25f, outageDuration: 5f);

        public static readonly IReadOnlyList<NightModifierDefinition> All = new[] { BloodMoon, Fog, Storm };
    }

    /// <summary>Bir gece olayının üretici tanımı.</summary>
    public readonly struct NightModifierDefinition
    {
        public NightModifierDefinition(
            string assetName,
            string id,
            string displayName,
            string description,
            float enemyCountMultiplier = 1f,
            float enemySpeedMultiplier = 1f,
            float coinMultiplier = 1f,
            bool overrideFog = false,
            Color fogColor = default,
            float fogDensity = 0f,
            bool overrideLight = false,
            Color lightColor = default,
            Color ambientColor = default,
            bool powerOutages = false,
            float outageInterval = 25f,
            float outageDuration = 5f)
        {
            AssetName = assetName;
            Id = id;
            DisplayName = displayName;
            Description = description;
            EnemyCountMultiplier = enemyCountMultiplier;
            EnemySpeedMultiplier = enemySpeedMultiplier;
            CoinMultiplier = coinMultiplier;
            OverrideFog = overrideFog;
            FogColor = fogColor;
            FogDensity = fogDensity;
            OverrideLight = overrideLight;
            LightColor = lightColor;
            AmbientColor = ambientColor;
            PowerOutages = powerOutages;
            OutageInterval = outageInterval;
            OutageDuration = outageDuration;
        }

        public string AssetName { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public float EnemyCountMultiplier { get; }
        public float EnemySpeedMultiplier { get; }
        public float CoinMultiplier { get; }
        public bool OverrideFog { get; }
        public Color FogColor { get; }
        public float FogDensity { get; }
        public bool OverrideLight { get; }
        public Color LightColor { get; }
        public Color AmbientColor { get; }
        public bool PowerOutages { get; }
        public float OutageInterval { get; }
        public float OutageDuration { get; }

        public override string ToString()
        {
            return AssetName;
        }
    }
}
