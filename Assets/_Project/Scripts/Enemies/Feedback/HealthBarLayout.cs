using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>Can barı dolgusunun ölçüsü (saf): merkez pivotlu quad'ı sola yaslı olarak kısaltır.</summary>
    public static class HealthBarLayout
    {
        /// <summary>
        /// <paramref name="normalized"/> (0..1, kırpılır) doluluk için dolgunun X ölçeği ve barın merkezine göre X konumu.
        /// </summary>
        public static void GetFill(float normalized, float width, out float scaleX, out float offsetX)
        {
            float fill = Mathf.Clamp01(normalized);
            scaleX = width * fill;
            offsetX = -0.5f * width * (1f - fill);
        }
    }
}
