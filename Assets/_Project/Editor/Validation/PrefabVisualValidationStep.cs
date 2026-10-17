using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Validate Project bölümü: <c>Prefabs/</c> altındaki varlık prefab'larında Visual alt objesi (<see cref="PrefabVisualValidator"/>).
    /// Efekt prefab'ları (<c>Prefabs/Feedback/</c>: iz, kıvılcım, namlu ışığı, moloz) varlık değildir; kontrol edilmez.
    /// </summary>
    public sealed class PrefabVisualValidationStep : IValidationStep
    {
        public const string PrefabsFolder = "Assets/_Project/Prefabs";

        /// <summary>İki katman kuralı dışındaki klasörler (sonunda '/').</summary>
        public static readonly IReadOnlyList<string> ExcludedFolders = new[] { PrefabsFolder + "/Feedback/" };

        public int Order => 200;

        public string Name => "Prefab görsel katmanı (Visual)";

        public void Validate(ValidationReport report)
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsFolder }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            int checkedCount = 0;
            foreach (string path in paths)
            {
                if (IsExcluded(path))
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    PrefabVisualValidator.ValidatePrefab(prefab, report);
                    checkedCount++;
                }
            }

            report.Note($"{PrefabsFolder} altında {checkedCount} varlık prefab'ı kontrol edildi (hariç: {string.Join(", ", ExcludedFolders)}).");
        }

        public static bool IsExcluded(string path)
        {
            for (int i = 0; i < ExcludedFolders.Count; i++)
            {
                if (path.StartsWith(ExcludedFolders[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
