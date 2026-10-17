using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Yere vurma (Brute, GDD §7): saldırı anında gövdenin önünde, hedefe doğru (gövde yarıçapı + <see cref="FrontOffset"/>) uyarı
    /// dairesi açılır (<see cref="GroundWarning"/>), hazırlık (1 sn) bitince aynı merkezde <see cref="AreaDamage"/>
    /// (<see cref="EnemyData.Damage"/>, yapılara <see cref="EnemyData.StructureDamageMultiplier"/>) ve <see cref="CameraShakeRequestedEvent"/>.
    /// Merkez önde olduğu için menzildeki (<see cref="EnemyData.AttackRange"/>, 4 m) hareketsiz hedef dairenin içinde kalır; daire
    /// gövdenin merkezinde olsaydı tam menzil sınırındaki hedef ıskalanırdı. Hazırlık sürerken düşman yerinde kalır
    /// (<see cref="IEnemyAttackRuntime.IsBusy"/>); ölürse darbe iptal olur. Aralık <see cref="EnemyData.AttackInterval"/> (6 sn).
    /// </summary>
    [Serializable]
    public sealed class GroundSlamAttack : IEnemyAttack
    {
        [Header("Darbe")]
        [SerializeField, Min(0.1f), Tooltip("Darbe yarıçapı (m), düşmanın ayak noktasından. GDD §7: 4.")]
        private float _radius = 4f;

        [SerializeField, Min(0f), Tooltip("Uyarı dairesinden darbeye kadar hazırlık (sn). GDD §7: 1.")]
        private float _windup = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("Yarıçap sınırındaki hasarın merkez hasarına oranı. 1 = mesafeyle azalmaz.")]
        private float _edgeDamageFraction = 1f;

        [SerializeField, Tooltip("Darbeden hasar alan layer'lar.")]
        private LayerMask _damageLayers = GameLayers.PlayerMask | GameLayers.StructureMask;

        [SerializeField, Tooltip("Darbe merkezinin gövde yüzeyinden hedefe doğru ek uzaklığı (m). Merkez = ayak noktası + yön × (gövde yarıçapı + bu).")]
        private float _frontOffset;

        [SerializeField, Min(0f), Tooltip("Darbe küresinin merkezinin yerden yüksekliği (m). Alçak: yerdeki her şeye yarıçap kadar uzanır.")]
        private float _centerHeight = 0.5f;

        [SerializeField, Min(0f), Tooltip("İstenen kamera sarsıntısının şiddeti (CameraShakeRequestedEvent).")]
        private float _cameraShake = 0.6f;

        [Header("Efekt (opsiyonel)")]
        [SerializeField, Tooltip("Opsiyonel yerdeki uyarı dairesi prefab'ı (kökte GroundWarning). Boşsa uyarı görünmez.")]
        private GroundWarning _warningVfx;

        [SerializeField, Tooltip("Opsiyonel darbe efekti prefab'ı (PooledLifetime ile havuza döner). Boşsa atlanır.")]
        private GameObject _impactVfx;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanan uyarı ve darbe efekti sayısı.")]
        private int _vfxPrewarm = 2;

        [SerializeField, Min(1), Tooltip("Darbe sorgusunda okunan en fazla collider.")]
        private int _bufferSize = 32;

        public float Radius => _radius;

        public float Windup => _windup;

        public float FrontOffset => _frontOffset;

        /// <summary>
        /// Darbe merkezi: <paramref name="feet"/>'ten <paramref name="direction"/> yönünde (yatay; sıfırsa <paramref name="fallback"/>)
        /// <paramref name="bodyRadius"/> + <paramref name="frontOffset"/> uzaklıkta, yerde. Saf.
        /// </summary>
        public static Vector3 GetCenter(Vector3 feet, Vector3 direction, Vector3 fallback, float bodyRadius, float frontOffset)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = new Vector3(fallback.x, 0f, fallback.z);
            }

            return direction.sqrMagnitude < 0.0001f ? feet : feet + direction.normalized * (bodyRadius + frontOffset);
        }

        /// <summary>Null olabilir.</summary>
        public GroundWarning WarningVfx => _warningVfx;

        /// <summary>Null olabilir.</summary>
        public GameObject ImpactVfx => _impactVfx;

        public void Prewarm(IPoolService pool)
        {
            if (_vfxPrewarm <= 0)
            {
                return;
            }

            if (_warningVfx != null)
            {
                pool.Prewarm(_warningVfx.gameObject, _vfxPrewarm);
            }

            if (_impactVfx != null)
            {
                pool.Prewarm(_impactVfx, _vfxPrewarm);
            }
        }

        public IEnemyAttackRuntime CreateRuntime(EnemyAttackContext context)
        {
            return new Runtime(this, context);
        }

        private sealed class Runtime : IEnemyAttackRuntime
        {
            private readonly GroundSlamAttack _settings;
            private readonly EnemyAttackContext _context;
            private readonly Countdown _windup = new Countdown();
            private readonly Collider[] _buffer;
            private GroundWarning _warning;
            private Vector3 _center;

            public Runtime(GroundSlamAttack settings, EnemyAttackContext context)
            {
                _settings = settings;
                _context = context;
                _buffer = new Collider[Mathf.Max(1, settings._bufferSize)];
            }

            public bool IsBusy => _windup.IsRunning;

            public void OnSpawned()
            {
                Cancel();
            }

            public void OnDespawned()
            {
                Cancel();
            }

            public void Tick(float deltaTime)
            {
                if (!_windup.IsRunning)
                {
                    return;
                }

                if (_windup.Tick(deltaTime))
                {
                    Strike();
                    return;
                }

                if (_warning != null)
                {
                    _warning.SetProgress(_windup.Progress);
                }
            }

            public void Perform()
            {
                if (IsBusy)
                {
                    return;
                }

                Vector3 feet = _context.SurfacePosition;
                Vector3 forward = _context.Transform.forward;
                Vector3 direction = _context.TryGetTargetHit(feet, out _, out _, out Vector3 toTarget, out _) ? toTarget : forward;
                _center = GetCenter(feet, direction, forward, _context.BodyRadius, _settings._frontOffset);
                _windup.Start(_settings._windup);
                if (_settings._warningVfx != null && _context.Pool != null)
                {
                    _warning = _context.Pool.Get(_settings._warningVfx, _center, Quaternion.identity);
                    _warning?.Show(_settings._radius);
                }
            }

            public void OnDied()
            {
                Cancel();
            }

            private void Strike()
            {
                ReleaseWarning();
                EnemyData data = _context.Data;
                var spec = new AreaDamageSpec(
                    _settings._radius, data.Damage, data.StructureDamageMultiplier, _settings._edgeDamageFraction, _settings._damageLayers,
                    DamageType.Explosion);
                AreaDamage.Apply(_center + Vector3.up * _settings._centerHeight, spec, _context.GameObject, _buffer);

                if (_settings._impactVfx != null && _context.Pool != null)
                {
                    _context.Pool.Get(_settings._impactVfx, _center, Quaternion.identity);
                }

                if (_settings._cameraShake > 0f)
                {
                    EventBus<CameraShakeRequestedEvent>.Raise(new CameraShakeRequestedEvent(_settings._cameraShake, _center));
                }
            }

            private void Cancel()
            {
                _windup.Stop();
                ReleaseWarning();
            }

            private void ReleaseWarning()
            {
                if (_warning == null)
                {
                    return;
                }

                // Havuzdan geldi (Pool null iken oluşturulmaz).
                _context.Pool.Release(_warning.gameObject);
                _warning = null;
            }
        }
    }
}
