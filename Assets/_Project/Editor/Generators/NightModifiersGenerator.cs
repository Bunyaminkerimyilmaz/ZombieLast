using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Gece olayları (GDD §14; <see cref="NightModifierDefinitions"/>) → <c>Data/Waves/Modifiers/&lt;Ad&gt;.asset</c>. Varsa dokunulmaz.
    /// Geceler olayları id ile bağlar (<see cref="WavesGenerator"/>, sonra çalışır): Sis 4 ve 9, Kan Ayı 5 ve 10, Fırtına 7.
    /// </summary>
    public sealed class NightModifiersGenerator : IGenerationStep
    {
        public const string Folder = WavesGenerator.DataFolder + "/Modifiers";

        public int Order => 850; // WavesGenerator'dan (900) önce: geceler olayları id ile bulur.

        public string Name => "Night Modifiers";

        public static string GetPath(NightModifierDefinition definition)
        {
            return $"{Folder}/{definition.AssetName}.asset";
        }

        public static NightModifierData LoadOrCreate(NightModifierDefinition definition)
        {
            string path = GetPath(definition);
            var data = EditorAssetUtility.LoadOrCreate<NightModifierData>(path, out bool created);
            if (created)
            {
                Apply(data, definition);
                AssetDatabase.SaveAssetIfDirty(data);
                Debug.Log($"[Last Haven] Night Modifiers: {path} oluşturuldu.");
            }

            return data;
        }

        /// <summary>Tanımdaki değerleri yazar (sadece ilk oluşturmada). Testler de kullanır.</summary>
        public static void Apply(NightModifierData data, NightModifierDefinition definition)
        {
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = definition.Id;
            serialized.FindProperty("_displayName").stringValue = definition.DisplayName;
            serialized.FindProperty("_description").stringValue = definition.Description;
            serialized.FindProperty("_enemyCountMultiplier").floatValue = definition.EnemyCountMultiplier;
            serialized.FindProperty("_enemySpeedMultiplier").floatValue = definition.EnemySpeedMultiplier;
            serialized.FindProperty("_coinMultiplier").floatValue = definition.CoinMultiplier;
            serialized.FindProperty("_overrideFog").boolValue = definition.OverrideFog;
            if (definition.OverrideFog)
            {
                serialized.FindProperty("_fogColor").colorValue = definition.FogColor;
                serialized.FindProperty("_fogDensity").floatValue = definition.FogDensity;
            }

            serialized.FindProperty("_overrideLight").boolValue = definition.OverrideLight;
            if (definition.OverrideLight)
            {
                serialized.FindProperty("_lightColor").colorValue = definition.LightColor;
                serialized.FindProperty("_ambientColor").colorValue = definition.AmbientColor;
            }

            serialized.FindProperty("_powerOutages").boolValue = definition.PowerOutages;
            serialized.FindProperty("_outageInterval").floatValue = definition.OutageInterval;
            serialized.FindProperty("_outageDuration").floatValue = definition.OutageDuration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public void GenerateData()
        {
            foreach (NightModifierDefinition definition in NightModifierDefinitions.All)
            {
                LoadOrCreate(definition);
            }
        }

        public void GeneratePrefabs()
        {
        }
    }
}
