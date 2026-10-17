using LastHaven.Building;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Editor
{
    /// <summary>
    /// Jeneratör prefab'ı (<c>Prefabs/Base/Generator.prefab</c>). Kök: BoxCollider + NavMeshObstacle (carve yok) + Health + Generator
    /// + GeneratorPower (yakıt/güç) + EmergencyFuelStation (acil yakıt), Structure layer ve Generator tag. "Visual": sarı 2×2×2 küp (GDD §18, collider yok).
    /// "BaseLights": Visual dışında, çapraz yönlerde 4 nokta ışık + <see cref="BaseLights"/> (GDD §12).
    /// </summary>
    public static class GeneratorPrefab
    {
        public const string Folder = "Assets/_Project/Prefabs/Base";
        public const string PrefabPath = Folder + "/Generator.prefab";
        public const string RootName = "Generator";
        public const string BodyName = "Body";
        public const string LightsName = "BaseLights";
        public const int LightCount = 4;

        private const string DataFieldName = "_data";
        private const string LightsFieldName = "_lights";

        // Üs ışıkları GDD'de sadece "4 nokta ışık"; değerler gece üssü aydınlatsın diye seçildi, prefab'dan ayarlanır.
        private const float LightDistance = 6f;
        private const float LightHeight = 4f;
        private const float LightRange = 12f;
        private const float LightIntensity = 8f;
        private static readonly Color LightColor = new Color(1f, 0.82f, 0.55f);
        private static readonly string[] LightNames = { "Light_NE", "Light_SE", "Light_SW", "Light_NW" };

        /// <summary>GDD §18: 2×2×2 küp. Collider ve NavMeshObstacle da bu boyutta.</summary>
        public static readonly Vector3 Size = new Vector3(2f, 2f, 2f);

        /// <summary>
        /// Prefab'ı yükler; yoksa oluşturur. Varsa sadece eksikler tamamlanır (boş GeneratorData referansı, üs ışıkları,
        /// güç ve acil yakıt bileşenleri);
        /// diğer ayarlara dokunulmaz.
        /// </summary>
        public static Generator LoadOrCreate(GeneratorData data)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                return Create(data);
            }

            int changes = EditorAssetUtility.EditPrefab(
                PrefabPath, root => AssignData(root, data) + EnsureLights(root) + EnsurePowerComponents(root));
            if (changes > 0)
            {
                Debug.Log($"[Last Haven] Base: {PrefabPath} içinde {changes} eksik bileşen/referans tamamlandı.");
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            return prefab.GetComponent<Generator>();
        }

        /// <summary>
        /// Prefab hiyerarşisini sahnede (kaydetmeden) kurar. Çağıran yok etmekle sorumludur. Testler de kullanır
        /// (materyal null olabilir; asset oluşturulmaz).
        /// </summary>
        public static GameObject BuildRoot(Material material)
        {
            var root = new GameObject(RootName) { layer = GameLayers.Structure, tag = GameTags.Generator };
            Vector3 center = new Vector3(0f, Size.y * 0.5f, 0f);

            var collider = root.AddComponent<BoxCollider>();
            collider.size = Size;
            collider.center = center;

            // Carve yok: hedef noktası (merkez) NavMesh üzerinde kalsın, yol kısmi (PathPartial) çıkmasın.
            // Ajanlar yerel kaçınmayla etrafından geçer ve menzilde durur.
            var obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Size;
            obstacle.center = center;
            obstacle.carving = false;

            root.AddComponent<Health>();
            root.AddComponent<Generator>();
            EnsurePowerComponents(root);

            BuildVisual(root.transform, material);
            EnsureLights(root);
            return root;
        }

        private static Generator Create(GeneratorData data)
        {
            GameObject root = BuildRoot(PlaceholderMaterials.GetOrCreate(PlaceholderMaterialId.Generator));
            try
            {
                AssignData(root, data);
                EditorAssetUtility.EnsureFolder(Folder);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"[Last Haven] Base: {PrefabPath} oluşturuldu.");
                return prefab.GetComponent<Generator>();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildVisual(Transform root, Material material)
        {
            var visual = new GameObject(VisualRoot.Name) { layer = GameLayers.Structure };
            visual.transform.SetParent(root, false);

            GameObject body = PrimitiveFactory.Create(
                PrimitiveType.Cube, BodyName, visual.transform, material, GameLayers.Structure, withCollider: false);
            body.transform.localPosition = new Vector3(0f, Size.y * 0.5f, 0f);
            body.transform.localScale = Size;
        }

        /// <summary>
        /// "BaseLights" grubu yoksa 4 ışıkla kurar. Grup varsa sadece eksik <see cref="BaseLights"/> bileşenini ekler ve boş
        /// ışık listesini gruptaki ışıklarla doldurur. Değişiklik sayısını döndürür.
        /// </summary>
        private static int EnsureLights(GameObject root)
        {
            int changes = 0;
            Transform group = root.transform.Find(LightsName);
            if (group == null)
            {
                group = BuildLightGroup(root.transform);
                changes++;
            }

            if (!group.TryGetComponent(out BaseLights lights))
            {
                lights = group.gameObject.AddComponent<BaseLights>();
                changes++;
            }

            var serialized = new SerializedObject(lights);
            SerializedProperty array = serialized.FindProperty(LightsFieldName);
            if (array.arraySize > 0)
            {
                return changes;
            }

            Light[] found = group.GetComponentsInChildren<Light>(true);
            array.arraySize = found.Length;
            for (int i = 0; i < found.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = found[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return changes + (found.Length > 0 ? 1 : 0);
        }

        private static Transform BuildLightGroup(Transform root)
        {
            var group = new GameObject(LightsName) { layer = GameLayers.Structure };
            group.transform.SetParent(root, false);

            for (int i = 0; i < LightCount; i++)
            {
                // Çapraz yönler: KD, GD, GB, KB (+Z kuzey, +X doğu).
                float angle = (45f + 90f * i) * Mathf.Deg2Rad;
                var lightObject = new GameObject(LightNames[i]) { layer = GameLayers.Structure };
                lightObject.transform.SetParent(group.transform, false);
                lightObject.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * LightDistance, LightHeight, Mathf.Cos(angle) * LightDistance);

                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = LightRange;
                light.intensity = LightIntensity;
                light.color = LightColor;
                light.shadows = LightShadows.None;
                light.lightmapBakeType = LightmapBakeType.Realtime;
            }

            return group.transform;
        }

        /// <summary>Kökte eksik <see cref="GeneratorPower"/> ve <see cref="EmergencyFuelStation"/>'ı ekler (9.2). Eklenen sayısını döndürür.</summary>
        private static int EnsurePowerComponents(GameObject root)
        {
            int changes = 0;
            if (!root.TryGetComponent(out GeneratorPower _))
            {
                root.AddComponent<GeneratorPower>();
                changes++;
            }

            if (!root.TryGetComponent(out EmergencyFuelStation _))
            {
                root.AddComponent<EmergencyFuelStation>();
                changes++;
            }

            return changes;
        }

        private static int AssignData(GameObject root, GeneratorData data)
        {
            if (!root.TryGetComponent(out Generator generator))
            {
                Debug.LogError($"[Last Haven] Base: {PrefabPath} kökünde Generator bileşeni yok; GeneratorData bağlanamadı.", data);
                return 0;
            }

            return EditorAssetUtility.AssignIfMissing(generator, DataFieldName, data) ? 1 : 0;
        }
    }
}
