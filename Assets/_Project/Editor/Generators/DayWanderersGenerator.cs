using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Gündüz gezginleri (11.2, GDD §9): <see cref="DayWanderersConfig"/> (<c>Data/Waves/DayWanderersConfig.asset</c>) eksikse
    /// varsayılanlarla oluşturulur; boş gezgin tipine Yürüyen verisi bağlanır. Var olan ayarlara dokunulmaz.
    /// </summary>
    public sealed class DayWanderersGenerator : IGenerationStep
    {
        public const string ConfigPath = WavesGenerator.DataFolder + "/DayWanderersConfig.asset";

        private const string EnemyField = "_enemy";

        public int Order => 950; // EnemiesGenerator'dan (700) sonra: Yürüyen verisi bağlanır.

        public string Name => "DayWanderers";

        /// <summary>Config'i yükler veya oluşturur; boş tipe Yürüyen'i bağlar. Sahne adımı buradan alır.</summary>
        public static DayWanderersConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<DayWanderersConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] DayWanderers: {ConfigPath} oluşturuldu.");
            }

            if (EditorAssetUtility.AssignIfMissing(config, EnemyField, EnemiesGenerator.LoadOrCreateData(EnemyDefinitions.Walker)))
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] DayWanderers: {ConfigPath} gezgin tipine Yürüyen bağlandı.");
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
