using System;
using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>Stres testi karışımının bir satırı: düşman tipi ve ağırlığı (<see cref="StressMix"/>).</summary>
    [Serializable]
    public struct StressEnemy
    {
        [SerializeField, Tooltip("Düşman tipi.")]
        private EnemyData _enemy;

        [SerializeField, Min(0), Tooltip("Ağırlık: karışımdaki payı (ör. 16 Yürüyen, 7 Koşucu).")]
        private int _weight;

        public StressEnemy(EnemyData enemy, int weight)
        {
            _enemy = enemy;
            _weight = weight;
        }

        public EnemyData Enemy => _enemy;

        public int Weight => _weight;
    }
}
