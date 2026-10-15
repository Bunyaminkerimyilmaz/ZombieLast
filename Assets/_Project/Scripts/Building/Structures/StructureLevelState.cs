using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının seviyesi ve bunun Unity'deki karşılığı (<see cref="Structure"/>'ın parçası): seviye/harcama
    /// (<see cref="StructureProgress"/>), maks. can (<see cref="Health"/>) ve görsel (<see cref="StructureLevelVisual"/>).
    /// Yükseltmede maks. can artar, eksik can korunur (yükseltme bedava onarım değil).
    /// </summary>
    public sealed class StructureLevelState
    {
        private readonly StructureProgress _progress = new StructureProgress();
        private readonly Health _health;
        private readonly StructureLevelVisual _visual;
        private BuildableData _data;

        /// <param name="visual">Görsel kök ("Visual"); null olabilir.</param>
        public StructureLevelState(Health health, Transform visual)
        {
            _health = health ? health : throw new ArgumentNullException(nameof(health));
            _visual = new StructureLevelVisual(visual);
        }

        public StructureProgress Progress => _progress;

        /// <summary>Yerleşti: 1. seviye, tam can, 1. seviye görseli; harcama = inşa fiyatı.</summary>
        public void Reset(BuildableData data, int buildCost)
        {
            _data = data ? data : throw new ArgumentNullException(nameof(data));
            _progress.Reset(data.Levels, buildCost);
            Apply(refill: true);
        }

        /// <summary>Sonraki seviyeye geçer (fiyat harcamaya eklenir), canı ve görseli günceller. En üst seviyedeyse false.</summary>
        public bool Advance()
        {
            if (_data == null || !_progress.Advance())
            {
                return false;
            }

            Apply(refill: false);
            return true;
        }

        /// <summary>Prefab'daki görünüme döner (havuza dönerken).</summary>
        public void Restore()
        {
            _visual.Restore();
        }

        private void Apply(bool refill)
        {
            BuildableLevel level = _progress.Current;
            if (level != null)
            {
                float previousMax = _health.Max;
                _health.SetMax(level.MaxHealth, refill);
                if (!refill && level.MaxHealth > previousMax)
                {
                    _health.Heal(level.MaxHealth - previousMax);
                }
            }

            _visual.Apply(_data.Levels, _progress.LevelIndex);
        }
    }
}
