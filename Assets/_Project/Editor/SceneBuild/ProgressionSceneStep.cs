using LastHaven.Progression;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/Progression: oyuncunun XP ve seviye servisi (<see cref="ExperienceService"/>) ve perk servisi
    /// (<see cref="PerkService"/>, 13.2), ikisi de <see cref="ProgressionConfig"/> ile; perk servisi ayrıca <see cref="PerkCatalog"/> ile.
    /// </summary>
    public sealed class ProgressionSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Progression";

        public int Order => 78;

        public void Build(SceneBuildContext context)
        {
            ProgressionConfig config = ProgressionGenerator.LoadOrCreateConfig();
            PerkCatalog catalog = ProgressionGenerator.LoadOrCreateCatalog();
            GameObject progressionObject = context.CreateObject(ObjectName, context.Systems);

            var experience = new SerializedObject(progressionObject.AddComponent<ExperienceService>());
            experience.FindProperty("_config").objectReferenceValue = config;
            experience.ApplyModifiedPropertiesWithoutUndo();

            var perks = new SerializedObject(progressionObject.AddComponent<PerkService>());
            perks.FindProperty("_config").objectReferenceValue = config;
            perks.FindProperty("_catalog").objectReferenceValue = catalog;
            perks.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
