using System;
using System.Collections.Generic;
using LastHaven.Core;

namespace LastHaven.Progression
{
    /// <summary>
    /// Run'ın perk'leri (saf C#, GDD §13): perk başına yığın sayısı ve alım. <see cref="TryAcquire"/> maks. yığını kontrol eder,
    /// yığını artırır ve perk'in etkilerini sırayla uygular (<see cref="IRunModifiers"/>, iyileşme). Perk'ler run boyunca kalır.
    /// Id'ler ordinal karşılaştırılır. <see cref="PerkService"/> sarar; testler doğrudan kullanır.
    /// </summary>
    public sealed class PerkInventory
    {
        private readonly Dictionary<string, int> _stacks = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<string> _acquiredIds = new List<string>();
        private readonly IRunModifiers _modifiers;
        private readonly Action<float> _heal;

        /// <param name="modifiers">Stat ve bayrak etkilerinin yazıldığı yer; null ise o etkiler atlanır.</param>
        /// <param name="heal">İyileşme etkisi (oyuncuyu iyileştirir); null ise atlanır.</param>
        public PerkInventory(IRunModifiers modifiers, Action<float> heal = null)
        {
            _modifiers = modifiers;
            _heal = heal;
        }

        /// <summary>Alınan toplam yığın.</summary>
        public int TotalStacks { get; private set; }

        /// <summary>En az bir kez alınan perk'lerin id'leri, ilk alım sırasıyla.</summary>
        public IReadOnlyList<string> AcquiredIds => _acquiredIds;

        public int GetStacks(string perkId)
        {
            return perkId != null && _stacks.TryGetValue(perkId, out int stacks) ? stacks : 0;
        }

        /// <summary>Perk geçerli (id'si boş değil) ve maks. yığınına ulaşmamış mı.</summary>
        public bool CanAcquire(PerkData perk)
        {
            return perk != null && !string.IsNullOrEmpty(perk.Id) && GetStacks(perk.Id) < perk.MaxStacks;
        }

        /// <summary>Alınabiliyorsa yığını artırır ve etkileri uygular; alınamazsa hiçbir şey değişmez ve false döner.</summary>
        public bool TryAcquire(PerkData perk)
        {
            if (!CanAcquire(perk))
            {
                return false;
            }

            int stacks = GetStacks(perk.Id) + 1;
            _stacks[perk.Id] = stacks;
            if (stacks == 1)
            {
                _acquiredIds.Add(perk.Id);
            }

            TotalStacks++;
            var context = new PerkEffectContext(_modifiers, perk.SourceId, _heal);
            IReadOnlyList<PerkEffect> effects = perk.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i]?.Apply(context);
            }

            return true;
        }
    }
}
