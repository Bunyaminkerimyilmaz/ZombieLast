using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Ekrana yansıtılmış bir dünya nesnesinin yanına kutu yerleştirme (saf): kutunun alt ortası nesnenin üst noktasının
    /// <c>gap</c> kadar üstünde; üstte yer yoksa üst ortası nesnenin alt noktasının <c>gap</c> kadar altında. Her durumda kutu ekran
    /// kenarından en az <c>inset</c> içeride kalır. Koordinatlar panel uzayında (y aşağı doğru artar).
    /// </summary>
    public static class AnchorPlacement
    {
        /// <summary>Tek noktaya (üst = alt) göre yerleştirir; bkz. <see cref="AboveOrBelow"/>.</summary>
        public static Vector2 Above(Vector2 anchor, Vector2 size, Vector2 bounds, float gap, float inset)
        {
            return AboveOrBelow(anchor, anchor, size, bounds, gap, inset);
        }

        /// <param name="top">Nesnenin ekrandaki üst noktası (ebeveyn elementte).</param>
        /// <param name="bottom">Nesnenin ekrandaki alt noktası; üstte yer yoksa kutu bunun altına konur.</param>
        /// <param name="size">Kutunun boyutu (henüz ölçülmediyse NaN/negatif 0 sayılır).</param>
        /// <param name="bounds">Ebeveynin (ekranın) boyutu.</param>
        /// <param name="gap">Kutu ile nesne arasındaki boşluk.</param>
        /// <param name="inset">Ekran kenarına en az uzaklık. Kutu ekrana sığmıyorsa sol/üst kenara yaslanır.</param>
        /// <returns>Kutunun sol üst köşesi.</returns>
        public static Vector2 AboveOrBelow(Vector2 top, Vector2 bottom, Vector2 size, Vector2 bounds, float gap, float inset)
        {
            float width = Sanitize(size.x);
            float height = Sanitize(size.y);
            Vector2 anchor = top;
            float y = top.y - gap - height;
            float below = bottom.y + gap;
            if (y < inset && below + height <= bounds.y - inset)
            {
                anchor = bottom;
                y = below;
            }

            float x = anchor.x - width * 0.5f;
            return new Vector2(Clamp(x, inset, bounds.x - width - inset), Clamp(y, inset, bounds.y - height - inset));
        }

        private static float Clamp(float value, float min, float max)
        {
            return max < min ? min : Mathf.Clamp(value, min, max);
        }

        private static float Sanitize(float value)
        {
            return float.IsNaN(value) || value < 0f ? 0f : value;
        }
    }
}
