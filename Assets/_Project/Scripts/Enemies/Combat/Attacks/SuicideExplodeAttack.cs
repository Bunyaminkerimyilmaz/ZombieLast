using System;
using LastHaven.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Kendini patlatma (Patlayan, GDD §7): yapıya veya oyuncuya değince (gövde merkezinden <see cref="EnemyData.AttackRange"/> içinde),
    /// hedefi menzile girince ya da öldürülünce bir kez patlar. Patlama <see cref="AreaDamage"/>: <see cref="EnemyData.Damage"/>,
    /// yapılara <see cref="EnemyData.StructureDamageMultiplier"/>, yarıçap burada. Yakındaki Patlayanlar <see cref="ExplosionChain"/>
    /// ile gecikmeli tetiklenir. Patlayan patlayınca ölür (ölüm yolu normal: ödül, sayaç).
    /// </summary>
    [Serializable]
    public sealed class SuicideExplodeAttack : IEnemyAttack
    {
        [Header("Patlama")]
        [SerializeField, Min(0.1f), Tooltip("Patlama yarıçapı (m), gövde merkezinden hedef yüzeyine. GDD §7: 3.")]
        private float _radius = 3f;

        [SerializeField, Range(0f, 1f), Tooltip("Yarıçap sınırındaki hasarın merkez hasarına oranı. 1 = mesafeyle azalmaz (GDD §7).")]
        private float _edgeDamageFraction = 1f;

        [SerializeField, Tooltip("Patlamadan hasar alan layer'lar. Düşmanlar varsayılan olarak dahil değil (zincir ayrı çalışır).")]
        private LayerMask _damageLayers = GameLayers.PlayerMask | GameLayers.StructureMask;

        [Header("Temas")]
        [SerializeField, Tooltip("Değince patlatan layer'lar (trigger'lar hariç: üstünden geçilen tuzak tetiklemez).")]
        private LayerMask _contactLayers = GameLayers.PlayerMask | GameLayers.StructureMask;

        [SerializeField, Min(0.02f), Tooltip("Temas kontrolü aralığı (sn). Başlangıç her düşmanda rastgele kaydırılır.")]
        private float _contactCheckInterval = 0.1f;

        [Header("Zincir")]
        [SerializeField, Min(0f), Tooltip("Patlayınca bu yatay yarıçaptaki Patlayanları tetikler (m).")]
        private float _chainRadius = 3f;

        [SerializeField, Min(0f), Tooltip("Zincirle tetiklenenin patlama gecikmesi (sn).")]
        private float _chainDelay = 0.15f;

        [Header("Efekt (opsiyonel)")]
        [SerializeField, Tooltip("Opsiyonel patlama efekti prefab'ı (PooledLifetime ile havuza döner). Boşsa atlanır.")]
        private GameObject _explosionVfx;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanan efekt sayısı.")]
        private int _vfxPrewarm = 8;

        [SerializeField, Min(1), Tooltip("Patlama ve temas sorgusunda okunan en fazla collider.")]
        private int _bufferSize = 32;

        public float Radius => _radius;

        public float EdgeDamageFraction => _edgeDamageFraction;

        public float ChainRadius => _chainRadius;

        public float ChainDelay => _chainDelay;

        /// <summary>Null olabilir.</summary>
        public GameObject ExplosionVfx => _explosionVfx;

        public void Prewarm(IPoolService pool)
        {
            if (_explosionVfx != null && _vfxPrewarm > 0)
            {
                pool.Prewarm(_explosionVfx, _vfxPrewarm);
            }
        }

        public IEnemyAttackRuntime CreateRuntime(EnemyAttackContext context)
        {
            return new Runtime(this, context);
        }

        private sealed class Runtime : IEnemyAttackRuntime, IChainExplosive
        {
            private readonly SuicideExplodeAttack _settings;
            private readonly EnemyAttackContext _context;
            private readonly ExplosiveFuse _fuse = new ExplosiveFuse();
            private readonly Collider[] _buffer;
            private float _contactTimer;

            public Runtime(SuicideExplodeAttack settings, EnemyAttackContext context)
            {
                _settings = settings;
                _context = context;
                _buffer = new Collider[Mathf.Max(1, settings._bufferSize)];
            }

            public bool IsBusy => false;

            public Vector3 Position => _context.SurfacePosition;

            public bool TryPrime(float delay) => _fuse.TryPrime(delay);

            public void OnSpawned()
            {
                _fuse.Arm();
                _contactTimer = Random.Range(0f, _settings._contactCheckInterval);
                ExplosionChain.Shared.Add(this);
            }

            public void OnDespawned()
            {
                _fuse.Disarm();
                ExplosionChain.Shared.Remove(this);
            }

            public void Tick(float deltaTime)
            {
                if (_fuse.Tick(deltaTime))
                {
                    Detonate();
                    return;
                }

                if (!_context.IsActive)
                {
                    return;
                }

                _contactTimer -= deltaTime;
                if (_contactTimer > 0f)
                {
                    return;
                }

                _contactTimer = Mathf.Max(0f, _contactTimer + _settings._contactCheckInterval);
                if (IsTouching())
                {
                    Detonate();
                }
            }

            public void Perform()
            {
                Detonate();
            }

            public void OnDied()
            {
                Detonate();
            }

            private bool IsTouching()
            {
                int count = Physics.OverlapSphereNonAlloc(
                    _context.Center, _context.Data.AttackRange, _buffer, _settings._contactLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    IDamageable damageable = DamageableLookup.Find(_buffer[i]);
                    if (damageable != null && damageable.IsAlive)
                    {
                        return true;
                    }
                }

                return false;
            }

            // Ölümden de çağrılır: o zaman can zaten bitmiştir, sadece patlar.
            private void Detonate()
            {
                if (!_fuse.TryDetonate())
                {
                    return;
                }

                ExplosionChain.Shared.Remove(this);
                Vector3 center = _context.Center;
                Vector3 ground = _context.SurfacePosition;
                EnemyData data = _context.Data;
                var spec = new AreaDamageSpec(
                    _settings._radius, data.Damage, data.StructureDamageMultiplier, _settings._edgeDamageFraction, _settings._damageLayers,
                    DamageType.Explosion);
                AreaDamage.Apply(center, spec, _context.GameObject, _buffer);

                if (_settings._explosionVfx != null && _context.Pool != null)
                {
                    _context.Pool.Get(_settings._explosionVfx, center, Quaternion.identity);
                }

                ExplosionChain.Shared.PrimeAround(ground, _settings._chainRadius, _settings._chainDelay);

                Health health = _context.Health;
                if (health.IsAlive)
                {
                    health.TakeDamage(new DamageInfo(health.Current, DamageType.Explosion, _context.GameObject, center, Vector3.zero));
                }
            }
        }
    }
}
