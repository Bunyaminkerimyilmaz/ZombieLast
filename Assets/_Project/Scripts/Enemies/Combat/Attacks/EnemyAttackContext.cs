using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Saldırı stratejisinin (<see cref="IEnemyAttackRuntime"/>) düşmana açılan kapısı: konumlar, hedef, can, havuz. Düşman başına
    /// bir tane; <see cref="Enemy"/> Awake'te kurar.
    /// </summary>
    public sealed class EnemyAttackContext
    {
        private readonly EnemyNavigator _navigator;
        private readonly Collider _collider;
        private readonly Transform _muzzle;

        internal EnemyAttackContext(Enemy enemy, Health health, Collider collider, Transform muzzle, EnemyNavigator navigator, EnemyTargeting targeting)
        {
            Enemy = enemy;
            GameObject = enemy.gameObject;
            Transform = enemy.transform;
            Health = health;
            _collider = collider;
            _muzzle = muzzle;
            _navigator = navigator;
            Targeting = targeting;
        }

        public Enemy Enemy { get; }

        public GameObject GameObject { get; }

        public Transform Transform { get; }

        public EnemyData Data => Enemy.Data;

        public Health Health { get; }

        /// <summary>Havuz servisi; sahnede yoksa null.</summary>
        public IPoolService Pool { get; internal set; }

        /// <summary>Ayakların zemindeki konumu (yerden çıkarken transform gömülüdür).</summary>
        public Vector3 SurfacePosition => _navigator.SurfacePosition;

        /// <summary>
        /// Gövdenin merkezi, zemin yüzeyinde ölçülür: <see cref="SurfacePosition"/> + kök collider merkezinin yüksekliği (yerden çıkarken
        /// gömülü transform yerine yüzey). Collider kapalıysa yüzey konumu.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                Vector3 surface = SurfacePosition;
                if (_collider == null || !_collider.enabled)
                {
                    return surface;
                }

                surface.y += _collider.bounds.center.y - Transform.position.y;
                return surface;
            }
        }

        /// <summary>Gövde yarıçapı (m): kök collider kutusunun yatay yarı genişliği; collider yoksa veya kapalıysa 0.</summary>
        public float BodyRadius
        {
            get
            {
                if (_collider == null || !_collider.enabled)
                {
                    return 0f;
                }

                Vector3 extents = _collider.bounds.extents;
                return Mathf.Max(extents.x, extents.z);
            }
        }

        /// <summary>Mermi çıkış noktası: <c>Muzzle</c> (Visual dışında), yoksa gövde merkezi.</summary>
        public Vector3 LaunchPoint => _muzzle != null ? _muzzle.position : Center;

        /// <summary>Yerden çıkmış ve canlı: kovalıyor, saldırıyor ya da dolaşıyor.</summary>
        public bool IsActive
        {
            get
            {
                EnemyStateId state = Enemy.State;
                return Enemy.IsAlive && (state == EnemyStateId.Chasing || state == EnemyStateId.Attacking || state == EnemyStateId.Wandering);
            }
        }

        internal EnemyTargeting Targeting { get; }

        /// <summary>
        /// Vurulabilir canlı hedef varsa hasar alanını, yüzeyindeki vuruş noktasını (<paramref name="from"/>'a en yakın, gövde ortası
        /// yüksekliğinde), yatay yönü ve yapı olup olmadığını verir.
        /// </summary>
        public bool TryGetTargetHit(Vector3 from, out IDamageable damageable, out Vector3 point, out Vector3 direction, out bool isStructure)
        {
            isStructure = false;
            if (!Targeting.Target.TryGetHit(from, Transform.forward, out damageable, out point, out direction))
            {
                return false;
            }

            isStructure = Targeting.Target.Target.TargetType.IsStructure();
            return true;
        }
    }
}
