using System.Collections.Generic;
using LastHaven.Waves;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>Validate Project bölümü: WaveConfig ve projedeki tüm NightData'lar (<see cref="NightDataValidator"/>).</summary>
    public sealed class WavesValidationStep : IValidationStep
    {
        public int Order => 900;

        public string Name => "Gece verisi (NightData)";

        public void Validate(ValidationReport report)
        {
            var config = AssetDatabase.LoadAssetAtPath<WaveConfig>(WavesGenerator.ConfigPath);
            if (config == null)
            {
                report.Error($"{WavesGenerator.ConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }
            else
            {
                NightDataValidator.ValidateConfig(config, report);
            }

            List<string> paths = NightDataValidator.FindAllNightPaths();
            foreach (string path in paths)
            {
                var night = AssetDatabase.LoadAssetAtPath<NightData>(path);
                if (night != null)
                {
                    NightDataValidator.ValidateNight(night, report);
                }
            }

            string configNote = config != null ? $"{WavesGenerator.ConfigPath} ({config.Nights.Count} gece)" : "WaveConfig yok";
            report.Note($"{paths.Count} NightData ve {configNote} kontrol edildi.");
        }
    }
}
