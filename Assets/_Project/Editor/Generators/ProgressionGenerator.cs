using System.Collections.Generic;
using LastHaven.Progression;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Seviye ve perk'ler (GDD §13): <see cref="ProgressionConfig"/> (eşik 10 + 6 × seviye, nadirlik 60/30/10, 3 kart),
    /// <see cref="PerkDefinitions"/>'taki 14 perk (<c>Data/Progression/Perks/&lt;Ad&gt;.asset</c>) ve <see cref="PerkCatalog"/>
    /// (<c>Data/Progression/PerkCatalog.asset</c>). Varsa dokunulmaz (inspector'daki denge ayarları korunur); katalogda olmayan
    /// perk'ler sona eklenir.
    /// </summary>
    public sealed class ProgressionGenerator : IGenerationStep
    {
        public const string ConfigPath = PerkDefinitions.DataFolder + "/ProgressionConfig.asset";
        public const string CatalogPath = PerkDefinitions.DataFolder + "/PerkCatalog.asset";

        private const string PerksFieldName = "_perks";

        public int Order => 1700;

        public string Name => "Progression";

        public static string GetPerkPath(string assetName)
        {
            return $"{PerkDefinitions.PerksFolder}/{assetName}.asset";
        }

        /// <summary>Sahne adımı config'i buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static ProgressionConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<ProgressionConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Progression: {ConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>Perk'leri ve katalogu yükler; eksikleri oluşturur, katalogda olmayan perk'leri ekler. Sahne adımı buradan alır.</summary>
        public static PerkCatalog LoadOrCreateCatalog()
        {
            var catalog = EditorAssetUtility.LoadOrCreate<PerkCatalog>(CatalogPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Progression: {CatalogPath} oluşturuldu.");
            }

            var perks = new List<PerkData>(PerkDefinitions.All.Count);
            foreach (PerkDefinitions.Definition definition in PerkDefinitions.All)
            {
                perks.Add(LoadOrCreatePerk(definition));
            }

            int added = AddMissing(catalog, perks);
            if (added > 0)
            {
                AssetDatabase.SaveAssetIfDirty(catalog);
                Debug.Log($"[Last Haven] Progression: {CatalogPath} içine {added} perk eklendi.");
            }

            return catalog;
        }

        public void GenerateData()
        {
            LoadOrCreateConfig();
            LoadOrCreateCatalog();
        }

        public void GeneratePrefabs()
        {
        }

        private static PerkData LoadOrCreatePerk(PerkDefinitions.Definition definition)
        {
            string path = GetPerkPath(definition.AssetName);
            var perk = EditorAssetUtility.LoadOrCreate<PerkData>(path, out bool created);
            if (created)
            {
                Apply(perk, definition);
                Debug.Log($"[Last Haven] Progression: {path} oluşturuldu.");
            }

            return perk;
        }

        /// <summary>Tanımı perk asset'ine yazar (testler bellek içi asset'le de kullanır).</summary>
        public static void Apply(PerkData perk, PerkDefinitions.Definition definition)
        {
            var serialized = new SerializedObject(perk);
            serialized.FindProperty("_id").stringValue = definition.Id;
            serialized.FindProperty("_displayName").stringValue = definition.DisplayName;
            serialized.FindProperty("_description").stringValue = definition.Description;
            serialized.FindProperty("_rarity").enumValueIndex = (int)definition.Rarity;
            serialized.FindProperty("_maxStacks").intValue = definition.MaxStacks;
            SerializedProperty effects = serialized.FindProperty("_effects");
            PerkEffect[] values = definition.CreateEffects();
            effects.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                effects.GetArrayElementAtIndex(i).managedReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (EditorUtility.IsPersistent(perk))
            {
                AssetDatabase.SaveAssetIfDirty(perk);
            }
        }

        // Katalogda (referans olarak) olmayan perk'leri sona ekler.
        private static int AddMissing(PerkCatalog catalog, List<PerkData> perks)
        {
            var serialized = new SerializedObject(catalog);
            SerializedProperty list = serialized.FindProperty(PerksFieldName);
            int added = 0;
            foreach (PerkData perk in perks)
            {
                if (Contains(list, perk))
                {
                    continue;
                }

                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = perk;
                added++;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return added;
        }

        private static bool Contains(SerializedProperty list, Object perk)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == perk)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
