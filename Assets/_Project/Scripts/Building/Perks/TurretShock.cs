using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>
    /// Şok Taret (GDD §13): taretin düşmana isabet eden vuruşları hedefi yavaşlatır (<see cref="IStatusEffectReceiver.ApplySlow"/>;
    /// yavaşlatmalar toplanmaz, en güçlüsü geçerli). <see cref="Turret"/> atıştan sonra, bayrak açıksa çağırır.
    /// </summary>
    public static class TurretShock
    {
        /// <summary>Düşmana isabet eden her sonuca yavaşlatma uygular; yavaşlatılan sayısını döndürür. Oran/süre 0 ise etkisiz.</summary>
        public static int Apply(IReadOnlyList<ShotResult> results, float fraction, float duration)
        {
            if (results == null || fraction <= 0f || duration <= 0f)
            {
                return 0;
            }

            int slowed = 0;
            for (int i = 0; i < results.Count; i++)
            {
                ShotResult result = results[i];
                if (!result.IsHit || result.TargetType != HitTargetType.Enemy)
                {
                    continue;
                }

                IStatusEffectReceiver receiver = StatusEffectLookup.Find(result.Collider);
                if (receiver != null)
                {
                    receiver.ApplySlow(fraction, duration);
                    slowed++;
                }
            }

            return slowed;
        }
    }
}
