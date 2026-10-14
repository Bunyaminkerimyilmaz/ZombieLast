using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Test haritasının <see cref="MapLayoutConfig"/> asset'ini eksikse oluşturur.</summary>
    public sealed class MapGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/Map/MapLayoutConfig.asset";

        public int Order => 100;

        public string Name => "Map";

        /// <summary>Sahne adımları config'i buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static MapLayoutConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<MapLayoutConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Map: {ConfigPath} oluşturuldu.");
            }

            return config;
        }

        public void GenerateData()
        {
            LoadOrCreateConfig();
        }

        public void GeneratePrefabs()
        {
        }
    }
}
