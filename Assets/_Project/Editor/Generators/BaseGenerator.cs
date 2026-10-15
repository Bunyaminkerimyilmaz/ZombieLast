using LastHaven.Building;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Üs (base): <see cref="GeneratorData"/> ve <see cref="BaseAreaConfig"/> (<c>Data/Base/</c>), Generator prefab'ı
    /// (<see cref="GeneratorPrefab"/>). Varsa dokunulmaz; prefab'da sadece eksik bileşen ve boş referanslar tamamlanır.
    /// </summary>
    public sealed class BaseGenerator : IGenerationStep
    {
        public const string DataFolder = "Assets/_Project/Data/Base";
        public const string GeneratorDataPath = DataFolder + "/GeneratorData.asset";
        public const string AreaConfigPath = DataFolder + "/BaseAreaConfig.asset";

        public int Order => 800;

        public string Name => "Base";

        /// <summary>Veriyi yükler; yoksa GDD §12 varsayılanlarıyla oluşturur.</summary>
        public static GeneratorData LoadOrCreateGeneratorData()
        {
            var data = EditorAssetUtility.LoadOrCreate<GeneratorData>(GeneratorDataPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Base: {GeneratorDataPath} oluşturuldu.");
            }

            return data;
        }

        /// <summary>Config'i yükler; yoksa GDD §11 varsayılanlarıyla oluşturur. Harita ve sahne adımları yarıçapı buradan alır.</summary>
        public static BaseAreaConfig LoadOrCreateAreaConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<BaseAreaConfig>(AreaConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Base: {AreaConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>Veri + prefab. Sahne adımı buradan alır; Generate Prefabs çalışmamış olsa da prefab oluşur.</summary>
        public static Generator LoadOrCreateGeneratorPrefab()
        {
            return GeneratorPrefab.LoadOrCreate(LoadOrCreateGeneratorData());
        }

        public void GenerateData()
        {
            LoadOrCreateGeneratorData();
            LoadOrCreateAreaConfig();
        }

        public void GeneratePrefabs()
        {
            LoadOrCreateGeneratorPrefab();
        }
    }
}
