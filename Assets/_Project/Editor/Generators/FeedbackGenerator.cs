using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Feedback;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Atış efektleri: FX materyalleri, <see cref="CombatFeedbackConfig"/> (<c>Data/Feedback/</c>) ve
    /// <see cref="FeedbackPrefabs"/> (Tracer, Impact, MuzzleFlash). Varsa dokunulmaz; sadece config'teki boş prefab referansları
    /// doldurulur ve iz rengi listesinde olmayan silahlar eklenir.
    /// </summary>
    public sealed class FeedbackGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/Feedback/CombatFeedbackConfig.asset";

        private const string WeaponsFieldName = "_weapons";

        public int Order => 600; // WeaponsGenerator'dan (400) sonra: iz renkleri silah verisinden.

        public string Name => "Feedback";

        /// <summary>Config'i yükler ya da oluşturur ve eksik silahları iz rengi listesine ekler. Prefab referanslarına dokunmaz.</summary>
        public static CombatFeedbackConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<CombatFeedbackConfig>(ConfigPath, out bool created);
            int added = AddMissingWeapons(config, WeaponsGenerator.LoadOrCreateAll());
            if (created)
            {
                Debug.Log($"[Last Haven] Feedback: {ConfigPath} oluşturuldu.");
            }
            else if (added > 0)
            {
                Debug.Log($"[Last Haven] Feedback: {ConfigPath} iz rengi listesine {added} silah eklendi.");
            }

            return config;
        }

        /// <summary>
        /// Config + prefab'lar; config'teki boş prefab referanslarını doldurur. Sahne adımı buradan alır;
        /// Generate Prefabs çalışmamış olsa da her şey oluşur.
        /// </summary>
        public static CombatFeedbackConfig LoadOrCreateAll()
        {
            CombatFeedbackConfig config = LoadOrCreateConfig();
            int filled = Assign(config, "_tracerPrefab", FeedbackPrefabs.LoadOrCreateTracer())
                         + Assign(config, "_impactPrefab", FeedbackPrefabs.LoadOrCreateImpact())
                         + Assign(config, "_muzzleFlashPrefab", FeedbackPrefabs.LoadOrCreateMuzzleFlash());
            if (filled > 0)
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Feedback: {ConfigPath} içinde {filled} boş prefab referansı dolduruldu.");
            }

            return config;
        }

        public void GenerateData()
        {
            FxMaterials.EnsureAll();
            LoadOrCreateConfig();
        }

        public void GeneratePrefabs()
        {
            LoadOrCreateAll();
        }

        private static int Assign(CombatFeedbackConfig config, string fieldName, Component prefab)
        {
            return prefab != null && EditorAssetUtility.AssignIfMissing(config, fieldName, prefab) ? 1 : 0;
        }

        private static int AddMissingWeapons(CombatFeedbackConfig config, List<WeaponData> weapons)
        {
            var serialized = new SerializedObject(config);
            SerializedProperty list = serialized.FindProperty(WeaponsFieldName);
            int added = 0;
            foreach (WeaponData weapon in weapons)
            {
                if (weapon == null || Contains(list, weapon))
                {
                    continue;
                }

                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = weapon;
                added++;
            }

            if (added > 0)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(config);
            }

            return added;
        }

        private static bool Contains(SerializedProperty list, Object value)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
