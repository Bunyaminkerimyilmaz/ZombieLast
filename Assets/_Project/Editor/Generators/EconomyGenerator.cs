using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="EconomyConfig"/> asset'ini eksikse oluşturur (GDD §10 varsayılanlarıyla) ve jeton prefab'ını
    /// (<see cref="CoinPrefab"/>) üretip config'in boş prefab referansına bağlar. Varsa dokunulmaz.
    /// </summary>
    public sealed class EconomyGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/Economy/EconomyConfig.asset";

        private const string CoinPrefabFieldName = "_coinPrefab";

        public int Order => 1300;

        public string Name => "Economy";

        /// <summary>Config'i yükler ya da oluşturur. Prefab referansına dokunmaz.</summary>
        public static EconomyConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<EconomyConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Economy: {ConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>
        /// Config + jeton prefab'ı; config'teki boş prefab referansını doldurur. Sahne adımı buradan alır;
        /// Generate Prefabs çalışmamış olsa da her şey oluşur.
        /// </summary>
        public static EconomyConfig LoadOrCreateAll()
        {
            EconomyConfig config = LoadOrCreateConfig();
            Coin coin = CoinPrefab.LoadOrCreate();
            if (coin != null && EditorAssetUtility.AssignIfMissing(config, CoinPrefabFieldName, coin))
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Economy: {ConfigPath} içine jeton prefab'ı bağlandı.");
            }

            return config;
        }

        public void GenerateData()
        {
            LoadOrCreateConfig();
        }

        public void GeneratePrefabs()
        {
            LoadOrCreateAll();
        }
    }
}
