using System.Collections.Generic;
using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Düşman tipleri (<see cref="EnemyDefinitions"/>): <see cref="EnemyData"/> asset'leri (<c>Data/Enemies/&lt;Tip&gt;.asset</c>),
    /// prefab'lar (<see cref="EnemyPrefabs"/>) ve <see cref="EnemySpawnerConfig"/>. Varsa dokunulmaz; sadece boş prefab/data
    /// referansları doldurulur ve config'te olmayan tipler listeye eklenir. Can barı materyalleri (<see cref="HealthBarMaterials"/>)
    /// veri aşamasında üretilir.
    /// </summary>
    public sealed class EnemiesGenerator : IGenerationStep
    {
        public const string Folder = "Assets/_Project/Data/Enemies";
        public const string ConfigPath = Folder + "/EnemySpawnerConfig.asset";

        private const string PrefabFieldName = "_prefab";
        private const string EntriesFieldName = "_enemies";
        private const string EntryDataFieldName = "_data";
        private const string EntryPrewarmFieldName = "_prewarm";

        public int Order => 700;

        public string Name => "Enemies";

        public static string GetDataPath(EnemyDefinition definition)
        {
            return $"{Folder}/{definition.AssetName}.asset";
        }

        /// <summary>Veriyi yükler; yoksa GDD §7 değerleriyle oluşturur. Prefab'a dokunmaz.</summary>
        public static EnemyData LoadOrCreateData(EnemyDefinition definition)
        {
            string path = GetDataPath(definition);
            var data = EditorAssetUtility.LoadOrCreate<EnemyData>(path, out bool created);
            if (created)
            {
                Apply(data, definition);
                AssetDatabase.SaveAssetIfDirty(data);
                Debug.Log($"[Last Haven] Enemies: {path} oluşturuldu.");
            }

            return data;
        }

        /// <summary>
        /// Tüm veriler + prefab'lar + config; prefab ↔ veri referanslarının boş olanlarını bağlar. Sahne adımları buradan alır;
        /// Generate Prefabs çalışmamış olsa da her şey oluşur.
        /// </summary>
        public static EnemySpawnerConfig LoadOrCreateAll()
        {
            List<EnemyData> types = LoadOrCreateAllData();
            for (int i = 0; i < types.Count; i++)
            {
                Enemy prefab = EnemyPrefabs.LoadOrCreate(EnemyDefinitions.All[i], types[i]);
                if (prefab != null && EditorAssetUtility.AssignIfMissing(types[i], PrefabFieldName, prefab))
                {
                    AssetDatabase.SaveAssetIfDirty(types[i]);
                    Debug.Log($"[Last Haven] Enemies: {GetDataPath(EnemyDefinitions.All[i])} prefab referansı bağlandı.");
                }
            }

            return LoadOrCreateConfig(types);
        }

        /// <summary>Tanımdaki değerleri veriye yazar (sadece ilk oluşturmada çağrılır). Testler de kullanır.</summary>
        public static void Apply(EnemyData data, EnemyDefinition definition)
        {
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = definition.Id;
            serialized.FindProperty("_displayName").stringValue = definition.DisplayName;
            serialized.FindProperty("_maxHealth").floatValue = definition.MaxHealth;
            serialized.FindProperty("_moveSpeed").floatValue = definition.MoveSpeed;
            serialized.FindProperty("_damage").floatValue = definition.Damage;
            serialized.FindProperty("_attackRate").floatValue = definition.AttackRate;
            serialized.FindProperty("_attackRange").floatValue = definition.AttackRange;
            serialized.FindProperty("_structureDamageMultiplier").floatValue = definition.StructureDamageMultiplier;
            serialized.FindProperty("_aggroRadius").floatValue = definition.AggroRadius;
            serialized.FindProperty("_hasPreferredTarget").boolValue = definition.PreferredTarget.HasValue;
            if (definition.PreferredTarget.HasValue)
            {
                serialized.FindProperty("_preferredTarget").intValue = (int)definition.PreferredTarget.Value;
            }

            serialized.FindProperty("_moneyReward").intValue = definition.MoneyReward;
            serialized.FindProperty("_xpReward").intValue = definition.XpReward;
            serialized.FindProperty("_knockbackResistance").floatValue = definition.KnockbackResistance;
            serialized.FindProperty("_burrowDepth").floatValue = definition.BurrowDepth;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public void GenerateData()
        {
            HealthBarMaterials.EnsureAll();
            LoadOrCreateConfig(LoadOrCreateAllData());
        }

        public void GeneratePrefabs()
        {
            LoadOrCreateAll();
        }

        /// <summary>Tanım sırasıyla (<see cref="EnemyDefinitions.All"/>) tüm veriler.</summary>
        private static List<EnemyData> LoadOrCreateAllData()
        {
            var types = new List<EnemyData>(EnemyDefinitions.All.Count);
            for (int i = 0; i < EnemyDefinitions.All.Count; i++)
            {
                types.Add(LoadOrCreateData(EnemyDefinitions.All[i]));
            }

            return types;
        }

        /// <summary>Config'i yükler ya da oluşturur; listede olmayan tipleri tanımdaki ön ısıtma sayısıyla ekler.</summary>
        private static EnemySpawnerConfig LoadOrCreateConfig(List<EnemyData> types)
        {
            var config = EditorAssetUtility.LoadOrCreate<EnemySpawnerConfig>(ConfigPath, out bool created);
            var serialized = new SerializedObject(config);
            SerializedProperty entries = serialized.FindProperty(EntriesFieldName);
            int added = 0;
            for (int i = 0; i < types.Count; i++)
            {
                if (types[i] == null || Contains(entries, types[i]))
                {
                    continue;
                }

                entries.arraySize++;
                SerializedProperty entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                entry.FindPropertyRelative(EntryDataFieldName).objectReferenceValue = types[i];
                entry.FindPropertyRelative(EntryPrewarmFieldName).intValue = EnemyDefinitions.All[i].Prewarm;
                added++;
            }

            if (added > 0)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(config);
            }

            if (created)
            {
                Debug.Log($"[Last Haven] Enemies: {ConfigPath} oluşturuldu ({added} tip).");
            }
            else if (added > 0)
            {
                Debug.Log($"[Last Haven] Enemies: {ConfigPath} listesine {added} tip eklendi.");
            }

            return config;
        }

        private static bool Contains(SerializedProperty entries, EnemyData data)
        {
            for (int i = 0; i < entries.arraySize; i++)
            {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative(EntryDataFieldName).objectReferenceValue == data)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
