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
    /// Perk kancaları (<see cref="SetPerks"/>): Avcı açıksa yavaşlamış hedefe (<see cref="IStatusEffectReceiver.IsSlowed"/>) ek hasar;
    /// Seken Mermi açıksa düşmana isabet eden her saçma en yakın ikinci düşmana seker (<see cref="Ricochet"/>; sekmenin de
    /// <see cref="ShotHitEvent"/> ve <see cref="ShotTracedEvent"/>'i gönderilir, sonuç listesine girmez).
    /// </summary>
    public sealed class HitscanShooter
    {
        private readonly List<ShotResult> _results = new List<ShotResult>(8);
        private IRunModifiers _modifiers;
        private CombatPerkConfig _perks;
        private Collider[] _ricochetBuffer;

        /// <summary>
        /// Perk kancalarını bağlar (GDD §13 Avcı, Seken Mermi). İkisi de gerekir; biri null ise kancalar kapalı. Oyuncu silahları bağlar.
        /// </summary>
        public void SetPerks(IRunModifiers modifiers, CombatPerkConfig perks)
        {
            _modifiers = modifiers;
            _perks = perks;
            _ricochetBuffer = perks != null ? new Collider[perks.RicochetBufferSize] : null;
        }

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
                if (result.Damage > 0f && result.TargetType == HitTargetType.Enemy && HasPerk(PerkFlag.Ricochet))
                {
                    FireRicochet(request, result);
                }
            }

            return _results;
        }

        // Seken Mermi: isabet noktasından en yakın ikinci düşmana, saçma hasarının oranıyla (Avcı yeni hedefe ayrıca bakar).
        private void FireRicochet(in HitscanRequest request, in ShotResult hit)
        {
            if (!Ricochet.TryFindTarget(hit.EndPoint, _perks.RicochetRadius, _perks.RicochetTargetLayers, hit.Damageable, _ricochetBuffer,
                    out Collider collider, out IDamageable target, out Vector3 point))
            {
                return;
            }

            Vector3 offset = point - hit.EndPoint;
            Vector3 direction = offset.sqrMagnitude > 0.0001f ? offset.normalized : hit.Direction;
            float damage = Ricochet.GetDamage(request.Damage, _perks.RicochetDamageFraction) * GetPerkMultiplier(collider);
            target.TakeDamage(new DamageInfo(damage, DamageType.Bullet, request.Source, point, direction));

            GameObject targetObject = target is Component component ? component.gameObject : collider.gameObject;
            EventBus<ShotHitEvent>.Raise(new ShotHitEvent(
                request.Source, targetObject, HitTargetType.Enemy, point, -direction, direction, damage));
            EventBus<ShotTracedEvent>.Raise(new ShotTracedEvent(request.Source, request.WeaponId, hit.EndPoint, point, direction, true));
        }

        private bool HasPerk(PerkFlag flag)
        {
            return _perks != null && _modifiers != null && _modifiers.HasFlag(flag);
        }

        private ShotResult CastPellet(in HitscanRequest request, Vector3 direction)
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
                damage = request.Damage * GetPerkMultiplier(collider);
                damageable.TakeDamage(new DamageInfo(damage, DamageType.Bullet, request.Source, hit.point, direction));
            }

            HitTargetType targetType = HitTargetTypes.FromLayer(collider.gameObject.layer);
            GameObject target = damageable is Component component ? component.gameObject : collider.gameObject;
            EventBus<ShotHitEvent>.Raise(new ShotHitEvent(request.Source, target, targetType, hit.point, hit.normal, direction, damage));
            return ShotResult.Hit(request.Origin, direction, hit, damageable, targetType, damage);
        }

        // Avcı: bayrak açıksa vurulan hedefin yavaşlaması sorulur (alıcı aranması sadece o zaman).
        private float GetPerkMultiplier(Collider collider)
        {
            if (!HasPerk(PerkFlag.Hunter))
            {
                return 1f;
            }

            IStatusEffectReceiver receiver = StatusEffectLookup.Find(collider);
            return HunterBonus.GetMultiplier(true, receiver != null && receiver.IsSlowed, _perks.HunterDamageBonus);
        }
    }
}
