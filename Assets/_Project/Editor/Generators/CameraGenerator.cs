using LastHaven.Player;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary><see cref="CameraConfig"/> asset'ini eksikse oluşturur (GDD §4 varsayılanlarıyla).</summary>
    public sealed class CameraGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/Camera/CameraConfig.asset";

        public int Order => 300;

        public string Name => "Camera";

        /// <summary>Sahne adımı config'i buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static CameraConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<CameraConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Camera: {ConfigPath} oluşturuldu.");
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
