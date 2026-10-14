using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>Hareket ve nişan için yatay düzlem hesapları (saf, test edilebilir).</summary>
    public static class PlanarMath
    {
        /// <summary>Bunun altındaki kare uzunluktaki yönler "yön yok" sayılır.</summary>
        public const float MinDirectionSqr = 0.0001f;

        /// <summary>
        /// Ekran düzlemindeki girdiyi (x sağ, y yukarı) kamera yaw'ına göre dünya XZ yönüne çevirir. Uzunluk korunur.
        /// Yaw 0'da yukarı = +Z (kuzey); yaw 90'da yukarı = +X (doğu).
        /// </summary>
        public static Vector3 CameraRelative(Vector2 input, float cameraYawDegrees)
        {
            float radians = cameraYawDegrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector3(
                input.x * cos + input.y * sin,
                0f,
                -input.x * sin + input.y * cos);
        }

        /// <summary>
        /// Işının <paramref name="height"/> yüksekliğindeki yatay düzlemle kesiştiği nokta.
        /// Işın düzleme paralelse veya düzlemden uzaklaşıyorsa false döner.
        /// </summary>
        public static bool TryIntersectHorizontalPlane(Vector3 origin, Vector3 direction, float height, out Vector3 point)
        {
            point = default;
            if (Mathf.Abs(direction.y) < Mathf.Epsilon)
            {
                return false;
            }

            float distance = (height - origin.y) / direction.y;
            if (distance < 0f)
            {
                return false;
            }

            point = origin + direction * distance;
            point.y = height;
            return true;
        }

        /// <summary>
        /// <paramref name="from"/>'dan <paramref name="to"/>'ya yatay, normalize yön.
        /// Noktalar yatayda <paramref name="minDistance"/>'tan yakınsa <paramref name="fallback"/> döner.
        /// </summary>
        public static Vector3 FlatDirection(Vector3 from, Vector3 to, Vector3 fallback, float minDistance)
        {
            Vector3 offset = to - from;
            offset.y = 0f;
            float sqr = offset.sqrMagnitude;
            if (sqr < minDistance * minDistance || sqr < MinDirectionSqr)
            {
                return fallback;
            }

            return offset / Mathf.Sqrt(sqr);
        }
    }
}
