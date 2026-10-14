using System;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="IRunModifiers"/>'ın saf C# implementasyonu. Statlar <see cref="StatCollection"/>'da, bayraklar dizide.
    /// Sahnede <see cref="RunModifiersHost"/> bunu oluşturup servis olarak kaydeder; testler doğrudan kullanır.
    /// </summary>
    public sealed class RunModifiers : IRunModifiers
    {
        private readonly StatCollection _stats = new StatCollection();
        private readonly bool[] _flags = new bool[Enum.GetValues(typeof(PerkFlag)).Length];

        public RunModifiers()
        {
            _stats.Changed += stat => StatChanged?.Invoke(stat);
        }

        public event Action<StatType> StatChanged;

        public event Action<PerkFlag> FlagAdded;

        /// <summary>Toplam modifier sayısı (debug ve testler için).</summary>
        public int ModifierCount => _stats.Count;

        public float Get(StatType stat, float baseValue)
        {
            return _stats.Get(stat, baseValue);
        }

        public void AddModifier(StatModifier modifier)
        {
            _stats.Add(modifier);
        }

        public int RemoveBySource(string sourceId)
        {
            return _stats.RemoveBySource(sourceId);
        }

        public bool HasFlag(PerkFlag flag)
        {
            return _flags[(int)flag];
        }

        public void AddFlag(PerkFlag flag)
        {
            if (_flags[(int)flag])
            {
                return;
            }

            _flags[(int)flag] = true;
            FlagAdded?.Invoke(flag);
        }
    }
}
