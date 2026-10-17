using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Bir gece olayının (GDD §14: Kan Ayı, Sis, Fırtına) tüm etkileri; <see cref="NightModifierActivatedEvent"/> taşır. Core gece
    /// verisini bilmediği için Waves'teki asset buraya çevrilir. Çarpan 1 / bayrak kapalı = o etki yok.
    /// </summary>
    public readonly struct NightModifierSettings
    {
        private const string SourcePrefix = "night_modifier:";

        public NightModifierSettings(
            string id,
            string displayName,
            string description,
            float enemyCountMultiplier,
            float enemySpeedMultiplier,
            float coinMultiplier,
            bool overridesFog,
            Color fogColor,
            float fogDensity,
            bool overridesLight,
            Color lightColor,
            Color ambientColor,
            bool hasPowerOutages,
            float outageInterval,
            float outageDuration)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            EnemyCountMultiplier = enemyCountMultiplier;
            EnemySpeedMultiplier = enemySpeedMultiplier;
            CoinMultiplier = coinMultiplier;
            OverridesFog = overridesFog;
            FogColor = fogColor;
            FogDensity = fogDensity;
            OverridesLight = overridesLight;
            LightColor = lightColor;
            AmbientColor = ambientColor;
            HasPowerOutages = hasPowerOutages;
            OutageInterval = outageInterval;
            OutageDuration = outageDuration;
            SourceId = SourcePrefix + Id;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Description { get; }

        /// <summary>Gecenin düşman adedi çarpanı (boss hariç). Kan Ayı 1.5.</summary>
        public float EnemyCountMultiplier { get; }

        /// <summary>Düşman yürüme hızı çarpanı. Sis 1.1.</summary>
        public float EnemySpeedMultiplier { get; }

        /// <summary>Jeton değeri çarpanı. Kan Ayı 2.</summary>
        public float CoinMultiplier { get; }

        /// <summary>Gece sisini değiştirir mi (Sis).</summary>
        public bool OverridesFog { get; }

        public Color FogColor { get; }

        /// <summary>Exponential Squared sis yoğunluğu.</summary>
        public float FogDensity { get; }

        /// <summary>Ay ışığını ve ambiyansı değiştirir mi (Kan Ayı: kırmızı).</summary>
        public bool OverridesLight { get; }

        public Color LightColor { get; }

        public Color AmbientColor { get; }

        /// <summary>Periyodik güç kesintisi var mı (Fırtına).</summary>
        public bool HasPowerOutages { get; }

        /// <summary>Kesintiler arası periyot (sn): her bu kadar saniyede bir kesinti başlar.</summary>
        public float OutageInterval { get; }

        /// <summary>Kesinti süresi (sn).</summary>
        public float OutageDuration { get; }

        /// <summary>Bu olayın geçici etkilerinin kaynak id'si (<see cref="IRunModifiers.RemoveBySource"/>): "night_modifier:&lt;id&gt;".</summary>
        public string SourceId { get; }
    }
}
