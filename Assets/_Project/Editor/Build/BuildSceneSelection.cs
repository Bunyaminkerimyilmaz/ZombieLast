using System.Collections.Generic;

namespace LastHaven.Editor
{
    /// <summary>
    /// Build'e girecek sahneler (saf; testler doğrudan kurar): Build Settings sırasıyla, etkin, dosyası var ve proje klasöründe
    /// (<c>Assets/_Project/Scenes</c>). Şablondan kalan SampleScene gibi klasör dışı sahneler listede etkin olsa da alınmaz.
    /// Açılış sahnesi listenin ilk etkin proje sahnesidir (Build Main Menu Scene ana menüyü başa alır).
    /// </summary>
    public static class BuildSceneSelection
    {
        /// <param name="skipped">Alınmayan satırlar buraya eklenir (log için; boş bırakılabilir).</param>
        public static List<string> Select(IReadOnlyList<BuildSceneEntry> scenes, string projectFolder, List<string> skipped = null)
        {
            var selected = new List<string>();
            for (int i = 0; i < scenes.Count; i++)
            {
                BuildSceneEntry scene = scenes[i];
                if (scene.Enabled && scene.Exists && BuildSettingsValidator.IsInFolder(scene.Path, projectFolder))
                {
                    selected.Add(scene.Path);
                }
                else
                {
                    skipped?.Add(scene.Path);
                }
            }

            return selected;
        }
    }
}
