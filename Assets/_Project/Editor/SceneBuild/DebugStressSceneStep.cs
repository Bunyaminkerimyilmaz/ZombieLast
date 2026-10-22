using LastHaven.Building;
using LastHaven.Core;
using LastHaven.DebugTools;
using LastHaven.Enemies;
using LastHaven.Progression;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DebugStress (15.2): performans stres testi (<see cref="DebugStressTest"/>; debug panelinden başlatılır, sadece editör
    /// ve development build). Bağlar: [Systems]/EnemySpawner, [Gameplay]/SpawnZones, [Systems]/BuildMode'daki <see cref="BuildSite"/>,
    /// jeneratörün Health'i. Karışım GDD §8 9. gecesinin oranı (80/35/8/15/15 ≈ 16/7/2/3/3).
    /// </summary>
    public sealed class DebugStressSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DebugStress";

        private static readonly (EnemyDefinition Enemy, int Weight)[] Mix =
        {
            (EnemyDefinitions.Walker, 16),
            (EnemyDefinitions.Runner, 7),
            (EnemyDefinitions.Tank, 2),
            (EnemyDefinitions.Spitter, 3),
            (EnemyDefinitions.Exploder, 3),
        };

        public int Order => 735; // Spawner (50), inşa (80), üs (250), bölgeler (300) kurulmuş olmalı; panelden (740) önce.

        public void Build(SceneBuildContext context)
        {
            var stress = context.CreateObject(ObjectName, context.Systems).AddComponent<DebugStressTest>();
            var serialized = new SerializedObject(stress);
            serialized.FindProperty("_spawner").objectReferenceValue = FindInSystems<EnemySpawner>(context, EnemySpawnerSceneStep.ObjectName);
            serialized.FindProperty("_buildSite").objectReferenceValue = FindInSystems<BuildSite>(context, BuildingSceneStep.ObjectName);
            var generator = context.Gameplay.GetComponentInChildren<Generator>(true);
            serialized.FindProperty("_generatorHealth").objectReferenceValue = generator != null ? generator.GetComponent<Health>() : null;
            serialized.FindProperty("_economy").objectReferenceValue = EconomyGenerator.LoadOrCreateConfig();
            serialized.FindProperty("_experience").objectReferenceValue = FindInSystems<ExperienceService>(context, ProgressionSceneStep.ObjectName);

            Transform zoneGroup = context.Gameplay.Find(WavesSceneStep.SpawnZonesGroupName);
            SpawnZone[] zones = zoneGroup != null ? zoneGroup.GetComponentsInChildren<SpawnZone>() : new SpawnZone[0];
            SerializedProperty zoneList = serialized.FindProperty("_zones");
            zoneList.arraySize = zones.Length;
            for (int i = 0; i < zones.Length; i++)
            {
                zoneList.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
            }

            SerializedProperty mix = serialized.FindProperty("_mix");
            mix.arraySize = Mix.Length;
            for (int i = 0; i < Mix.Length; i++)
            {
                SerializedProperty entry = mix.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_enemy").objectReferenceValue = EnemiesGenerator.LoadOrCreateData(Mix[i].Enemy);
                entry.FindPropertyRelative("_weight").intValue = Mix[i].Weight;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (zones.Length == 0 || generator == null)
            {
                Debug.LogWarning("[Last Haven] DebugStress: spawn bölgesi veya jeneratör yok; stres testi eksik çalışır.");
            }
        }

        private static T FindInSystems<T>(SceneBuildContext context, string objectName) where T : Component
        {
            Transform child = context.Systems.Find(objectName);
            T component = child != null ? child.GetComponent<T>() : null;
            if (component == null)
            {
                Debug.LogWarning($"[Last Haven] DebugStress: [Systems]/{objectName} ({typeof(T).Name}) yok.");
            }

            return component;
        }
    }
}
