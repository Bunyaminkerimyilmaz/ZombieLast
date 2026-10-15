using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>Üs alanının saf geometri hesapları.</summary>
    public static class BaseAreaMath
    {
        /// <summary>
        /// Nokta, merkezi ve yarıçapı verilen dairenin içinde mi. Mesafe yatay düzlemde (XZ) ölçülür, yükseklik yok sayılır;
        /// sınır dahildir. Negatif yarıçapta hiçbir nokta içeride değildir.
        /// </summary>
        public static bool Contains(Vector3 center, float radius, Vector3 position)
        {
            if (radius < 0f)
            {
                return false;
            }

            float dx = position.x - center.x;
            float dz = position.z - center.z;
            return dx * dx + dz * dz <= radius * radius;
        }
    }
}
