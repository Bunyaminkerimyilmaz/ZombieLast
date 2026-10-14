using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Silah testi kuklalarının <see cref="TestDummiesConfig"/> asset'ini eksikse oluşturur ve
    /// <c>Tools/Last Haven/Options/Build Test Dummies</c> açma/kapama menüsünü sağlar.
    /// </summary>
    public sealed class TestDummiesGenerator : IGenerationStep
    {
        public const string ConfigPath = "Assets/_Project/Data/Debug/TestDummiesConfig.asset";

        private const string ToggleMenuPath = "Tools/Last Haven/Options/Build Test Dummies";
        private const string EnabledFieldName = "_enabled";

        public int Order => 500;

        public string Name => "Test Dummies";

        /// <summary>Sahne adımı config'i buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static TestDummiesConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<TestDummiesConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Test Dummies: {ConfigPath} oluşturuldu.");
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

        [MenuItem(ToggleMenuPath, priority = 40)]
        private static void ToggleEnabled()
        {
            TestDummiesConfig config = LoadOrCreateConfig();
            var serialized = new SerializedObject(config);
            SerializedProperty enabled = serialized.FindProperty(EnabledFieldName);
            enabled.boolValue = !enabled.boolValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(config);

            string state = enabled.boolValue ? "açık" : "kapalı";
            Debug.Log($"[Last Haven] Test kuklaları {state}. Sahneye yansıması için Tools/Last Haven/Build Game Scene çalıştır.");
        }

        [MenuItem(ToggleMenuPath, true)]
        private static bool ValidateToggleEnabled()
        {
            // Menü açılırken asset oluşturulmaz; yoksa varsayılan (açık) gösterilir.
            var config = AssetDatabase.LoadAssetAtPath<TestDummiesConfig>(ConfigPath);
            Menu.SetChecked(ToggleMenuPath, config == null || config.Enabled);
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }
    }
}
