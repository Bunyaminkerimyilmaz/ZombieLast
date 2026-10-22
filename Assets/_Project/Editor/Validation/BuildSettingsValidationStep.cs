using System;
using System.Collections.Generic;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>Validate Project bölümü: proje sahneleri ve Build Settings listesi (<see cref="BuildSettingsValidator"/>).</summary>
    public sealed class BuildSettingsValidationStep : IValidationStep
    {
        public int Order => 300;

        public string Name => "Build Settings sahneleri";

        public void Validate(ValidationReport report)
        {
            var projectScenes = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { GameSceneBuilder.ScenesFolder }))
            {
                projectScenes.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            var entries = new List<BuildSceneEntry>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                bool exists = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) != null;
                entries.Add(new BuildSceneEntry(scene.path, scene.enabled, exists));
            }

            BuildSettingsValidator.Validate(entries, new List<string>(projectScenes), GameSceneBuilder.ScenesFolder,
                MainMenuSceneBuilder.ScenePath, report);
            report.Note($"{GameSceneBuilder.ScenesFolder} altında {projectScenes.Count} sahne, Build Settings'te {entries.Count} satır kontrol edildi.");
        }
    }
}
