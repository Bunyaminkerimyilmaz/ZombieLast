using LastHaven.Feedback;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/Feedback: olayları efektlere bağlayan <see cref="FeedbackDirector"/> (config üreticiden).</summary>
    public sealed class FeedbackSceneStep : ISceneBuildStep
    {
        public const string FeedbackObjectName = "Feedback";

        public int Order => 40; // PoolSceneStep'ten (30) sonra.

        public void Build(SceneBuildContext context)
        {
            CombatFeedbackConfig config = FeedbackGenerator.LoadOrCreateAll();
            GameObject feedback = context.CreateObject(FeedbackObjectName, context.Systems);
            var director = feedback.AddComponent<FeedbackDirector>();

            var serialized = new SerializedObject(director);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
