using System;

namespace LastHaven.Core
{
    /// <summary>Modifier'ın taban değere nasıl uygulandığı.</summary>
    public enum StatModifierType
    {
        /// <summary>Tabana eklenir (ör. Kalın Deri +25 can, market Mıknatıs +1 m).</summary>
        Flat = 0,

        /// <summary>
        /// Yüzdeler toplanır, sonra çarpılır: 0.15 = +%15, -0.4 = -%40.
        /// Sonuç = (taban + Σflat) × (1 + Σyüzde).
        /// </summary>
        PercentAdd = 1,
    }

    /// <summary>Bir stata uygulanan değişiklik. <see cref="SourceId"/> ile toplu kaldırılır (perk, gece olayı, market öğesi).</summary>
    public readonly struct StatModifier
    {
        public StatModifier(StatType stat, StatModifierType type, float value, string sourceId)
        {
            if (string.IsNullOrEmpty(sourceId))
            {
                throw new ArgumentException("Modifier'ın kaynak id'si boş olamaz; RemoveBySource bununla kaldırır.", nameof(sourceId));
            }

            Stat = stat;
            Type = type;
            Value = value;
            SourceId = sourceId;
        }

        public StatType Stat { get; }

        public StatModifierType Type { get; }

        /// <summary>Flat'ta birim değer; PercentAdd'de oran (0.15 = +%15).</summary>
        public float Value { get; }

        /// <summary>Modifier'ı ekleyenin id'si (ör. perk id'si). Karşılaştırma ordinal.</summary>
        public string SourceId { get; }

        public static StatModifier Flat(StatType stat, float value, string sourceId)
        {
            return new StatModifier(stat, StatModifierType.Flat, value, sourceId);
        }

        /// <summary>Oran olarak: 0.15 = +%15.</summary>
        public static StatModifier Percent(StatType stat, float value, string sourceId)
        {
            return new StatModifier(stat, StatModifierType.PercentAdd, value, sourceId);
        }

        public override string ToString()
        {
            return Type == StatModifierType.Flat
                ? $"{Stat} {Value:+0.##;-0.##} ({SourceId})"
                : $"{Stat} {Value * 100f:+0.##;-0.##}% ({SourceId})";
        }
    }
}
