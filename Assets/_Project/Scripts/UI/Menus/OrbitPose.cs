using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>Ana menü kamerasının yörünge konumu ve bakışı (saf).</summary>
    public static class OrbitPose
    {
        /// <summary>
        /// Merkezin etrafında <paramref name="angleDegrees"/> açısındaki kamera konumu (0° = merkezin güneyi, -Z; açı arttıkça saat
        /// yönünün tersine) ve merkezin <paramref name="lookHeight"/> üstüne bakan dönüş; bakış <paramref name="yawOffsetDegrees"/> kadar
        /// yatay döndürülür (negatif = sola: merkez ekranın sağında görünür).
        /// </summary>
        public static void Evaluate(Vector3 center, float radius, float height, float lookHeight, float angleDegrees, float yawOffsetDegrees,
            out Vector3 position, out Quaternion rotation)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            position = center + new Vector3(Mathf.Sin(radians) * radius, height, -Mathf.Cos(radians) * radius);
            Vector3 look = center + Vector3.up * lookHeight - position;
            rotation = look.sqrMagnitude > 0f
                ? Quaternion.AngleAxis(yawOffsetDegrees, Vector3.up) * Quaternion.LookRotation(look, Vector3.up)
                : Quaternion.identity;
        }

        /// <summary>Açıyı ilerletir ve 0..360'ta tutar (uzun açık kalan menüde kayan nokta büyümesin).</summary>
        public static float Advance(float angleDegrees, float speedDegreesPerSecond, float deltaTime)
        {
            return Mathf.Repeat(angleDegrees + speedDegreesPerSecond * deltaTime, 360f);
        }
    }
}
