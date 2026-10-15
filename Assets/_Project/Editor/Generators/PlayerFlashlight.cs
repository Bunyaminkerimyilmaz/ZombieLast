using LastHaven.Core;
using LastHaven.Player;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Oyuncu prefab'ının feneri (GDD §18, 6.2): kökün çocuğu "Flashlight" (Visual dışında; model değişince kaybolmasın),
    /// kapalı başlayan spot ışık + <see cref="Flashlight"/>. Kök nişan yönüne döndüğü için fener de nişanla döner.
    /// </summary>
    public static class PlayerFlashlight
    {
        public const string ObjectName = "Flashlight";

        private const string LightFieldName = "_light";

        // GDD'de sadece "spot ışık". Değerler oyun kamerasından ekran görüntüsüyle seçildi; prefab'dan ayarlanır.
        // Konum: gövde (0.5 m yarıçap) ve burun küpünün (y 1.4–1.6, z ≤ 0.65) önünde, göğüs hizasında; ışık kendi gövdesine çarpmaz.
        private static readonly Vector3 LocalPosition = new Vector3(0f, 1.25f, 0.75f);

        // Hafif yere bakar: ışık konisinin ortası ~6 m önde yere değer.
        private static readonly Vector3 LocalEulerAngles = new Vector3(12f, 0f, 0f);
        private static readonly Color LightColor = new Color(1f, 0.95f, 0.85f);
        private const float Range = 22f;
        private const float SpotAngle = 70f;
        private const float InnerSpotAngle = 40f;
        private const float Intensity = 25f;

        /// <summary>
        /// "Flashlight" çocuğu yoksa kurar; varsa sadece eksik <see cref="Flashlight"/> bileşenini ekler ve boş ışık referansını
        /// doldurur. Değişiklik sayısını döndürür.
        /// </summary>
        public static int Ensure(GameObject root)
        {
            int changes = 0;
            Transform child = root.transform.Find(ObjectName);
            if (child == null)
            {
                child = Build(root.transform);
                changes++;
            }

            if (!child.TryGetComponent(out Flashlight flashlight))
            {
                flashlight = child.gameObject.AddComponent<Flashlight>();
                changes++;
            }

            if (child.TryGetComponent(out Light light) && EditorAssetUtility.AssignIfMissing(flashlight, LightFieldName, light))
            {
                changes++;
            }

            return changes;
        }

        private static Transform Build(Transform root)
        {
            var flashlight = new GameObject(ObjectName) { layer = GameLayers.Player };
            flashlight.transform.SetParent(root, false);
            flashlight.transform.SetLocalPositionAndRotation(LocalPosition, Quaternion.Euler(LocalEulerAngles));

            var light = flashlight.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = LightColor;
            light.intensity = Intensity;
            light.range = Range;
            light.spotAngle = SpotAngle;
            light.innerSpotAngle = InnerSpotAngle;
            light.shadows = LightShadows.Soft;
            light.lightmapBakeType = LightmapBakeType.Realtime;

            // Oyun gündüz başlar; gece Flashlight açar.
            light.enabled = false;
            return flashlight.transform;
        }
    }
}
