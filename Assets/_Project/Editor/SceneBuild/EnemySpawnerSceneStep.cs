using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/EnemySpawner: düşmanları havuzdan doğuran <see cref="EnemySpawner"/> (config, veriler ve prefab'lar üreticiden) ve
    /// Zincirleme Son perk'i (<see cref="ChainExplosionPerk"/>, <see cref="EnemyPerkConfig"/> ile; 13.2).
    /// </summary>
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

            var chain = new SerializedObject(spawnerObject.AddComponent<ChainExplosionPerk>());
            chain.FindProperty("_config").objectReferenceValue = EnemyPerksGenerator.LoadOrCreateConfig();
            chain.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
