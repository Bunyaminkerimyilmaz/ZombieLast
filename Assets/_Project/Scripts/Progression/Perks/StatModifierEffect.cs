using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// Bir stata modifier ekler (<see cref="IRunModifiers.AddModifier"/>; kaynak = perk). Yüzdeler toplanır: Hızlı Tetik iki yığında
    /// +%30 (GDD §13). Statın tüketicisi (silah, oyuncu, taret, barikat, jeton) değişimi kendisi uygular.
    /// </summary>
    [Serializable]
    public sealed class StatModifierEffect : PerkEffect
    {
        [SerializeField, Tooltip("Değişen stat.")]
        private StatType _stat;

        [SerializeField, Tooltip("Flat: tabana eklenir (birim). PercentAdd: oran, yüzdeler toplanır (0.15 = +%15, -0.4 = -%40).")]
        private StatModifierType _type;

        [SerializeField, Tooltip("Flat'ta birim değer, PercentAdd'de oran.")]
        private float _value;

        public StatModifierEffect()
        {
        }

        public StatModifierEffect(StatType stat, StatModifierType type, float value)
        {
            _stat = stat;
            _type = type;
            _value = value;
        }

        public StatType Stat => _stat;

        public StatModifierType Type => _type;

        public float Value => _value;

        public override void Apply(in PerkEffectContext context)
        {
            context.Modifiers?.AddModifier(new StatModifier(_stat, _type, _value, context.SourceId));
        }
    }
}
