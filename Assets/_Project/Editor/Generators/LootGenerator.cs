using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ganimet sandıkları (11.1, GDD §9): <see cref="LootTableData"/> (<c>Data/Loot/LootTable.asset</c>) ve
    /// <see cref="LootConfig"/> (<c>Data/Loot/LootConfig.asset</c>) eksikse GDD varsayılanlarıyla oluşturulur; prefab'lar
    /// (<see cref="LootPrefabs"/>: sandık, yakıt bidonu, mermi kutusu) üretilip config'in boş referanslarına, tablo da config'in boş
    /// tablo alanına bağlanır. Var olan asset'lere başka dokunulmaz.
    /// </summary>
    public sealed class LootGenerator : IGenerationStep
    {
        public const string Folder = "Assets/_Project/Data/Loot";
        public const string TablePath = Folder + "/LootTable.asset";
        public const string ConfigPath = Folder + "/LootConfig.asset";

        private const string TableField = "_lootTable";
        private const string CratePrefabField = "_cratePrefab";
        private const string FuelPickupField = "_fuelPickupPrefab";
        private const string AmmoPickupField = "_ammoPickupPrefab";

        public int Order => 1600;

        public string Name => "Loot";

        /// <summary>Tablo + config; config'in boş tablo referansını doldurur. Prefab'lara dokunmaz.</summary>
        public static LootConfig LoadOrCreateData()
        {
            var table = EditorAssetUtility.LoadOrCreate<LootTableData>(TablePath, out bool tableCreated);
            if (tableCreated)
            {
                Debug.Log($"[Last Haven] Loot: {TablePath} oluşturuldu.");
            }

            var config = EditorAssetUtility.LoadOrCreate<LootConfig>(ConfigPath, out bool configCreated);
            if (configCreated)
            {
                Debug.Log($"[Last Haven] Loot: {ConfigPath} oluşturuldu.");
            }

            if (EditorAssetUtility.AssignIfMissing(config, TableField, table))
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Loot: {ConfigPath} içine ödül tablosu bağlandı.");
            }

            return config;
        }

        /// <summary>
        /// Data + prefab'lar; config'teki boş prefab referanslarını doldurur. Sahne adımı buradan alır; Generate Prefabs çalışmamış
        /// olsa da her şey oluşur.
        /// </summary>
        public static LootConfig LoadOrCreateAll()
        {
            LootConfig config = LoadOrCreateData();
            int linked = 0;
            linked += Link(config, CratePrefabField, LootPrefabs.LoadOrCreateCrate());
            linked += Link(config, FuelPickupField, LootPrefabs.LoadOrCreateFuelPickup());
            linked += Link(config, AmmoPickupField, LootPrefabs.LoadOrCreateAmmoPickup());
            if (linked > 0)
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Loot: {ConfigPath} içine {linked} prefab bağlandı.");
            }

            return config;
        }

        public void GenerateData()
        {
            LoadOrCreateData();
        }

        public void GeneratePrefabs()
        {
            LoadOrCreateAll();
        }

        private static int Link(LootConfig config, string field, Object prefab)
        {
            return prefab != null && EditorAssetUtility.AssignIfMissing(config, field, prefab) ? 1 : 0;
        }
    }
}
