using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Ekran dışındaki bir hedefin kenar göstergesi (saf): hedefin viewport noktası (kameranın <c>WorldToViewportPoint</c>'i;
    /// x/y 0..1, y yukarı, z derinlik) ekran ortasından o yöne uzatılıp kenardan <c>inset</c> içerideki dikdörtgene kırpılır.
    /// Kameranın arkasındaki nokta viewport'ta ters döndüğü için yönü çevrilir. Sonuç panel uzayında (y aşağı).
    /// </summary>
    public static class EdgeIndicatorMath
    {
        private const float ViewportCenter = 0.5f;
        private const float ViewportMin = 0f;
        private const float ViewportMax = 1f;

        /// <summary>Nokta kameranın önünde ve ekranın içinde mi (kenar dahil).</summary>
        public static bool IsOnScreen(Vector3 viewport)
        {
            return viewport.z > 0f
                   && viewport.x >= ViewportMin && viewport.x <= ViewportMax
                   && viewport.y >= ViewportMin && viewport.y <= ViewportMax;
        }

        /// <param name="viewport">Hedefin viewport noktası.</param>
        /// <param name="size">Ekranın (panel) boyutu.</param>
        /// <param name="inset">Göstergenin merkezinin kenarlara en az uzaklığı (yatay, dikey).</param>
        /// <param name="angleDegrees">Göstergenin yönü: sağ = 0°, saat yönünde artar (panelde y aşağı; UI Toolkit <c>rotate</c>).</param>
        /// <returns>Göstergenin merkezi (panel uzayı).</returns>
        public static Vector2 ToEdge(Vector3 viewport, Vector2 size, Vector2 inset, out float angleDegrees)
        {
            float x = viewport.x - ViewportCenter;
            float y = viewport.y - ViewportCenter;
            if (viewport.z < 0f)
            {
                x = -x;
                y = -y;
            }

            // Panelde y aşağı doğru artar; tam ortadaki (arkadaki) nokta için alt kenar (kameraya en yakın yön): +y.
            var direction = new Vector2(x * size.x, -y * size.y);
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = new Vector2(0f, 1f);
            }

            Vector2 half = size * ViewportCenter;
            float extentX = Mathf.Max(0f, half.x - inset.x);
            float extentY = Mathf.Max(0f, half.y - inset.y);
            float scaleX = Mathf.Abs(direction.x) > Mathf.Epsilon ? extentX / Mathf.Abs(direction.x) : float.PositiveInfinity;
            float scaleY = Mathf.Abs(direction.y) > Mathf.Epsilon ? extentY / Mathf.Abs(direction.y) : float.PositiveInfinity;
            angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return half + direction * Mathf.Min(scaleX, scaleY);
        }
    }
}
