using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Bir gecenin dalga verisi (GDD §8): spawn grupları, opsiyonel gece olayı ve opsiyonel boss.
    /// Sıralı listesi <see cref="WaveConfig"/>'tedir. Asset'leri üretici oluşturur: <c>Data/Waves/</c>.
    /// </summary>
    public sealed class NightData : ScriptableObject
    {
        [Header("Gece")]
        [SerializeField, Min(1), Tooltip("Arayüzde görünen gece numarası (1 tabanlı). WaveConfig'teki sırası + 1 olmalı.")]
        private int _nightNumber = 1;

        [Header("Spawn grupları")]
        [SerializeField, Tooltip("Gruplar birbirinden bağımsız, kendi gecikme ve aralıklarıyla doğar.")]
        private List<SpawnGroup> _groups = new List<SpawnGroup>();

        [Header("Gece olayı (opsiyonel)")]
        [SerializeField, Tooltip("Opsiyonel gece olayı (GDD §14: Kan Ayı, Sis, Fırtına). Boşsa olay yok.")]
        private NightModifierData _modifier;

        [Header("Boss (opsiyonel)")]
        [SerializeField, Tooltip("Opsiyonel boss tipi (GDD §7: Brute). Boşsa boss yok. Tek düşman olarak doğar.")]
        private EnemyData _bossData;

        [SerializeField, Min(0f), Tooltip("Gece başından boss'un doğmasına kadar bekleme (sn).")]
        private float _bossStartDelay = 20f;

        [SerializeField, Tooltip("Boss'un doğacağı yön.")]
        private CompassDirection _bossDirection = CompassDirection.North;

        public int NightNumber => _nightNumber;

        public IReadOnlyList<SpawnGroup> Groups => _groups;

        /// <summary>Null olabilir.</summary>
        public NightModifierData Modifier => _modifier;

        /// <summary>Null olabilir.</summary>
        public EnemyData BossData => _bossData;

        public float BossStartDelay => _bossStartDelay;

        public CompassDirection BossDirection => _bossDirection;
    }
}
