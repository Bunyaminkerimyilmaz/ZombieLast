using System;
using System.Collections.Generic;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>
    /// Build Settings sahne listesinin düzenleme kuralı (saf liste işlemi; testler doğrudan kurar). Player build ilk etkin sahneyle
    /// açıldığı için proje sahnesi, proje klasörü dışındaki sahnelerin (ör. şablondaki SampleScene) önünde durur.
    /// </summary>
    public static class BuildSceneList
    {
        [Flags]
        public enum Change
        {
            None = 0,
            Added = 1,
            Enabled = 2,
            MovedForward = 4,
        }

        /// <summary>
        /// Sahneyi listeye ekler (yoksa) ve etkinleştirir; proje klasörü dışındaki ilk sahneden sonra duruyorsa onun önüne alır.
        /// Proje sahneleri arasındaki sıra ve diğer satırlar korunur.
        /// </summary>
        public static Change Ensure(List<EditorBuildSettingsScene> scenes, string scenePath, string projectFolder)
        {
            var change = Change.None;
            int index = scenes.FindIndex(scene => string.Equals(scene.path, scenePath, StringComparison.Ordinal));
            EditorBuildSettingsScene entry;
            if (index < 0)
            {
                entry = new EditorBuildSettingsScene(scenePath, true);
                index = scenes.Count;
                scenes.Add(entry);
                change |= Change.Added;
            }
            else
            {
                entry = scenes[index];
                if (!entry.enabled)
                {
                    entry.enabled = true;
                    change |= Change.Enabled;
                }
            }

            int firstExternal = scenes.FindIndex(scene => !BuildSettingsValidator.IsInFolder(scene.path, projectFolder));
            if (firstExternal >= 0 && firstExternal < index)
            {
                scenes.RemoveAt(index);
                scenes.Insert(firstExternal, entry);
                if ((change & Change.Added) == 0)
                {
                    change |= Change.MovedForward;
                }
            }

            return change;
        }

        /// <summary>
        /// Sahneyi listenin başına alır (yoksa ekler) ve etkinleştirir: player build bununla açılır (14.3: ana menü). Diğer satırların
        /// sırası korunur.
        /// </summary>
        public static Change EnsureFirst(List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            var change = Change.None;
            int index = scenes.FindIndex(scene => string.Equals(scene.path, scenePath, StringComparison.Ordinal));
            EditorBuildSettingsScene entry;
            if (index < 0)
            {
                entry = new EditorBuildSettingsScene(scenePath, true);
                change |= Change.Added;
            }
            else
            {
                entry = scenes[index];
                scenes.RemoveAt(index);
                if (!entry.enabled)
                {
                    entry.enabled = true;
                    change |= Change.Enabled;
                }

                if (index > 0)
                {
                    change |= Change.MovedForward;
                }
            }

            scenes.Insert(0, entry);
            return change;
        }
    }
}
