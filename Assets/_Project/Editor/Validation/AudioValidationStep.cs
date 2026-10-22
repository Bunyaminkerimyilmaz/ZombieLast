using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>
    /// Validate Project bölümü (14.2): <c>Data/</c> altındaki tüm <see cref="SoundData"/>'lar — klipsiz sesler uyarı olarak listelenir
    /// (<see cref="SoundDataValidator"/>). Ses referanslarının boşluğu hata değildir (CLAUDE.md: ses opsiyonel).
    /// </summary>
    public sealed class AudioValidationStep : IValidationStep
    {
        public int Order => 1000;

        public string Name => "Ses verisi (SoundData)";

        public void Validate(ValidationReport report)
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(SoundData)}", new[] { RequiredReferencesValidationStep.DataFolder }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            int silent = 0;
            int checkedCount = 0;
            foreach (string path in paths)
            {
                var sound = AssetDatabase.LoadAssetAtPath<SoundData>(path);
                if (sound == null)
                {
                    continue;
                }

                checkedCount++;
                silent += SoundDataValidator.Validate(sound, report) ? 1 : 0;
            }

            report.Note($"{RequiredReferencesValidationStep.DataFolder} altında {checkedCount} SoundData kontrol edildi; {silent} tanesinde " +
                        "klip yok (uyarı: o ses sessiz kalır, oyun bozulmaz).");
        }
    }
}
