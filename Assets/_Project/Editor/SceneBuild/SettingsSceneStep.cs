using LastHaven.GameFlow;
using UnityEditor;
using UnityEngine;
using SettingsService = LastHaven.GameFlow.SettingsService;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/Settings (14.3): <see cref="SettingsService"/> (ISettingsService, PlayerPrefs; varsayılanlar
    /// <see cref="SettingsConfig"/>) ve <see cref="SettingsApplier"/> (kamera preset'i, sarsıntı, tam ekran). Ana menü sahnesi de aynı
    /// objeyi <see cref="Create"/> ile kurar. Ses düzeylerini [Systems]/Audio'daki <c>AudioVolumeSettings</c> uygular.
    /// </summary>
    public sealed class SettingsSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Settings";

        public int Order => 5; // Ayarları Start'ta okuyanlardan (kamera, ses) önce kaydolsun diye erken; sıra Awake için önemli değil.

        public void Build(SceneBuildContext context)
        {
            Create(context);
        }

        public static GameObject Create(SceneBuildContext context)
        {
            GameObject settings = context.CreateObject(ObjectName, context.Systems);
            var service = settings.AddComponent<SettingsService>();
            var serialized = new SerializedObject(service);
            serialized.FindProperty("_config").objectReferenceValue = SettingsGenerator.LoadOrCreateConfig();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            settings.AddComponent<SettingsApplier>();
            return settings;
        }
    }
}
