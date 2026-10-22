using System;
using System.Collections.Generic;

namespace LastHaven.Editor
{
    /// <summary>Build Settings'teki bir sahne satırı (saf; testler doğrudan kurar).</summary>
    public readonly struct BuildSceneEntry
    {
        public BuildSceneEntry(string path, bool enabled, bool exists)
        {
            Path = path ?? string.Empty;
            Enabled = enabled;
            Exists = exists;
        }

        public string Path { get; }

        public bool Enabled { get; }

        /// <summary>Sahne dosyası projede var mı.</summary>
        public bool Exists { get; }
    }

    /// <summary>
    /// Build Settings sahne kuralları. Validate Project ve EditMode testleri aynı kuralları kullanır.
    /// Player build listedeki ilk etkin sahneyle açılır.
    /// </summary>
    public static class BuildSettingsValidator
    {
        /// <summary>
        /// Hatalar: proje sahnesi listede yok; listedeki sahne dosyası yok; etkin sahne yok.
        /// Uyarılar: proje sahnesi listede kapalı; ilk etkin sahne proje klasörü dışında (ör. şablondaki SampleScene).
        /// </summary>
        /// <param name="projectScenes">Proje klasöründeki sahnelerin yolları.</param>
        /// <param name="projectFolder">Proje sahnelerinin klasörü (ör. <c>Assets/_Project/Scenes</c>).</param>
        public static void Validate(IReadOnlyList<BuildSceneEntry> buildScenes, IReadOnlyList<string> projectScenes,
            string projectFolder, ValidationReport report)
        {
            Validate(buildScenes, projectScenes, projectFolder, null, report);
        }

        /// <summary>
        /// Yukarıdaki kurallar + 14.3: <paramref name="startScene"/> (ana menü) projede varsa ilk etkin sahne o olmalı (uyarı).
        /// </summary>
        public static void Validate(IReadOnlyList<BuildSceneEntry> buildScenes, IReadOnlyList<string> projectScenes,
            string projectFolder, string startScene, ValidationReport report)
        {
            foreach (string scene in projectScenes)
            {
                int index = IndexOf(buildScenes, scene);
                if (index < 0)
                {
                    report.Error($"{scene} Build Settings'te yok; build'e girmez. Tools/Last Haven/Build Game Scene ekler " +
                                 "(diğer sahneler için File → Build Profiles).");
                }
                else if (!buildScenes[index].Enabled)
                {
                    report.Warning($"{scene} Build Settings'te kapalı; build'e girmez.");
                }
            }

            BuildSceneEntry? first = null;
            foreach (BuildSceneEntry entry in buildScenes)
            {
                if (!entry.Exists)
                {
                    report.Error($"Build Settings'teki '{entry.Path}' dosyası yok (silinmiş veya taşınmış); listeden çıkar.");
                }
                else if (entry.Enabled && first == null)
                {
                    first = entry;
                }
            }

            if (first == null)
            {
                report.Error("Build Settings'te etkin sahne yok; player build açılamaz.");
            }
            else if (!IsInFolder(first.Value.Path, projectFolder))
            {
                report.Warning($"Player build '{first.Value.Path}' ile açılır (ilk etkin sahne {projectFolder} dışında). " +
                               "Tools/Last Haven/Build Game Scene oyun sahnesini öne alır.");
            }
            else if (startScene != null && Contains(projectScenes, startScene)
                     && !string.Equals(first.Value.Path, startScene, StringComparison.Ordinal))
            {
                report.Warning($"Player build '{first.Value.Path}' ile açılır; açılış sahnesi {startScene} olmalı. " +
                               "Tools/Last Haven/Build Main Menu Scene onu başa alır.");
            }
        }

        private static bool Contains(IReadOnlyList<string> paths, string path)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (string.Equals(paths[i], path, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary><paramref name="path"/> klasörün (veya alt klasörünün) içinde mi; ordinal.</summary>
        public static bool IsInFolder(string path, string folder)
        {
            return path != null && path.StartsWith(folder.TrimEnd('/') + "/", StringComparison.Ordinal);
        }

        private static int IndexOf(IReadOnlyList<BuildSceneEntry> scenes, string path)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (string.Equals(scenes[i].Path, path, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
