using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary><see cref="PlayerBuildConfig"/> asset'ini eksikse oluşturur (15.3). Player Settings'e yazan <see cref="PlayerSettingsSetup"/>.</summary>
    public sealed class PlayerBuildGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/Build/PlayerBuildConfig.asset";

        public int Order => 1900;

        public string Name => "Player Build";

        /// <summary>Setup Project ve build menüleri config'i buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static PlayerBuildConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<PlayerBuildConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Player Build: {ConfigPath} oluşturuldu.");
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
