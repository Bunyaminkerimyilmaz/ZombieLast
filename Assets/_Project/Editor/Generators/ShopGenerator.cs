using System.Collections.Generic;
using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Market (GDD §6, §10): <see cref="ShopDefinitions"/>'taki öğelerin <see cref="ShopItemData"/> asset'leri
    /// (<c>Data/Shop/Items/&lt;Ad&gt;.asset</c>) ve <see cref="ShopCatalog"/> (<c>Data/Shop/ShopCatalog.asset</c>). Varsa dokunulmaz
    /// (inspector'daki fiyat/denge ayarları korunur); katalogda olmayan öğeler sona eklenir.
    /// </summary>
    public sealed class ShopGenerator : IGenerationStep
    {
        public const string CatalogPath = ShopDefinitions.DataFolder + "/ShopCatalog.asset";

        private const string ItemsFieldName = "_items";

        public int Order => 1500;

        public string Name => "Shop";

        public static string GetItemPath(string assetName)
        {
            return $"{ShopDefinitions.ItemsFolder}/{assetName}.asset";
        }

        /// <summary>Öğeleri ve katalogu yükler; eksikleri oluşturur, katalogda olmayan öğeleri ekler. Sahne adımı buradan alır.</summary>
        public static ShopCatalog LoadOrCreateAll()
        {
            var catalog = EditorAssetUtility.LoadOrCreate<ShopCatalog>(CatalogPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Shop: {CatalogPath} oluşturuldu.");
            }

            var items = new List<ShopItemData>(ShopDefinitions.All.Count);
            foreach (ShopDefinitions.Definition definition in ShopDefinitions.All)
            {
                items.Add(LoadOrCreateItem(definition));
            }

            int added = AddMissing(catalog, items);
            if (added > 0)
            {
                AssetDatabase.SaveAssetIfDirty(catalog);
                Debug.Log($"[Last Haven] Shop: {CatalogPath} içine {added} öğe eklendi.");
            }

            return catalog;
        }

        public void GenerateData()
        {
            LoadOrCreateAll();
        }

        public void GeneratePrefabs()
        {
        }

        private static ShopItemData LoadOrCreateItem(ShopDefinitions.Definition definition)
        {
            string path = GetItemPath(definition.AssetName);
            var item = EditorAssetUtility.LoadOrCreate<ShopItemData>(path, out bool created);
            if (created)
            {
                Apply(item, definition);
                Debug.Log($"[Last Haven] Shop: {path} oluşturuldu.");
            }

            return item;
        }

        private static void Apply(ShopItemData item, ShopDefinitions.Definition definition)
        {
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_id").stringValue = definition.Id;
            serialized.FindProperty("_displayName").stringValue = definition.DisplayName;
            serialized.FindProperty("_description").stringValue = definition.Description;
            serialized.FindProperty("_category").enumValueIndex = (int)definition.Category;
            SerializedProperty prices = serialized.FindProperty("_prices");
            prices.arraySize = definition.Prices.Length;
            for (int i = 0; i < definition.Prices.Length; i++)
            {
                prices.GetArrayElementAtIndex(i).intValue = definition.Prices[i];
            }

            serialized.FindProperty("_maxLevel").intValue = definition.MaxLevel;
            serialized.FindProperty("_requiredItemId").stringValue = definition.RequiredItemId;
            serialized.FindProperty("_effect").enumValueIndex = (int)definition.Effect;
            serialized.FindProperty("_targetId").stringValue = definition.TargetId;
            serialized.FindProperty("_upgradeType").enumValueIndex = (int)definition.UpgradeType;
            serialized.FindProperty("_value").floatValue = definition.Value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(item);
        }

        // Katalogda (referans olarak) olmayan öğeleri sona ekler.
        private static int AddMissing(ShopCatalog catalog, List<ShopItemData> items)
        {
            var serialized = new SerializedObject(catalog);
            SerializedProperty list = serialized.FindProperty(ItemsFieldName);
            int added = 0;
            foreach (ShopItemData item in items)
            {
                if (Contains(list, item))
                {
                    continue;
                }

                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = item;
                added++;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return added;
        }

        private static bool Contains(SerializedProperty list, Object item)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == item)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
