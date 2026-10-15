using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// <see cref="NightData"/> satırı: bir düşman tipinden belirli sayıda, sabit aralıkla doğan grup (GDD §8).
    /// Spawn'lar grubun yönlerine sırayla dağıtılır (K, D, K, D...).
    /// </summary>
    [Serializable]
    public sealed class SpawnGroup
    {
        [SerializeField, Tooltip("Düşman tipi. Boşsa grup atlanır (uyarıyla).")]
        private EnemyData _enemy;

        [SerializeField, Min(0), Tooltip("Gruptaki düşman sayısı.")]
        private int _count = 10;

        [SerializeField, Min(0f), Tooltip("Ardışık iki spawn arası süre (sn). GDD §8 varsayılanı 0.6–1.2. 0 = hepsi aynı anda.")]
        private float _spawnInterval = 0.9f;

        [SerializeField, Min(0f), Tooltip("Gece başından grubun ilk spawn'ına kadar bekleme (sn). Ağır düşmanları gecenin ortasına koymak için.")]
        private float _startDelay;

        [SerializeField, Tooltip("Grubun geleceği yönler. Spawn'lar bu sırayla dönüşümlü dağıtılır. Boşsa grup atlanır (uyarıyla).")]
        private List<CompassDirection> _directions = new List<CompassDirection>();

        /// <summary>Unity serileştirmesi ve inspector için.</summary>
        public SpawnGroup()
        {
        }

        /// <summary>Üretici ve testler için.</summary>
        public SpawnGroup(EnemyData enemy, int count, float spawnInterval, float startDelay, IEnumerable<CompassDirection> directions)
        {
            _enemy = enemy;
            _count = count;
            _spawnInterval = spawnInterval;
            _startDelay = startDelay;
            _directions = directions != null ? new List<CompassDirection>(directions) : new List<CompassDirection>();
        }

        /// <summary>Null olabilir.</summary>
        public EnemyData Enemy => _enemy;

        public int Count => _count;

        public float SpawnInterval => _spawnInterval;

        public float StartDelay => _startDelay;

        public IReadOnlyList<CompassDirection> Directions => _directions;
    }
}
