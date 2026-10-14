using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>
    /// Yeniden kullanılabilir hitscan atıcı (oyuncu silahları, taretler). Her saçma için tek ışın atar (delme yok), isabet edilen
    /// <see cref="IDamageable"/>'a <see cref="DamageType.Bullet"/> hasarı uygular ve isabet başına <see cref="ShotHitEvent"/> gönderir.
    /// Her saçma için (ıska dahil) mermi izi olayı <see cref="ShotTracedEvent"/> gönderir.
    /// Hasar mesafeyle düşmez (GDD §6). Her kullanıcı kendi örneğini tutar; sonuç listesi tekrar kullanılır, bellek ayırmaz.
    /// </summary>
    public sealed class HitscanShooter
    {
        private readonly List<ShotResult> _results = new List<ShotResult>(8);

        /// <summary>Ateşler ve saçma başına sonuçları döndürür. Liste bir sonraki <see cref="Fire"/>'a kadar geçerlidir; saklama.</summary>
        public IReadOnlyList<ShotResult> Fire(in HitscanRequest request)
        {
            _results.Clear();
            for (int i = 0; i < request.PelletCount; i++)
            {
                float angle = SpreadPattern.GetPelletAngle(i, request.PelletCount, request.SpreadAngle, Random.value);
                Vector3 direction = SpreadPattern.RotateAroundUp(request.Direction, angle);
                ShotResult result = CastPellet(request, direction);
                EventBus<ShotTracedEvent>.Raise(new ShotTracedEvent(
                    request.Source, request.WeaponId, request.TracerOrigin, result.EndPoint, result.Direction, result.IsHit));
                _results.Add(result);
            }

            return _results;
        }

        private static ShotResult CastPellet(in HitscanRequest request, Vector3 direction)
        {
            if (!Physics.Raycast(request.Origin, direction, out RaycastHit hit, request.Range, request.HitMask, QueryTriggerInteraction.Ignore))
            {
                return ShotResult.Miss(request.Origin, direction, request.Range);
            }

            Collider collider = hit.collider;
            IDamageable damageable = DamageableLookup.Find(collider);
            float damage = 0f;
            if (damageable != null && damageable.IsAlive)
            {
                damage = request.Damage;
                damageable.TakeDamage(new DamageInfo(damage, DamageType.Bullet, request.Source, hit.point, direction));
            }

            HitTargetType targetType = HitTargetTypes.FromLayer(collider.gameObject.layer);
            GameObject target = damageable is Component component ? component.gameObject : collider.gameObject;
            EventBus<ShotHitEvent>.Raise(new ShotHitEvent(request.Source, target, targetType, hit.point, hit.normal, direction, damage));
            return ShotResult.Hit(request.Origin, direction, hit, damageable, targetType, damage);
        }
    }
}
