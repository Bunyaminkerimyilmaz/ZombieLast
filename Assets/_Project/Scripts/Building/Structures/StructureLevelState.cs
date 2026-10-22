using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının seviyesi ve bunun Unity'deki karşılığı (<see cref="Structure"/>'ın parçası): seviye/harcama
    /// (<see cref="StructureProgress"/>), maks. can (<see cref="Health"/>) ve görsel (<see cref="StructureLevelVisual"/>).
    /// Yükseltmede maks. can artar, eksik can korunur (yükseltme bedava onarım değil). Barikatın canı run çarpanıyla değişir
    /// (<see cref="StructureHealthRule"/>, Sağlam Duvar); yerleşikken çarpan değişirse maks. can yeniden hesaplanır (artış kadar
    /// iyileşir, azalırsa kırpılır). Çarpan aboneliği yerleşmede açılır, havuza dönüşte (<see cref="Restore"/>) kapanır.
    /// </summary>
    public sealed class StructureLevelState
    {
        private readonly StructureProgress _progress = new StructureProgress();
        private readonly Health _health;
        private readonly StructureLevelVisual _visual;
        private BuildableData _data;
        private IRunModifiers _modifiers;

        /// <param name="visual">Görsel kök ("Visual"); null olabilir.</param>
        public StructureLevelState(Health health, Transform visual)
        {
            _health = health ? health : throw new ArgumentNullException(nameof(health));
            _visual = new StructureLevelVisual(visual);
        }

        public StructureProgress Progress => _progress;

        /// <summary>Yerleşti: 1. seviye, tam can, 1. seviye görseli; harcama = inşa fiyatı.</summary>
        /// <param name="modifiers">Run çarpanları (barikat canı); null olabilir.</param>
        public void Reset(BuildableData data, int buildCost, IRunModifiers modifiers = null)
        {
            _data = data ? data : throw new ArgumentNullException(nameof(data));
            Watch(modifiers);
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

        /// <summary>Maks. canı run çarpanıyla yeniden hesaplar (artış kadar iyileşir). Yerleşmeden önce etkisiz.</summary>
        public void RefreshMaxHealth()
        {
            if (_data != null)
            {
                ApplyHealth(refill: false);
            }
        }

        /// <summary>Prefab'daki görünüme döner ve çarpan aboneliğini kapatır (havuza dönerken).</summary>
        public void Restore()
        {
            Watch(null);
            _visual.Restore();
        }

        private void Apply(bool refill)
        {
            ApplyHealth(refill);
            _visual.Apply(_data.Levels, _progress.LevelIndex);
        }

        private void ApplyHealth(bool refill)
        {
            BuildableLevel level = _progress.Current;
            if (level == null)
            {
                return;
            }

            float max = StructureHealthRule.GetMaxHealth(_data.TargetType, level.MaxHealth, _modifiers);
            float previousMax = _health.Max;
            _health.SetMax(max, refill);
            if (!refill && max > previousMax)
            {
                _health.Heal(max - previousMax);
            }
        }

        private void Watch(IRunModifiers modifiers)
        {
            if (_modifiers != null)
            {
                _modifiers.StatChanged -= OnStatChanged;
            }

            _modifiers = modifiers;
            if (_modifiers != null && _data != null && StructureHealthRule.UsesModifiers(_data.TargetType))
            {
                _modifiers.StatChanged += OnStatChanged;
            }
        }

        private void OnStatChanged(StatType stat)
        {
            if (stat == StatType.BarricadeMaxHealth)
            {
                RefreshMaxHealth();
            }
        }
    }
}
