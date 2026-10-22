using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// Davranış bayrağı açar (<see cref="IRunModifiers.AddFlag"/>; GDD §13: Seken Mermi, Vampir, Dikenli Duvar, Şok Taret,
    /// Zincirleme Son, Avcı). Etkinin değeri bayrağı soran modülün config'inde.
    /// </summary>
    [Serializable]
    public sealed class FlagEffect : PerkEffect
    {
        [SerializeField, Tooltip("Açılan bayrak.")]
        private PerkFlag _flag;

        public FlagEffect()
        {
        }

        public FlagEffect(PerkFlag flag)
        {
            _flag = flag;
        }

        public PerkFlag Flag => _flag;

        public override void Apply(in PerkEffectContext context)
        {
            context.Modifiers?.AddFlag(_flag);
        }
    }
}
