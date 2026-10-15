using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın seçili hedefi ve önbelleklenmiş bileşenleri (<see cref="IDamageable"/>, ayak izi collider'ı).
    /// Saldırı menzili hedefin merkezine değil yüzeyine (collider AABB'si) ölçülür; collider yoksa merkezine.
    /// </summary>
    internal sealed class TrackedTarget
    {
        private ITargetable _target;
        private IDamageable _damageable;
        private Collider _collider;

        /// <summary>Hedef var, yok edilmemiş ve hedeflenebilir.</summary>
        public bool IsValid => _target != null && !(_target is Object unityObject && unityObject == null) && _target.IsTargetable;

        /// <summary>Geçerliyse hedef, değilse null.</summary>
        public ITargetable Target => IsValid ? _target : null;

        /// <summary>Hedefin merkezi. Sadece <see cref="IsValid"/> iken çağır.</summary>
        public Vector3 Position => _target.Transform.position;

        private bool HasCollider => _collider != null && _collider.enabled;

        /// <summary>Hedefi değiştirir; değiştiyse true döner.</summary>
        public bool Set(ITargetable target)
        {
            if (ReferenceEquals(target, _target))
            {
                return false;
            }

            _target = target;
            TargetComponentCache.Get(target, out _damageable, out _collider);
            return true;
        }

        /// <summary>Hedefin yüzeyine yatay uzaklık (m); hedef geçersizse +∞.</summary>
        public float DistanceFrom(Vector3 position)
        {
            if (!IsValid)
            {
                return float.PositiveInfinity;
            }

            return HasCollider
                ? TargetDistance.PlanarToBounds(position, _collider.bounds)
                : TargetDistance.Planar(position, _target.Transform.position);
        }

        /// <summary>
        /// Vurulabilir canlı hedef varsa hasar alanını, yüzeydeki vuruş noktasını (gövde ortası yüksekliğinde) ve
        /// yatay vuruş yönünü verir. Yön hesaplanamazsa <paramref name="fallbackDirection"/> kullanılır.
        /// </summary>
        public bool TryGetHit(Vector3 from, Vector3 fallbackDirection, out IDamageable damageable, out Vector3 hitPoint, out Vector3 direction)
        {
            damageable = _damageable;
            hitPoint = default;
            direction = fallbackDirection;
            if (!IsValid || _damageable == null || !_damageable.IsAlive)
            {
                return false;
            }

            hitPoint = _target.Transform.position;
            if (HasCollider)
            {
                Bounds bounds = _collider.bounds;
                hitPoint = bounds.ClosestPoint(new Vector3(from.x, bounds.center.y, from.z));
            }

            Vector3 planar = hitPoint - from;
            planar.y = 0f;
            if (planar.sqrMagnitude > 0f)
            {
                direction = planar.normalized;
            }

            return true;
        }
    }
}
