using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>
    /// Seken Mermi (GDD §13): mermi bir düşmana isabet edince, isabet noktasına en yakın ikinci (vurulan dışındaki) canlı düşmana
    /// azaltılmış hasarla seker. <see cref="HitscanShooter"/> kullanır. Sekme tek seferdir (sekme tekrar sekmez).
    /// </summary>
    public static class Ricochet
    {
        /// <summary>Sekmenin hasarı: saçma hasarı × oran (negatifler 0).</summary>
        public static float GetDamage(float pelletDamage, float fraction)
        {
            return pelletDamage > 0f && fraction > 0f ? pelletDamage * fraction : 0f;
        }

        /// <summary>
        /// İsabet noktasına en yakın, vurulan hedef dışındaki canlı hedefi bulur (yatay mesafe, collider kutusunun merkezine).
        /// Görüş hattına bakılmaz. Collider'lar çağıranın tamponuna okunur (bellek ayırmaz).
        /// </summary>
        /// <param name="exclude">İlk vurulan hedef (birden çok collider'ı olsa da atlanır).</param>
        /// <param name="point">Bulunan hedefin collider kutusunun merkezi (iz ve isabet noktası).</param>
        public static bool TryFindTarget(Vector3 origin, float radius, int layerMask, IDamageable exclude, Collider[] buffer,
            out Collider collider, out IDamageable target, out Vector3 point)
        {
            collider = null;
            target = null;
            point = default;
            if (buffer == null || buffer.Length == 0 || radius <= 0f)
            {
                return false;
            }

            int count = Physics.OverlapSphereNonAlloc(origin, radius, buffer, layerMask, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                IDamageable candidate = DamageableLookup.Find(buffer[i]);
                if (candidate == null || !candidate.IsAlive || ReferenceEquals(candidate, exclude))
                {
                    continue;
                }

                Vector3 center = buffer[i].bounds.center;
                float distance = PlanarDistance.Between(origin, center);
                if (distance < best)
                {
                    best = distance;
                    collider = buffer[i];
                    target = candidate;
                    point = center;
                }
            }

            return target != null;
        }
    }
}
