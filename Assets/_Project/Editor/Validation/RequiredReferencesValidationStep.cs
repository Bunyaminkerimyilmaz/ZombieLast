using System;
using System.Collections.Generic;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Validate Project bölümü: <c>Data/</c> altındaki projenin tüm ScriptableObject'lerinde boş zorunlu ve kayıp referanslar
    /// (<see cref="RequiredReferenceValidator"/>). Gece verisi kendi bölümünde (<see cref="WavesValidationStep"/>).
    /// </summary>
    public sealed class RequiredReferencesValidationStep : IValidationStep
    {
        public const string DataFolder = "Assets/_Project/Data";

        // Kendi doğrulama bölümü olan tipler; aynı sorun iki kez raporlanmasın.
        private static readonly HashSet<Type> OwnSectionTypes = new HashSet<Type> { typeof(NightData), typeof(WaveConfig) };

        public int Order => 100;

        public string Name => "Zorunlu referanslar (Data)";

        public void Validate(ValidationReport report)
        {
            int checkedCount = 0;
            foreach (string path in FindAssetPaths())
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type == null)
                {
                    report.Error($"{path}: yüklenemiyor (script'i silinmiş veya derlenmemiş olabilir).");
                    continue;
                }

                if (!typeof(ScriptableObject).IsAssignableFrom(type) || !RequiredReferenceValidator.IsProjectType(type)
                    || OwnSectionTypes.Contains(type))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadMainAssetAtPath(path) as ScriptableObject;
                if (asset != null)
                {
                    RequiredReferenceValidator.ValidateAsset(asset, report);
                    checkedCount++;
                }
            }

            report.Note($"{DataFolder} altında {checkedCount} ScriptableObject kontrol edildi (gece verisi kendi bölümünde). " +
                        $"Boş bırakılabilir alanlar: Tooltip'inde \"Opsiyonel\" geçenler, ses ve görsel (AudioClip, Sprite, Texture).");
        }

        private static List<string> FindAssetPaths()
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { DataFolder }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            return new List<string>(paths);
        }
    }
}
