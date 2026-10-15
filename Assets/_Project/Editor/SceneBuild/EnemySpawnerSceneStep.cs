using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/EnemySpawner: düşmanları havuzdan doğuran <see cref="EnemySpawner"/> (config, veriler ve prefab'lar üreticiden).</summary>
    public sealed class EnemySpawnerSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "EnemySpawner";

        public int Order => 50; // PoolSceneStep'ten (30) sonra.

        public void Build(SceneBuildContext context)
        {
            EnemySpawnerConfig config = EnemiesGenerator.LoadOrCreateAll();
            GameObject spawnerObject = context.CreateObject(ObjectName, context.Systems);
            var spawner = spawnerObject.AddComponent<EnemySpawner>();

            var serialized = new SerializedObject(spawner);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
