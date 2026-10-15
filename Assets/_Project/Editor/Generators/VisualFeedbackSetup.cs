using LastHaven.Core;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Varlık prefab'larının ortak görsel geri bildirim bileşenleri: <see cref="AnimationDriver"/> ve <see cref="HitFlash"/>.
    /// Bileşen yoksa üretici değerleriyle ekler; varsa ayarlarına dokunmaz, sadece boş Visual referansını doldurur.
    /// </summary>
    public static class VisualFeedbackSetup
    {
        private const string VisualFieldName = "_visual";
        private const string ToppleLiftPath = "_procedural._toppleLift";
        private const string FlashColorFieldName = "_color";
        private const string FlashDurationFieldName = "_duration";

        /// <summary>AnimationDriver'ı ekler (devrilme yüksekliği = gövde yarıçapı). Değişiklik sayısını döndürür.</summary>
        public static int EnsureAnimationDriver(GameObject root, float toppleLift)
        {
            int changes = 0;
            if (!root.TryGetComponent(out AnimationDriver driver))
            {
                driver = root.AddComponent<AnimationDriver>();
                var serialized = new SerializedObject(driver);
                serialized.FindProperty(ToppleLiftPath).floatValue = toppleLift;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                changes++;
            }

            return changes + AssignVisual(driver, root);
        }

        /// <summary>HitFlash'ı verilen renk ve süreyle ekler. Değişiklik sayısını döndürür.</summary>
        public static int EnsureHitFlash(GameObject root, Color color, float duration)
        {
            int changes = 0;
            if (!root.TryGetComponent(out HitFlash flash))
            {
                flash = root.AddComponent<HitFlash>();
                var serialized = new SerializedObject(flash);
                serialized.FindProperty(FlashColorFieldName).colorValue = color;
                serialized.FindProperty(FlashDurationFieldName).floatValue = duration;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                changes++;
            }

            return changes + AssignVisual(flash, root);
        }

        /// <summary>Kökte bileşen yoksa ekler. Eklediyse 1 döndürür.</summary>
        public static int EnsureComponent<T>(GameObject root) where T : Component
        {
            if (root.TryGetComponent(out T _))
            {
                return 0;
            }

            root.AddComponent<T>();
            return 1;
        }

        private static int AssignVisual(Component component, GameObject root)
        {
            Transform visual = VisualRoot.Find(root.transform);
            if (visual == null)
            {
                Debug.LogWarning($"[Last Haven] {root.name}: '{VisualRoot.Name}' çocuğu yok; {component.GetType().Name} görsel bulamayacak.", root);
                return 0;
            }

            return EditorAssetUtility.AssignIfMissing(component, VisualFieldName, visual) ? 1 : 0;
        }
    }
}
