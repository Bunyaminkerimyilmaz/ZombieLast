using LastHaven.GameFlow;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary><see cref="GameConfig"/> asset'ini eksikse oluşturur (GDD §3, §5, §9 varsayılanlarıyla). Varsa dokunulmaz.</summary>
    public sealed class GameFlowGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/GameFlow/GameConfig.asset";

        public int Order => 1000;

        public string Name => "GameFlow";

        /// <summary>Sahne adımı config'i buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static GameConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<GameConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] GameFlow: {ConfigPath} oluşturuldu.");
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
