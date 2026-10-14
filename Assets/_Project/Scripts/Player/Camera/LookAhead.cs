using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>Kamera takip hedefinin nişan yönüne kayması (saf, test edilebilir).</summary>
    public static class LookAhead
    {
        /// <summary>
        /// Oyuncudan nişan noktasına yatay ofset; en fazla <paramref name="maxDistance"/> uzunlukta.
        /// Nişan noktası yakınsa kayma da o kadar azdır.
        /// </summary>
        public static Vector3 Compute(Vector3 playerPosition, Vector3 aimPoint, float maxDistance)
        {
            Vector3 offset = aimPoint - playerPosition;
            offset.y = 0f;
            return Vector3.ClampMagnitude(offset, Mathf.Max(0f, maxDistance));
        }
    }
}
