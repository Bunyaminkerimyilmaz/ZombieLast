using LastHaven.Building;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Diken tuzağı prefab'ının tuzağa özel parçaları (GDD §18: koyu gri yassı küp). <see cref="BuildablePrefabs"/> kökü, Health,
    /// Structure ve onarımı kurar; tuzakta kökteki BoxCollider trigger'dır (alan, <see cref="AreaHeight"/> yüksekliğinde) ve
    /// NavMeshObstacle yoktur. Bu sınıf:
    /// <list type="bullet">
    /// <item>"Visual/Plate" (ayak izinden biraz küçük yassı küp) + "Visual/Spike_i" (3×3 diken, metal gri). Collider yok.</item>
    /// <item>Kökte <see cref="SpikeTrap"/> (+ <see cref="SpikeTrapConfig"/>).</item>
    /// </list>
    /// Ölçüler GDD'de yok; ekran görüntüsüyle seçildi. Var olan prefab'da sadece eksik bileşen ve boş config tamamlanır.
    /// </summary>
    public static class SpikeTrapPrefab
    {
        public const string PlateName = "Plate";
        public const string SpikePrefix = "Spike_";

        /// <summary>Trigger alanının yüksekliği (m): düşman kapsülünün altına yerden değsin (görselden yüksek; görünmez).</summary>
        public const float AreaHeight = 1f;

        /// <summary>Sıra başına diken sayısı (3×3).</summary>
        public const int SpikesPerRow = 3;

        private const string ConfigFieldName = "_config";
        private const float PlateHeight = 0.1f;
        private const float PlateInset = 0.1f; // Komşu yapıyla aradaki boşluk (m, kenar başına).
        private const float SpikeWidth = 0.12f;
        private const float SpikeHeight = 0.25f;

        /// <summary>Görselin yüksekliği (plaka + diken, m); tanımın yüksekliği (onarım barı bunun üstünde).</summary>
        public const float VisualHeight = PlateHeight + SpikeHeight;

        /// <summary>
        /// Görseli <paramref name="visual"/>'ın altına kurar (yeni prefab): <paramref name="footprintSize"/> ayak izinin X/Z boyu (m).
        /// </summary>
        public static void BuildVisual(Transform visual, Vector2 footprintSize, Material plate, Material spikes)
        {
            GameObject plateObject = CreatePart(PlateName, visual, plate);
            plateObject.transform.localPosition = new Vector3(0f, PlateHeight * 0.5f, 0f);
            plateObject.transform.localScale = new Vector3(
                Mathf.Max(SpikeWidth, footprintSize.x - 2f * PlateInset), PlateHeight, Mathf.Max(SpikeWidth, footprintSize.y - 2f * PlateInset));

            float stepX = (footprintSize.x - 2f * PlateInset) / SpikesPerRow;
            float stepZ = (footprintSize.y - 2f * PlateInset) / SpikesPerRow;
            for (int row = 0; row < SpikesPerRow; row++)
            {
                for (int column = 0; column < SpikesPerRow; column++)
                {
                    GameObject spike = CreatePart(SpikePrefix + (row * SpikesPerRow + column), visual, spikes);
                    spike.transform.localPosition = new Vector3(
                        (column - (SpikesPerRow - 1) * 0.5f) * stepX, PlateHeight + SpikeHeight * 0.5f, (row - (SpikesPerRow - 1) * 0.5f) * stepZ);
                    spike.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                    spike.transform.localScale = new Vector3(SpikeWidth, SpikeHeight, SpikeWidth);
                }
            }
        }

        /// <summary>Kökte <see cref="SpikeTrap"/> yoksa ekler, boş config'i doldurur. Değişiklik sayısını döndürür.</summary>
        public static int EnsureParts(GameObject root, SpikeTrapConfig config)
        {
            int changes = VisualFeedbackSetup.EnsureComponent<SpikeTrap>(root);
            if (config != null && EditorAssetUtility.AssignIfMissing(root.GetComponent<SpikeTrap>(), ConfigFieldName, config))
            {
                changes++;
            }

            return changes;
        }

        private static GameObject CreatePart(string name, Transform parent, Material material)
        {
            return PrimitiveFactory.Create(PrimitiveType.Cube, name, parent, material, GameLayers.Structure, withCollider: false);
        }
    }
}
