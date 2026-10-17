using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DayWanderers (11.2, GDD §9): <see cref="DayWanderers"/> + <see cref="DayWanderersConfig"/>; gece gezginleri geceye
    /// katmak için [Systems]/WaveSpawner bağlanır (Waves adımından sonra).
    /// </summary>
    public sealed class DayWanderersSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DayWanderers";

        public int Order => 310;

        public void Build(SceneBuildContext context)
        {
            GameObject wanderersObject = context.CreateObject(ObjectName, context.Systems);
            var wanderers = wanderersObject.AddComponent<DayWanderers>();

            var waves = context.Systems.GetComponentInChildren<WaveSpawner>(true);
            if (waves == null)
            {
                Debug.LogWarning("[Last Haven] DayWanderers: sahnede WaveSpawner yok; gezginler gecenin sayacına katılmayacak.");
            }

            var serialized = new SerializedObject(wanderers);
            serialized.FindProperty("_config").objectReferenceValue = DayWanderersGenerator.LoadOrCreateConfig();
            serialized.FindProperty("_waves").objectReferenceValue = waves;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
