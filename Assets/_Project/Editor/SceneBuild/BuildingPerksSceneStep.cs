using LastHaven.Building;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/BuildingPerks: Dikenli Duvar (<see cref="BarricadeThorns"/>, <see cref="BuildingPerkConfig"/> ile; 13.2).</summary>
    public sealed class BuildingPerksSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "BuildingPerks";

        public int Order => 85; // BuildingSceneStep'ten (80) sonra.

        public void Build(SceneBuildContext context)
        {
            BuildingPerkConfig config = BuildingPerksGenerator.LoadOrCreateConfig();
            GameObject perksObject = context.CreateObject(ObjectName, context.Systems);
            var thorns = new SerializedObject(perksObject.AddComponent<BarricadeThorns>());
            thorns.FindProperty("_config").objectReferenceValue = config;
            thorns.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
