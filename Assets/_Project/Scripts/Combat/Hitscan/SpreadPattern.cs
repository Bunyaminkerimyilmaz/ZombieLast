using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>
    /// Saçılma hesabı (saf). Saçılma yatay düzlemdedir (dünya yukarı ekseni etrafında): tepeden kamerada aşağı sapan saçmalar
    /// menzilden önce yere gömülmesin diye.
    /// </summary>
    public static class SpreadPattern
    {
        /// <summary>
        /// Saçmanın merkezden sapma açısı (derece, ±<paramref name="spreadAngle"/>/2). Koni saçma sayısı kadar eşit dilime bölünür;
        /// her saçma kendi diliminde rastgele bir açı alır. Böylece saçmalar koniye yayılır ama yine de rastgele düşer.
        /// </summary>
        /// <param name="random01">0..1 arası rastgele değer (dışındakiler kırpılır).</param>
        public static float GetPelletAngle(int pelletIndex, int pelletCount, float spreadAngle, float random01)
        {
            if (pelletCount <= 0 || spreadAngle <= 0f)
            {
                return 0f;
            }

            float slice = spreadAngle / pelletCount;
            return -spreadAngle * 0.5f + (pelletIndex + Mathf.Clamp01(random01)) * slice;
        }

        /// <summary>Yönü dünya yukarı ekseni etrafında döndürür (derece; yukarıdan bakınca pozitif = saat yönü).</summary>
        public static Vector3 RotateAroundUp(Vector3 direction, float degrees)
        {
            return Quaternion.AngleAxis(degrees, Vector3.up) * direction;
        }
    }
}
