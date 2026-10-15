using LastHaven.DebugTools;
using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DebugWaves: F5 sıradaki gece, F6 tüm canlı düşmanları öldür (<see cref="DebugWaveControls"/>; sadece editör ve
    /// development build). F6'nın <see cref="EnemySpawner"/>'ı [Systems]/EnemySpawner'dan bağlanır.
    /// </summary>
    public sealed class DebugWavesSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DebugWaves";

        public int Order => 710; // EnemySpawnerSceneStep (50) ve WavesSceneStep (300) kurulmuş olmalı.

        public void Build(SceneBuildContext context)
        {
            GameObject controlsObject = context.CreateObject(ObjectName, context.Systems);
            var controls = controlsObject.AddComponent<DebugWaveControls>();

            Transform spawnerObject = context.Systems.Find(EnemySpawnerSceneStep.ObjectName);
            EnemySpawner spawner = spawnerObject != null ? spawnerObject.GetComponent<EnemySpawner>() : null;
            if (spawner == null)
            {
                Debug.LogWarning("[Last Haven] DebugWaves: [Systems]/EnemySpawner yok; F6 çalışmayacak.");
                return;
            }

            var serialized = new SerializedObject(controls);
            serialized.FindProperty("_enemySpawner").objectReferenceValue = spawner;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
