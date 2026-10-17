using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace LastHaven.Core
{
    /// <summary>
    /// Alan hasarı (patlama, darbe): merkezin yarıçapı içindeki canlı her <see cref="IDamageable"/>'a bir kez hasar verir.
    /// Collider'lar çağıranın tamponuna <c>OverlapSphereNonAlloc</c> ile okunur; ara liste havuzdan gelir (bellek ayırmaz).
    /// Hesap kısmı (<see cref="Falloff"/>, <see cref="Calculate"/>) saftır.
    /// </summary>
    public static class AreaDamage
    {
        private readonly struct AreaHit
        {
            public AreaHit(IDamageable damageable, float distance, Vector3 point, Vector3 direction, bool isStructure)
            {
                Damageable = damageable;
                Distance = distance;
                Point = point;
                Direction = direction;
                IsStructure = isStructure;
            }

            public IDamageable Damageable { get; }
            public float Distance { get; }
            public Vector3 Point { get; }
            public Vector3 Direction { get; }
            public bool IsStructure { get; }
        }

        /// <summary>Mesafeye göre hasar oranı: merkezde 1, yarıçapta <paramref name="edgeFraction"/> (doğrusal; dışı da sınır değeri).</summary>
        public static float Falloff(float distance, float radius, float edgeFraction)
        {
            if (radius <= 0f)
            {
                return 1f;
            }

            return Mathf.Lerp(1f, Mathf.Clamp01(edgeFraction), Mathf.Clamp01(distance / radius));
        }

        /// <summary>Merkezden <paramref name="distance"/> m'deki hedefin aldığı hasar; yapıysa <see cref="AreaDamageSpec.StructureMultiplier"/> uygulanır.</summary>
        public static float Calculate(in AreaDamageSpec spec, float distance, bool isStructure)
        {
            float amount = spec.Damage * Falloff(distance, spec.Radius, spec.EdgeDamageFraction);
            return isStructure ? amount * spec.StructureMultiplier : amount;
        }

        /// <summary>
        /// <paramref name="center"/>'ın <see cref="AreaDamageSpec.Radius"/> içindeki canlılara hasar verir. Birden çok collider'ı olan hedef
        /// bir kez, en yakın collider'ının yüzey uzaklığıyla vurulur. <paramref name="source"/>'un kendisi atlanır. Tampon dolarsa
        /// fazlası o patlamada etkilenmez. Hasar alan sayısını döndürür. Hasar sırasında başka bir alan hasarı tetiklenebilir
        /// (zincir); toplama hasardan önce bittiği için aynı tampon güvenle yeniden kullanılır.
        /// </summary>
        /// <param name="triggers">Tuzak gibi sadece trigger collider'ı olan yapılar da etkilensin diye varsayılan Collide.</param>
        public static int Apply(Vector3 center, in AreaDamageSpec spec, GameObject source, Collider[] buffer,
            QueryTriggerInteraction triggers = QueryTriggerInteraction.Collide)
        {
            if (buffer == null || buffer.Length == 0 || spec.Radius <= 0f || spec.Damage <= 0f)
            {
                return 0;
            }

            int count = Physics.OverlapSphereNonAlloc(center, spec.Radius, buffer, spec.LayerMask, triggers);
            List<AreaHit> hits = ListPool<AreaHit>.Get();
            try
            {
                Collect(center, buffer, count, source, hits);
                return Strike(spec, source, hits);
            }
            finally
            {
                ListPool<AreaHit>.Release(hits);
            }
        }

        private static void Collect(Vector3 center, Collider[] buffer, int count, GameObject source, List<AreaHit> hits)
        {
            for (int i = 0; i < count; i++)
            {
                Collider collider = buffer[i];
                IDamageable damageable = DamageableLookup.Find(collider);
                if (damageable == null || !damageable.IsAlive || IsSource(damageable, source))
                {
                    continue;
                }

                Bounds bounds = collider.bounds;
                Vector3 point = bounds.ClosestPoint(center);
                float distance = Vector3.Distance(center, point);
                int existing = IndexOf(hits, damageable);
                if (existing >= 0)
                {
                    if (distance < hits[existing].Distance)
                    {
                        hits[existing] = new AreaHit(damageable, distance, point, hits[existing].Direction, hits[existing].IsStructure);
                    }

                    continue;
                }

                bool isStructure = HitTargetTypes.FromLayer(collider.gameObject.layer) == HitTargetType.Structure;
                hits.Add(new AreaHit(damageable, distance, point, PlanarDirection(center, bounds.center), isStructure));
            }
        }

        private static int Strike(in AreaDamageSpec spec, GameObject source, List<AreaHit> hits)
        {
            int damaged = 0;
            for (int i = 0; i < hits.Count; i++)
            {
                AreaHit hit = hits[i];
                float amount = Calculate(spec, hit.Distance, hit.IsStructure);
                if (amount <= 0f || !hit.Damageable.IsAlive)
                {
                    continue;
                }

                hit.Damageable.TakeDamage(new DamageInfo(amount, spec.DamageType, source, hit.Point, hit.Direction));
                damaged++;
            }

            return damaged;
        }

        private static int IndexOf(List<AreaHit> hits, IDamageable damageable)
        {
            for (int i = 0; i < hits.Count; i++)
            {
                if (ReferenceEquals(hits[i].Damageable, damageable))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsSource(IDamageable damageable, GameObject source)
        {
            return source != null && damageable is Component component && component.gameObject == source;
        }

        /// <summary>Merkezden hedefe yatay yön (geri itme için); üst üsteyse sıfır.</summary>
        private static Vector3 PlanarDirection(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
        }
    }
}
