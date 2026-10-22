using LastHaven.GameFlow;
using LastHaven.Player;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ayarların varsayılanları (14.3): <c>Data/GameFlow/SettingsConfig.asset</c>. İlk oluşturmada kamera preset'i ve ekran sarsıntısı
    /// <c>CameraConfig</c>'ten alınır (tek kaynak); ses düzeyleri 1. Varsa dokunulmaz.
    /// </summary>
    public sealed class SettingsGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/GameFlow/SettingsConfig.asset";

        public int Order => 1050; // CameraGenerator'dan (300) sonra.

        public string Name => "Settings";

        /// <summary>Sahne adımları buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static SettingsConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<SettingsConfig>(ConfigPath, out bool created);
            if (created)
            {
                CameraConfig camera = CameraGenerator.LoadOrCreateConfig();
                var serialized = new SerializedObject(config);
                serialized.FindProperty("_cameraPreset").intValue = (int)camera.DefaultPreset;
                serialized.FindProperty("_screenShake").boolValue = camera.ShakeEnabled;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Settings: {ConfigPath} oluşturuldu (kamera {camera.DefaultPreset}, sarsıntı {camera.ShakeEnabled}).");
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
