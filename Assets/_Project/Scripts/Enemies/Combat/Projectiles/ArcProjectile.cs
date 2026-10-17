using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Yay çizen düşman mermisi (Tükürücü asidi, GDD §7): <see cref="BallisticArc"/> yolunda uçar, her karede önceki konumdan yenisine
    /// küre taramasıyla (<see cref="ProjectileShot.HitLayers"/>, trigger'lar hariç) çarpışma arar. Çarptığı canlı <see cref="IDamageable"/>'a
    /// hasar verir (yapıya çarpanla), zemine/engele çarpınca sadece söner; sonra havuza döner. Kök EnemyProjectile layer'ında,
    /// collider'ı yok. Görsel <c>Visual</c> altında.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ArcProjectile : MonoBehaviour, IPoolable
    {
        private ProjectileShot _shot;
        private IPoolService _pool;
        private float _elapsed;
        private bool _flying;

        /// <summary>Uçuyor (atıldı, henüz çarpmadı ve ömrü dolmadı).</summary>
        public bool IsFlying => _flying;

        /// <summary>Mermiyi <see cref="ProjectileShot.Start"/>'tan atar. Bitince <paramref name="pool"/>'a döner (null ise yok edilir).</summary>
        public void Launch(in ProjectileShot shot, IPoolService pool)
        {
            _shot = shot;
            _pool = pool;
            _elapsed = 0f;
            _flying = true;
            transform.position = shot.Start;
        }

        public void OnSpawned()
        {
            _flying = false;
            _elapsed = 0f;
        }

        public void OnDespawned()
        {
            _flying = false;
        }

        /// <summary>Uçuşu ilerletir. Update'ten çağrılır; testler doğrudan çağırır.</summary>
        internal void Advance(float deltaTime)
        {
            if (!_flying)
            {
                return;
            }

            Vector3 from = transform.position;
            _elapsed += deltaTime;
            Vector3 to = BallisticArc.PositionAt(_shot.Start, _shot.Velocity, _shot.Gravity, _elapsed);
            Vector3 step = to - from;
            float length = step.magnitude;
            if (length > 0f)
            {
                Vector3 direction = step / length;
                if (Physics.SphereCast(from, _shot.Radius, direction, out RaycastHit hit, length, _shot.HitLayers, QueryTriggerInteraction.Ignore))
                {
                    transform.position = from + direction * hit.distance;
                    Impact(hit.collider, hit.point, direction);
                    return;
                }
            }

            transform.position = to;
            if (_elapsed >= _shot.MaxLifetime)
            {
                Finish();
            }
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        private void Impact(Collider collider, Vector3 point, Vector3 direction)
        {
            IDamageable damageable = DamageableLookup.Find(collider);
            if (damageable != null && damageable.IsAlive && _shot.Damage > 0f)
            {
                bool isStructure = HitTargetTypes.FromLayer(collider.gameObject.layer) == HitTargetType.Structure;
                float amount = isStructure ? _shot.Damage * _shot.StructureMultiplier : _shot.Damage;
                Vector3 planar = new Vector3(direction.x, 0f, direction.z);
                planar = planar.sqrMagnitude > 0.0001f ? planar.normalized : Vector3.zero;
                damageable.TakeDamage(new DamageInfo(amount, DamageType.Projectile, _shot.Source, point, planar));
            }

            if (_shot.ImpactVfx != null && _pool != null)
            {
                _pool.Get(_shot.ImpactVfx, point, Quaternion.identity);
            }

            Finish();
        }

        private void Finish()
        {
            _flying = false;
            if (_pool != null)
            {
                _pool.Release(gameObject);
                return;
            }

            Destroy(gameObject);
        }
    }
}
