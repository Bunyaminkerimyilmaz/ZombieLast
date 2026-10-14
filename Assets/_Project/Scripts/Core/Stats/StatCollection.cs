using System;
using System.Collections.Generic;

namespace LastHaven.Core
{
    /// <summary>
    /// Stat modifier'larını tutar ve taban değere uygular (saf C#).
    /// Sonuç = (taban + Σflat) × max(0, 1 + Σyüzde). Toplamlar önbelleklidir; <see cref="Get"/> bellek ayırmaz.
    /// </summary>
    public sealed class StatCollection
    {
        private static readonly int StatCount = Enum.GetValues(typeof(StatType)).Length;

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private readonly float[] _flat = new float[StatCount];
        private readonly float[] _percent = new float[StatCount];
        private readonly bool[] _changedScratch = new bool[StatCount];

        /// <summary>Bir statın modifier'ları değişince, etkilenen her stat için bir kez çağrılır.</summary>
        public event Action<StatType> Changed;

        /// <summary>Tüm statlardaki toplam modifier sayısı.</summary>
        public int Count => _modifiers.Count;

        /// <summary>Taban değere bu statın modifier'larını uygular.</summary>
        public float Get(StatType stat, float baseValue)
        {
            int index = (int)stat;
            float multiplier = 1f + _percent[index];
            return (baseValue + _flat[index]) * (multiplier > 0f ? multiplier : 0f);
        }

        /// <summary>Bu statın flat modifier toplamı.</summary>
        public float GetFlat(StatType stat)
        {
            return _flat[(int)stat];
        }

        /// <summary>Bu statın yüzde modifier toplamı (oran: 0.15 = +%15).</summary>
        public float GetPercent(StatType stat)
        {
            return _percent[(int)stat];
        }

        public void Add(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            Recalculate(modifier.Stat);
            Changed?.Invoke(modifier.Stat);
        }

        /// <summary>Bu kaynağın tüm modifier'larını kaldırır; kaldırılan sayısını döndürür.</summary>
        public int RemoveBySource(string sourceId)
        {
            Array.Clear(_changedScratch, 0, StatCount);
            int removed = 0;
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (!string.Equals(_modifiers[i].SourceId, sourceId, StringComparison.Ordinal))
                {
                    continue;
                }

                _changedScratch[(int)_modifiers[i].Stat] = true;
                _modifiers.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
            {
                RecalculateAndNotifyChanged();
            }

            return removed;
        }

        /// <summary>Tüm modifier'ları kaldırır.</summary>
        public void Clear()
        {
            Array.Clear(_changedScratch, 0, StatCount);
            for (int i = 0; i < _modifiers.Count; i++)
            {
                _changedScratch[(int)_modifiers[i].Stat] = true;
            }

            _modifiers.Clear();
            RecalculateAndNotifyChanged();
        }

        private void RecalculateAndNotifyChanged()
        {
            for (int i = 0; i < StatCount; i++)
            {
                if (_changedScratch[i])
                {
                    Recalculate((StatType)i);
                }
            }

            // Önce hepsi güncellenir, sonra haber verilir: dinleyici diğer statları okuyabilir.
            for (int i = 0; i < StatCount; i++)
            {
                if (_changedScratch[i])
                {
                    Changed?.Invoke((StatType)i);
                }
            }
        }

        // Toplam her seferinde baştan hesaplanır; ekle/çıkar tekrarında float kayması birikmesin.
        private void Recalculate(StatType stat)
        {
            float flat = 0f;
            float percent = 0f;
            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier modifier = _modifiers[i];
                if (modifier.Stat != stat)
                {
                    continue;
                }

                if (modifier.Type == StatModifierType.Flat)
                {
                    flat += modifier.Value;
                }
                else
                {
                    percent += modifier.Value;
                }
            }

            _flat[(int)stat] = flat;
            _percent[(int)stat] = percent;
        }
    }
}
