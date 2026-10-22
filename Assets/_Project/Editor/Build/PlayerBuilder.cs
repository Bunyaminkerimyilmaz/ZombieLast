using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHaven.Editor
{
    /// <summary>
    /// Tools/Last Haven/Build/Windows ve macOS (15.3): Player Settings'i config'ten uygular, Validate Project'te hata varsa durur, Build
    /// Settings'teki proje sahneleriyle (<see cref="BuildSceneSelection"/>) <c>Builds/&lt;platform&gt;/</c> klasörüne sürüm build'i
    /// (development değil) alır. Sürüm build'inde debug paneli, kısayollar ve test kuklaları kapalıdır.
    /// </summary>
    public static class PlayerBuilder
    {
        private const string LogPrefix = "[Last Haven] Build:";
        private const float BytesPerMegabyte = 1024f * 1024f;

        /// <summary>Build'i alır; başarılıysa true.</summary>
        public static bool Build(PlayerBuildPlatform platform)
        {
            string platformName = PlayerBuildTargets.FolderName(platform);
            BuildTarget target = PlayerBuildTargets.Target(platform);
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
            {
                Debug.LogError($"{LogPrefix} {platformName} build modülü bu Unity kurulumunda yok. Unity Hub → Installs → " +
                               $"{Application.unityVersion} → Add modules → '{PlayerBuildTargets.ModuleName(platform)}' kur, sonra tekrar dene.");
                return false;
            }

            if (!PlayerSettingsSetup.Run(out PlayerBuildConfig config))
            {
                Debug.LogError($"{LogPrefix} {platformName} build'i alınmadı (Player Settings config'i hatalı).");
                return false;
            }

            ValidationReport validation = ProjectValidator.Run();
            if (validation.ErrorCount > 0)
            {
                Debug.LogError($"{LogPrefix} Validate Project {validation.ErrorCount} hata buldu; {platformName} build'i alınmadı. " +
                               "Tools/Last Haven/Validate Project → Docs/VALIDATION_REPORT.md.");
                return false;
            }

            List<string> scenes = SelectScenes();
            if (scenes.Count == 0)
            {
                Debug.LogError($"{LogPrefix} Build Settings'te {GameSceneBuilder.ScenesFolder} altında etkin sahne yok; Tools/Last Haven/Rebuild All çalıştır.");
                return false;
            }

            WarnIfScenesDirty();
            string relativePath = PlayerBuildTargets.OutputPath(config.OutputFolder, platform, config.ExecutableName);
            string location = Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath).FullName, relativePath));
            Debug.Log($"{LogPrefix} {platformName} build'i başlıyor → {relativePath} (sahneler: {string.Join(", ", scenes)}).");

            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = location,
                target = target,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };

            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"{LogPrefix} {platformName} build'i başarısız ({summary.result}, {summary.totalErrors} hata). " +
                               "Konsoldaki build hatalarına bak.");
                return false;
            }

            Debug.Log($"{LogPrefix} {platformName} build'i hazır: {location} ({summary.totalSize / BytesPerMegabyte:0.#} MB, " +
                      $"{summary.totalTime.TotalSeconds:0} sn, {summary.totalWarnings} uyarı). Sürüm build'i: debug paneli ve test kuklaları kapalı.");
            return true;
        }

        private static List<string> SelectScenes()
        {
            var entries = new List<BuildSceneEntry>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                bool exists = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) != null;
                entries.Add(new BuildSceneEntry(scene.path, scene.enabled, exists));
            }

            var skipped = new List<string>();
            List<string> selected = BuildSceneSelection.Select(entries, GameSceneBuilder.ScenesFolder, skipped);
            foreach (string path in skipped)
            {
                Debug.Log($"{LogPrefix} '{path}' build'e alınmadı (kapalı, dosyası yok veya {GameSceneBuilder.ScenesFolder} dışında).");
            }

            return selected;
        }

        private static void WarnIfScenesDirty()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    Debug.LogWarning($"{LogPrefix} '{scene.path}' kaydedilmemiş; build diskteki hâlini kullanır. Kalıcı olacaksa üreticiye taşı.");
                }
            }
        }
    }
}
