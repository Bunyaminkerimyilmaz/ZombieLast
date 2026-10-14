using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>Tek bir saçmanın sonucu.</summary>
    public readonly struct ShotResult
    {
        private ShotResult(
            Vector3 origin,
            Vector3 direction,
            Vector3 endPoint,
            bool isHit,
            Vector3 normal,
            Collider collider,
            IDamageable damageable,
            HitTargetType targetType,
            float damage)
        {
            Origin = origin;
            Direction = direction;
            EndPoint = endPoint;
            IsHit = isHit;
            Normal = normal;
            Collider = collider;
            Damageable = damageable;
            TargetType = targetType;
            Damage = damage;
        }

        public Vector3 Origin { get; }

        /// <summary>Saçılma uygulanmış yön.</summary>
        public Vector3 Direction { get; }

        /// <summary>İsabet noktası; ıskaladıysa menzilin sonu.</summary>
        public Vector3 EndPoint { get; }

        public bool IsHit { get; }

        /// <summary>İsabet yüzeyinin normali; ıskaladıysa sıfır.</summary>
        public Vector3 Normal { get; }

        /// <summary>Vurulan collider; ıskaladıysa null.</summary>
        public Collider Collider { get; }

        /// <summary>Vurulan hasar alabilen nesne; yoksa null.</summary>
        public IDamageable Damageable { get; }

        public HitTargetType TargetType { get; }

        /// <summary>Uygulanan hasar isteği; ıskalamada, yüzeyde veya ölü hedefte 0.</summary>
        public float Damage { get; }

        public static ShotResult Miss(Vector3 origin, Vector3 direction, float range)
        {
            return new ShotResult(origin, direction, origin + direction * range, false, Vector3.zero, null, null, HitTargetType.Surface, 0f);
        }

        public static ShotResult Hit(
            Vector3 origin,
            Vector3 direction,
            RaycastHit hit,
            IDamageable damageable,
            HitTargetType targetType,
            float damage)
        {
            return new ShotResult(origin, direction, hit.point, true, hit.normal, hit.collider, damageable, targetType, damage);
        }
    }
}
