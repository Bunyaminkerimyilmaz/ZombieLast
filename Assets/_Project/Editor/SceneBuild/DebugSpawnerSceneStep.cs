using LastHaven.DebugTools;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DebugSpawner: F1–F4 düşman doğurma kısayolları (<see cref="DebugSpawner"/>; sadece editör ve development build).
    /// Veriler üreticiden, F4'ün spawn bölgeleri [Gameplay]/SpawnZones'tan bağlanır.
    /// </summary>
    public sealed class DebugSpawnerSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DebugSpawner";

        public int Order => 700; // WavesSceneStep'ten (300) sonra: spawn bölgeleri kurulmuş olmalı.

        public void Build(SceneBuildContext context)
        {
            GameObject debugObject = context.CreateObject(ObjectName, context.Systems);
            var spawner = debugObject.AddComponent<DebugSpawner>();

            var serialized = new SerializedObject(spawner);
            serialized.FindProperty("_walker").objectReferenceValue = EnemiesGenerator.LoadOrCreateData(EnemyDefinitions.Walker);
            serialized.FindProperty("_runner").objectReferenceValue = EnemiesGenerator.LoadOrCreateData(EnemyDefinitions.Runner);
            serialized.FindProperty("_tank").objectReferenceValue = EnemiesGenerator.LoadOrCreateData(EnemyDefinitions.Tank);
            serialized.FindProperty("_edgeEnemy").objectReferenceValue = EnemiesGenerator.LoadOrCreateData(EnemyDefinitions.Walker);

            SpawnZone[] zones = FindSpawnZones(context);
            SerializedProperty zoneList = serialized.FindProperty("_edgeZones");
            zoneList.arraySize = zones.Length;
            for (int i = 0; i < zones.Length; i++)
            {
                zoneList.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (zones.Length == 0)
            {
                Debug.LogWarning("[Last Haven] DebugSpawner: [Gameplay]/SpawnZones altında SpawnZone yok; F4 çalışmayacak.");
            }
        }

        private static SpawnZone[] FindSpawnZones(SceneBuildContext context)
        {
            Transform group = context.Gameplay.Find(WavesSceneStep.SpawnZonesGroupName);
            return group != null ? group.GetComponentsInChildren<SpawnZone>() : new SpawnZone[0];
        }
    }
}
