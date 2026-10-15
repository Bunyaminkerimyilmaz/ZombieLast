using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Yatay (XZ) mesafe hesapları; yükseklik sayılmaz. Saf; testlenebilir.</summary>
    public static class PlanarDistance
    {
        /// <summary>İki nokta arası yatay mesafe.</summary>
        public static float Between(Vector3 from, Vector3 to)
        {
            float dx = to.x - from.x;
            float dz = to.z - from.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>
        /// Noktadan kutunun (AABB) yatay izdüşümüne en kısa mesafe; nokta kutunun içindeyse 0.
        /// Izgaradaki yapılar 90°'lik dönüşlerle yerleştiği için AABB onların gerçek ayak izidir.
        /// </summary>
        public static float ToBounds(Vector3 from, Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            float dx = Mathf.Max(min.x - from.x, 0f, from.x - max.x);
            float dz = Mathf.Max(min.z - from.z, 0f, from.z - max.z);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
