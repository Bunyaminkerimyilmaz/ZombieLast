using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Varlık prefab'larının iki katmanlı yapısı (CLAUDE.md "Görsel ayrımı"): kökte mantık, <see cref="Name"/> adlı çocukta
    /// sadece mesh ve animasyon. Kullanıcı modeli değiştirirken bu objenin altını değiştirir.
    /// </summary>
    public static class VisualRoot
    {
        public const string Name = "Visual";

        /// <summary>Kökün doğrudan çocuğu olan görsel objeyi döndürür; yoksa null.</summary>
        public static Transform Find(Transform root)
        {
            return root != null ? root.Find(Name) : null;
        }
    }
}
