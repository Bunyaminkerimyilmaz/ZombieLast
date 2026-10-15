using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Ayak izinde engel var mı (GDD §11 "çakışma"): ayak izi × yükseklik kutusunda verilen layer'lardan bir collider
    /// (oyuncu, düşman, engel). Kutu ayak izinden biraz içeride: komşu hücreye değen collider engel sayılmaz. Trigger'lar yok sayılır.
    /// </summary>
    public static class FootprintBlocking
    {
        // Kutunun ayak izinden içeride kalan payı (m).
        private const float Inset = 0.05f;
        private const float MinHalfExtent = 0.01f;

        /// <param name="center">Ayak izinin merkezi, yer seviyesinde.</param>
        public static bool IsBlocked(RectInt cells, float cellSize, Vector3 center, float height, LayerMask layers)
        {
            float halfHeight = height * 0.5f;
            var halfExtents = new Vector3(
                Mathf.Max(MinHalfExtent, cells.width * cellSize * 0.5f - Inset),
                halfHeight,
                Mathf.Max(MinHalfExtent, cells.height * cellSize * 0.5f - Inset));

            return Physics.CheckBox(center + Vector3.up * halfHeight, halfExtents, Quaternion.identity, layers, QueryTriggerInteraction.Ignore);
        }
    }
}
