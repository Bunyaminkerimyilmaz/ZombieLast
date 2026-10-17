using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Menzilli saldırı (Tükürücü, GDD §7): menzilde durur (<see cref="EnemyData.AttackRange"/>) ve saldırı anında hedefin yüzeyine
    /// yay çizen, havuzlanmış bir mermi (<see cref="ArcProjectile"/>) atar. Hasar <see cref="EnemyData.Damage"/>, yapılara
    /// <see cref="EnemyData.StructureDamageMultiplier"/>. Mermi çıkış noktası prefab'daki <c>Muzzle</c>.
    /// </summary>
    [Serializable]
    public sealed class RangedAttack : IEnemyAttack
    {
        [SerializeField, Tooltip("Mermi prefab'ı (kökte ArcProjectile, EnemyProjectile layer). Üretici bağlar.")]
        private ArcProjectile _projectile;

        [SerializeField, Min(0.1f), Tooltip("Merminin yatay hızı (m/sn). Uçuş süresi = mesafe / bu.")]
        private float _horizontalSpeed = 12f;

        [SerializeField, Min(0f), Tooltip("Aşağı ivme (m/sn²). Büyüdükçe yay yükselir (yay ortası = g·T²/8).")]
        private float _gravity = 20f;

        [SerializeField, Min(0.05f), Tooltip("En kısa uçuş süresi (sn): yakın hedefte de yay çizsin.")]
        private float _minFlightTime = 0.35f;

        [SerializeField, Min(0.01f), Tooltip("Mermi çarpışma küresinin yarıçapı (m).")]
        private float _hitRadius = 0.25f;

        [SerializeField, Min(0.1f), Tooltip("Hiçbir şeye çarpmazsa bu kadar sn sonra havuza döner.")]
        private float _maxLifetime = 4f;

        [SerializeField, Tooltip("Merminin çarptığı layer'lar. Hasarı sadece hasar alabilenler alır; zemin ve engel mermiyi söndürür.")]
        private LayerMask _hitLayers = GameLayers.PlayerMask | GameLayers.StructureMask | GameLayers.GroundMask | GameLayers.ObstacleMask;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanan mermi sayısı.")]
        private int _prewarm = 16;

        [SerializeField, Tooltip("Opsiyonel çarpma efekti prefab'ı (PooledLifetime ile havuza döner). Boşsa atlanır.")]
        private GameObject _impactVfx;

        /// <summary>Null olabilir (üretici bağlamadıysa: saldırı atlanır).</summary>
        public ArcProjectile Projectile => _projectile;

        public float HorizontalSpeed => _horizontalSpeed;

        public float Gravity => _gravity;

        public int HitLayers => _hitLayers;

        public void Prewarm(IPoolService pool)
        {
            if (_projectile != null && _prewarm > 0)
            {
                pool.Prewarm(_projectile.gameObject, _prewarm);
            }

            if (_impactVfx != null && _prewarm > 0)
            {
                pool.Prewarm(_impactVfx, _prewarm);
            }
        }

        public IEnemyAttackRuntime CreateRuntime(EnemyAttackContext context)
        {
            return new Runtime(this, context);
        }

        private sealed class Runtime : IEnemyAttackRuntime
        {
            private readonly RangedAttack _settings;
            private readonly EnemyAttackContext _context;
            private bool _warned;

            public Runtime(RangedAttack settings, EnemyAttackContext context)
            {
                _settings = settings;
                _context = context;
            }

            public bool IsBusy => false;

            public void OnSpawned()
            {
            }

            public void OnDespawned()
            {
            }

            public void Tick(float deltaTime)
            {
            }

            public void Perform()
            {
                if (_settings._projectile == null || _context.Pool == null)
                {
                    WarnOnce();
                    return;
                }

                Vector3 start = _context.LaunchPoint;
                if (!_context.TryGetTargetHit(start, out _, out Vector3 point, out _, out _))
                {
                    return;
                }

                float flightTime = BallisticArc.FlightTime(start, point, _settings._horizontalSpeed, _settings._minFlightTime);
                EnemyData data = _context.Data;
                var shot = new ProjectileShot(
                    start,
                    BallisticArc.LaunchVelocity(start, point, flightTime, _settings._gravity),
                    _settings._gravity,
                    _settings._hitRadius,
                    _settings._hitLayers,
                    _settings._maxLifetime,
                    data.Damage,
                    data.StructureDamageMultiplier,
                    _context.GameObject,
                    _settings._impactVfx);

                ArcProjectile projectile = _context.Pool.Get(_settings._projectile, start, Quaternion.identity);
                if (projectile != null)
                {
                    projectile.Launch(shot, _context.Pool);
                }
            }

            public void OnDied()
            {
            }

            private void WarnOnce()
            {
                if (_warned)
                {
                    return;
                }

                _warned = true;
                Debug.LogWarning($"[Enemy] '{_context.Data.Id}' menzilli saldırısının mermi prefab'ı veya havuz servisi yok; ateş edemiyor. "
                                 + "Tools/Last Haven/Generate Prefabs çalıştır.", _context.GameObject);
            }
        }
    }
}
