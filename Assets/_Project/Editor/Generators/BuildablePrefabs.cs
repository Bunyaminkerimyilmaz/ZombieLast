using LastHaven.Building;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Editor
{
    /// <summary>
    /// Kutu biçimli yapı prefab'ları (<c>Prefabs/Building/&lt;Ad&gt;.prefab</c>). Kök: Structure layer ve tag, ayak izi boyunda
    /// BoxCollider ve NavMeshObstacle (carving data'dan), <see cref="Health"/> (1. seviye canı; yerleşince data'dan),
    /// <see cref="Structure"/> ve onarım (<see cref="StructureRepair"/> + <see cref="RepairConfig"/>, kökün çocuğu "RepairBar").
    /// "Visual/Body": 1. seviye renginde küp, collider yok (seviye materyali buna uygulanır).
    /// Pivot ayak izinin merkezinde, yer seviyesinde. Varsa sadece eksik bileşenler ve boş onarım referansları tamamlanır.
    /// </summary>
    public static class BuildablePrefabs
    {
        public const string Folder = "Assets/_Project/Prefabs/Building";
        public const string BodyName = "Body";

        private const string MaxHealthFieldName = "_maxHealth";
        private const string RepairConfigFieldName = "_config";
        private const string RepairBarFieldName = "_bar";

        /// <summary>Onarımın asset'leri (8.3). Testlerde hepsi null olabilir: bileşenler kurulur, referans boş kalır.</summary>
        public readonly struct RepairAssets
        {
            public RepairAssets(RepairConfig config, Material barBackground, Material barFill)
            {
                Config = config;
                BarBackground = barBackground;
                BarFill = barFill;
            }

            public RepairConfig Config { get; }

            public Material BarBackground { get; }

            public Material BarFill { get; }
        }

        public static string GetPrefabPath(BuildableDefinitions.Definition definition)
        {
            return $"{Folder}/{definition.AssetName}.prefab";
        }

        /// <summary>Prefab'ı yükler (eksik bileşenleri tamamlar); yoksa tanımdan oluşturur.</summary>
        public static GameObject LoadOrCreate(BuildableDefinitions.Definition definition, float cellSize, in RepairAssets repair)
        {
            string path = GetPrefabPath(definition);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                RepairAssets assets = repair;
                int added = EditorAssetUtility.EditPrefab(path, root => EnsureStructureComponents(root, definition, assets));
                if (added > 0)
                {
                    Debug.Log($"[Last Haven] Building: {path} prefab'ına {added} eksik bileşen/referans eklendi.");
                }

                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            GameObject built = BuildRoot(definition, cellSize, PlaceholderMaterials.GetOrCreate(definition.Material), repair);
            try
            {
                EditorAssetUtility.EnsureFolder(Folder);
                prefab = PrefabUtility.SaveAsPrefabAsset(built, path);
                Debug.Log($"[Last Haven] Building: {path} oluşturuldu.");
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(built);
            }
        }

        /// <summary>
        /// Prefab hiyerarşisini sahnede (kaydetmeden) kurar. Çağıran yok etmekle sorumludur. Testler de kullanır (materyal ve
        /// onarım asset'leri null olabilir).
        /// </summary>
        public static GameObject BuildRoot(BuildableDefinitions.Definition definition, float cellSize, Material material,
            in RepairAssets repair = default)
        {
            var size = new Vector3(definition.Footprint.x * cellSize, definition.Height, definition.Footprint.y * cellSize);
            var center = new Vector3(0f, size.y * 0.5f, 0f);
            var root = new GameObject(definition.AssetName) { layer = GameLayers.Structure, tag = GameTags.Structure };

            var collider = root.AddComponent<BoxCollider>();
            collider.size = size;
            collider.center = center;

            var obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = size;
            obstacle.center = center;
            obstacle.carving = definition.CarvesNavMesh;

            var visual = new GameObject(VisualRoot.Name) { layer = GameLayers.Structure };
            visual.transform.SetParent(root.transform, false);

            GameObject body = PrimitiveFactory.Create(
                PrimitiveType.Cube, BodyName, visual.transform, material, GameLayers.Structure, withCollider: false);
            body.transform.localPosition = center;
            body.transform.localScale = size;

            EnsureStructureComponents(root, definition, repair);
            return root;
        }

        /// <summary>
        /// Kökte Health (maks. can = 1. seviye), Structure, StructureRepair ve onarım barı yoksa ekler; onarımın boş config ve bar
        /// referanslarını doldurur. Değişiklik sayısını döndürür.
        /// </summary>
        private static int EnsureStructureComponents(GameObject root, BuildableDefinitions.Definition definition, in RepairAssets repair)
        {
            int changes = 0;
            if (!root.TryGetComponent(out Health health))
            {
                health = root.AddComponent<Health>();
                var serialized = new SerializedObject(health);
                serialized.FindProperty(MaxHealthFieldName).floatValue = definition.Levels[0].MaxHealth;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                changes++;
            }

            changes += VisualFeedbackSetup.EnsureComponent<Structure>(root);
            return changes + EnsureRepair(root, definition, repair);
        }

        private static int EnsureRepair(GameObject root, BuildableDefinitions.Definition definition, in RepairAssets repair)
        {
            int changes = VisualFeedbackSetup.EnsureComponent<StructureRepair>(root);
            var component = root.GetComponent<StructureRepair>();

            StructureRepairBar bar = root.GetComponentInChildren<StructureRepairBar>(true);
            if (bar == null)
            {
                bar = StructureRepairBarBuilder.Build(root.transform, definition.Height, repair.BarBackground, repair.BarFill);
                changes++;
            }

            if (repair.Config != null && EditorAssetUtility.AssignIfMissing(component, RepairConfigFieldName, repair.Config))
            {
                changes++;
            }

            if (EditorAssetUtility.AssignIfMissing(component, RepairBarFieldName, bar))
            {
                changes++;
            }

            return changes;
        }
    }
}
