using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>Saldırı menzili için yatay (XZ) mesafe hesapları. Hesap Core'daki <see cref="PlanarDistance"/>'ta (onarım da kullanır).</summary>
    public static class TargetDistance
    {
        /// <summary>İki nokta arası yatay mesafe.</summary>
        public static float Planar(Vector3 from, Vector3 to)
        {
            return PlanarDistance.Between(from, to);
        }

        /// <summary>Noktadan kutunun (AABB) yatay izdüşümüne en kısa mesafe; nokta kutunun içindeyse 0.</summary>
        public static float PlanarToBounds(Vector3 from, Bounds bounds)
        {
            return PlanarDistance.ToBounds(from, bounds);
        }
    }
}
