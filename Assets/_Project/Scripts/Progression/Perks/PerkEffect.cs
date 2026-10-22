using System;
using LastHaven.Core;

namespace LastHaven.Progression
{
    /// <summary>
    /// Perk'in bir etkisi (GDD §13). <see cref="PerkData"/>'nın listesinde <c>[SerializeReference]</c> ile durur; perk her alındığında
    /// (yığın başına bir kez) sırayla uygulanır. Perk'ler run boyunca kalır, etkiler geri alınmaz.
    /// </summary>
    [Serializable]
    public abstract class PerkEffect
    {
        public abstract void Apply(in PerkEffectContext context);
    }

    /// <summary>Etkinin uygulandığı yer: run çarpanları, modifier kaynak id'si ve oyuncuyu iyileştirme.</summary>
    public readonly struct PerkEffectContext
    {
        /// <param name="modifiers">Null ise stat ve bayrak etkileri atlanır.</param>
        /// <param name="sourceId">Eklenen modifier'ların kaynağı (perk başına aynı; yığınlar toplanır).</param>
        /// <param name="heal">Oyuncuyu iyileştirir; null ise iyileşme etkileri atlanır.</param>
        public PerkEffectContext(IRunModifiers modifiers, string sourceId, Action<float> heal)
        {
            Modifiers = modifiers;
            SourceId = sourceId;
            Heal = heal;
        }

        public IRunModifiers Modifiers { get; }

        public string SourceId { get; }

        public Action<float> Heal { get; }
    }
}
