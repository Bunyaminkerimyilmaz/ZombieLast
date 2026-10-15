using System.Collections.Generic;
using LastHaven.Building;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// İnşa (GDD §11): <see cref="BuildConfig"/> (<c>Data/Building/BuildConfig.asset</c>), <see cref="RepairConfig"/>
    /// (<c>Data/Building/RepairConfig.asset</c>), önizleme materyali (<see cref="BuildGhostMaterial"/>), onarım barı dolgusu
    /// (<see cref="HealthBarMaterials"/>), <see cref="BuildableDefinitions"/>'taki yapıların data'ları (seviye materyalleri dahil)
    /// ve prefab'ları (<see cref="BuildablePrefabs"/>). Varsa dokunulmaz; sadece boş referanslar (önizleme materyali, data → prefab,
    /// prefab → onarım config'i/barı), prefab'da eksik Health/Structure/StructureRepair/onarım barı ve boş yuvalar doldurulur.
    /// Listede zaten olan yapı tekrar eklenmez.
    /// </summary>
    public sealed class BuildingGenerator : IGenerationStep
    {
        public const string ConfigPath = BuildableDefinitions.DataFolder + "/BuildConfig.asset";
        public const string RepairConfigPath = BuildableDefinitions.DataFolder + "/RepairConfig.asset";

        private const string GhostMaterialFieldName = "_ghostMaterial";
        private const string SlotsFieldName = "_slots";
        private const string PrefabFieldName = "_prefab";

        public int Order => 1400;

        public string Name => "Building";

        /// <summary>Config'i yükler ya da oluşturur; boş önizleme materyalini bağlar.</summary>
        public static BuildConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<BuildConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Building: {ConfigPath} oluşturuldu.");
            }

            Material ghost = BuildGhostMaterial.GetOrCreate();
            if (ghost != null && EditorAssetUtility.AssignIfMissing(config, GhostMaterialFieldName, ghost))
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Building: {ConfigPath} içine önizleme materyali bağlandı.");
            }

            return config;
        }

        /// <summary>Onarım ayarlarını (GDD §11; 8.3) yükler ya da varsayılanlarla oluşturur.</summary>
        public static RepairConfig LoadOrCreateRepairConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<RepairConfig>(RepairConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Building: {RepairConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>
        /// Config + yapı data'ları + prefab'lar; boş referansları ve yuvaları doldurur. Sahne adımı buradan alır;
        /// Generate Prefabs çalışmamış olsa da her şey oluşur.
        /// </summary>
        public static BuildConfig LoadOrCreateAll()
        {
            BuildConfig config = LoadOrCreateConfig();
            var repair = new BuildablePrefabs.RepairAssets(
                LoadOrCreateRepairConfig(), HealthBarMaterials.GetOrCreateBackground(), HealthBarMaterials.GetOrCreateRepairFill());
            List<BuildableData> buildables = LoadOrCreateBuildables();
            for (int i = 0; i < buildables.Count; i++)
            {
                BuildableDefinitions.Definition definition = BuildableDefinitions.All[i];
                GameObject prefab = BuildablePrefabs.LoadOrCreate(definition, config.CellSize, repair);
                if (prefab != null && EditorAssetUtility.AssignIfMissing(buildables[i], PrefabFieldName, prefab))
                {
                    AssetDatabase.SaveAssetIfDirty(buildables[i]);
                    Debug.Log($"[Last Haven] Building: {definition.AssetName} data'sına prefab bağlandı.");
                }
            }

            LinkSlots(config, buildables);
            return config;
        }

        /// <summary>Tanımlardaki her yapının data'sını yükler; yoksa tanımla oluşturur. Sıra <see cref="BuildableDefinitions.All"/> ile aynı.</summary>
        public static List<BuildableData> LoadOrCreateBuildables()
        {
            var result = new List<BuildableData>(BuildableDefinitions.All.Count);
            foreach (BuildableDefinitions.Definition definition in BuildableDefinitions.All)
            {
                string path = BuildableDefinitions.GetDataPath(definition);
                var data = EditorAssetUtility.LoadOrCreate<BuildableData>(path, out bool created);
                if (created)
                {
                    BuildableDefinitions.Apply(data, definition);
                    AssetDatabase.SaveAssetIfDirty(data);
                    Debug.Log($"[Last Haven] Building: {path} oluşturuldu.");
                }

                result.Add(data);
            }

            return result;
        }

        public void GenerateData()
        {
            BuildConfig config = LoadOrCreateConfig();
            LoadOrCreateRepairConfig();
            HealthBarMaterials.GetOrCreateRepairFill();
            LinkSlots(config, LoadOrCreateBuildables());
        }

        public void GeneratePrefabs()
        {
            LoadOrCreateAll();
        }

        /// <summary>
        /// Tanımın yuvası boşsa (veya liste kısaysa) yapıyı oraya yazar. Yapı listede herhangi bir yerde zaten varsa veya yuva
        /// doluysa dokunulmaz (kullanıcının sıralaması korunur).
        /// </summary>
        private static void LinkSlots(BuildConfig config, List<BuildableData> buildables)
        {
            var serialized = new SerializedObject(config);
            SerializedProperty slots = serialized.FindProperty(SlotsFieldName);
            int linked = 0;
            for (int i = 0; i < buildables.Count; i++)
            {
                int slot = BuildableDefinitions.All[i].SlotIndex;
                if (buildables[i] == null || Contains(slots, buildables[i]))
                {
                    continue;
                }

                if (slots.arraySize <= slot)
                {
                    // Büyütünce Unity son elemanı kopyalar; yeni yuvalar boş başlasın.
                    int oldSize = slots.arraySize;
                    slots.arraySize = slot + 1;
                    for (int j = oldSize; j < slots.arraySize; j++)
                    {
                        slots.GetArrayElementAtIndex(j).objectReferenceValue = null;
                    }
                }

                SerializedProperty element = slots.GetArrayElementAtIndex(slot);
                if (element.objectReferenceValue == null)
                {
                    element.objectReferenceValue = buildables[i];
                    linked++;
                }
            }

            if (linked > 0)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Building: {ConfigPath} içine {linked} yapı yuvası bağlandı.");
            }
        }

        private static bool Contains(SerializedProperty array, Object value)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
