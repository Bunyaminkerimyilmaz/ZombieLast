using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Perk bayraklarının (<see cref="PerkFlag"/>) düşman tarafındaki değerleri (GDD §13). Bayrağı perk sistemi açar; ne kadar etki
    /// edeceği burada. Asset: <c>Data/Enemies/EnemyPerkConfig.asset</c>; [Systems]/EnemySpawner'daki <see cref="ChainExplosionPerk"/>
    /// bağlıdır.
    /// </summary>
    public sealed class EnemyPerkConfig : ScriptableObject
    {
        [Header("Zincirleme Son (GDD §13)")]
        [SerializeField, Range(0f, 1f), Tooltip("Oyuncunun öldürdüğü düşmanın patlama olasılığı. 0.1 = %10.")]
        private float _chainExplosionChance = 0.1f;

        [SerializeField, Min(0f), Tooltip("Patlama yarıçapı (m): merkezden collider yüzeyine.")]
        private float _chainExplosionRadius = 3f;

        [SerializeField, Min(0f), Tooltip("Patlamanın merkezdeki hasarı.")]
        private float _chainExplosionDamage = 40f;

        [SerializeField, Range(0f, 1f), Tooltip("Yarıçap sınırındaki hasarın merkeze oranı. 1 = mesafeyle azalmaz (Patlayan gibi). GDD'de yok.")]
        private float _chainExplosionEdgeDamageFraction = 1f;

        [SerializeField, Tooltip("Hasar alan layer'lar. Sadece düşmanlar: perk patlaması oyuncuya ve yapılara vurmaz.")]
        private LayerMask _chainExplosionLayers = GameLayers.EnemyMask;

        [SerializeField, Min(0f), Tooltip("Patlama merkezinin ölüm noktasından (zemin) yüksekliği (m); gövde ortası.")]
        private float _chainExplosionHeight = 1f;

        [SerializeField, Min(1), Tooltip("Bir patlamada okunan en fazla collider. Kalabalıkta fazlası o patlamada etkilenmez.")]
        private int _chainExplosionBufferSize = 64;

        [Header("Efekt")]
        [SerializeField, Tooltip("Opsiyonel patlama efekti (havuzdan, PooledLifetime'lı). Üretici Patlayan'ın ExplosionFx'ini bağlar.")]
        private GameObject _explosionVfx;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanan efekt sayısı.")]
        private int _vfxPrewarm = 4;

        public float ChainExplosionChance => _chainExplosionChance;

        public float ChainExplosionRadius => _chainExplosionRadius;

        public float ChainExplosionDamage => _chainExplosionDamage;

        public float ChainExplosionEdgeDamageFraction => _chainExplosionEdgeDamageFraction;

        public LayerMask ChainExplosionLayers => _chainExplosionLayers;

        public float ChainExplosionHeight => _chainExplosionHeight;

        public int ChainExplosionBufferSize => Mathf.Max(1, _chainExplosionBufferSize);

        /// <summary>Null olabilir.</summary>
        public GameObject ExplosionVfx => _explosionVfx;

        public int VfxPrewarm => _vfxPrewarm;

        /// <summary>Alan hasarı ayarları (yapı çarpanı 1: layer'larda yapı yoksa etkisiz).</summary>
        public AreaDamageSpec CreateChainExplosionSpec()
        {
            return new AreaDamageSpec(
                _chainExplosionRadius, _chainExplosionDamage, 1f, _chainExplosionEdgeDamageFraction, _chainExplosionLayers, DamageType.Explosion);
        }
    }
}
