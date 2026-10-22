using LastHaven.Building;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Yapı perk değerleri (GDD §13): <see cref="BuildingPerkConfig"/> (<c>Data/Building/BuildingPerkConfig.asset</c>; Dikenli Duvar
    /// %30, Şok Taret %25 / 1 sn). Varsa dokunulmaz; <see cref="TurretConfig"/>'in boş perk referansı doldurulur. Sahnedeki
    /// <see cref="BarricadeThorns"/>'ı <see cref="BuildingPerksSceneStep"/> bağlar.
    /// </summary>
    public sealed class BuildingPerksGenerator : IGenerationStep
    {
        public const string ConfigPath = BuildableDefinitions.DataFolder + "/BuildingPerkConfig.asset";

        private const string TurretPerksFieldName = "_perks";

        public int Order => 1450; // BuildingGenerator'dan (1400; TurretConfig) sonra.

        public string Name => "Building Perks";

        /// <summary>Config'i yükler/oluşturur ve taret config'ine bağlar. Sahne adımı buradan alır.</summary>
        public static BuildingPerkConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<BuildingPerkConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Building Perks: {ConfigPath} oluşturuldu.");
            }

            TurretConfig turret = BuildingGenerator.LoadOrCreateTurretConfig();
            if (EditorAssetUtility.AssignIfMissing(turret, TurretPerksFieldName, config))
            {
                EditorUtility.SetDirty(turret);
                AssetDatabase.SaveAssetIfDirty(turret);
                Debug.Log($"[Last Haven] Building Perks: {BuildingGenerator.TurretConfigPath} perk referansı bağlandı.");
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
