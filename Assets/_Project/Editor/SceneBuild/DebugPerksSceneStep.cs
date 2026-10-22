using LastHaven.DebugTools;
using LastHaven.Progression;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DebugPerks: F10 seviye atla (seviye kartları, 13.3), Shift+F10 rastgele perk ver (13.2) (<see cref="DebugPerkControls"/>;
    /// sadece editör ve development build). F10'un <see cref="ExperienceService"/>'i [Systems]/Progression'dan bağlanır; perk servisini
    /// runtime'da <c>IPerkService</c>'ten alır.
    /// </summary>
    public sealed class DebugPerksSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DebugPerks";

        public int Order => 730; // ProgressionSceneStep (78) kurulmuş olmalı.

        public void Build(SceneBuildContext context)
        {
            var controls = context.CreateObject(ObjectName, context.Systems).AddComponent<DebugPerkControls>();
            Transform progression = context.Systems.Find(ProgressionSceneStep.ObjectName);
            ExperienceService experience = progression != null ? progression.GetComponent<ExperienceService>() : null;
            if (experience == null)
            {
                Debug.LogWarning("[Last Haven] DebugPerks: [Systems]/Progression yok; F10 (seviye atla) çalışmayacak.");
                return;
            }

            var serialized = new SerializedObject(controls);
            serialized.FindProperty("_experience").objectReferenceValue = experience;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
