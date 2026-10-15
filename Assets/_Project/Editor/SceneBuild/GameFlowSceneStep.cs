using LastHaven.GameFlow;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/GameFlow: oyun akışı (<see cref="GameFlowController"/> + <see cref="GameConfig"/>) ve run istatistikleri
    /// (<see cref="StatsTrackerHost"/>). Dalga servisini ve girdiyi runtime'da ServiceLocator'dan alır; sahnede referans bağlamaz.
    /// </summary>
    public sealed class GameFlowSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "GameFlow";

        public int Order => 60;

        public void Build(SceneBuildContext context)
        {
            GameConfig config = GameFlowGenerator.LoadOrCreateConfig();
            GameObject flowObject = context.CreateObject(ObjectName, context.Systems);
            var controller = flowObject.AddComponent<GameFlowController>();

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            flowObject.AddComponent<StatsTrackerHost>();
        }
    }
}
