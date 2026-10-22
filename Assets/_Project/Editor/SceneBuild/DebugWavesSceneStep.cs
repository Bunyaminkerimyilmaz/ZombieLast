using LastHaven.DebugTools;
using LastHaven.Enemies;
using LastHaven.GameFlow;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DebugWaves: F5 geceyi atla (akışsız sahnede sıradaki gece), F6 tüm canlı düşmanları öldür, panelden N. geceye git
    /// (<see cref="DebugWaveControls"/>; sadece editör ve development build). F6'nın <see cref="EnemySpawner"/>'ı
    /// [Systems]/EnemySpawner'dan, geceye atlamanın <see cref="GameFlowController"/>'ı [Systems]/GameFlow'dan bağlanır.
    /// </summary>
    public sealed class DebugWavesSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DebugWaves";

        public int Order => 710; // EnemySpawnerSceneStep (50), GameFlowSceneStep (60) ve WavesSceneStep (300) kurulmuş olmalı.

        public void Build(SceneBuildContext context)
        {
            GameObject controlsObject = context.CreateObject(ObjectName, context.Systems);
            var controls = controlsObject.AddComponent<DebugWaveControls>();
            var serialized = new SerializedObject(controls);

            Transform spawnerObject = context.Systems.Find(EnemySpawnerSceneStep.ObjectName);
            EnemySpawner spawner = spawnerObject != null ? spawnerObject.GetComponent<EnemySpawner>() : null;
            if (spawner == null)
            {
                Debug.LogWarning("[Last Haven] DebugWaves: [Systems]/EnemySpawner yok; F6 çalışmayacak.");
            }

            // Akış opsiyonel: yoksa F5 sıradaki geceyi başlatır, N. geceye git çalışmaz.
            Transform flowObject = context.Systems.Find(GameFlowSceneStep.ObjectName);
            GameFlowController flow = flowObject != null ? flowObject.GetComponent<GameFlowController>() : null;

            serialized.FindProperty("_enemySpawner").objectReferenceValue = spawner;
            serialized.FindProperty("_gameFlow").objectReferenceValue = flow;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
