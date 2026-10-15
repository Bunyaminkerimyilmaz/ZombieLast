using System.Collections.Generic;
using System.Text;
using LastHaven.Editor;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEditor;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Validate Project'in gece doğrulaması, projedeki gerçek asset'ler üzerinde: her NightData ayrı test, ayrıca WaveConfig.
    /// Asset'ler yoksa (Generate Default Data çalışmamış) testler sonuçsuz (Inconclusive) kalır.
    /// </summary>
    public class NightDataAssetTests
    {
        private const string NoAssets = "(NightData yok)";

        private static IEnumerable<string> NightPaths
        {
            get
            {
                List<string> paths = NightDataValidator.FindAllNightPaths();
                return paths.Count > 0 ? paths : new List<string> { NoAssets };
            }
        }

        [TestCaseSource(nameof(NightPaths))]
        public void NightData_PassesValidation(string path)
        {
            if (path == NoAssets)
            {
                Assert.Inconclusive("Projede NightData yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            var night = AssetDatabase.LoadAssetAtPath<NightData>(path);
            Assert.IsNotNull(night, path);
            var report = new ValidationReport();

            NightDataValidator.ValidateNight(night, report);

            Assert.AreEqual(0, report.ErrorCount, Describe(report));
        }

        [Test]
        public void WaveConfig_PassesValidation_WithTenNightsInOrder()
        {
            var config = AssetDatabase.LoadAssetAtPath<WaveConfig>(WavesGenerator.ConfigPath);
            if (config == null)
            {
                Assert.Inconclusive($"{WavesGenerator.ConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            var report = new ValidationReport();

            NightDataValidator.ValidateConfig(config, report);

            Assert.IsTrue(report.IsClean, Describe(report));
            Assert.AreEqual(NightDefinitions.All.Count, config.Nights.Count, "GDD §8: 10 gece.");
        }

        private static string Describe(ValidationReport report)
        {
            var text = new StringBuilder();
            foreach (ValidationIssue issue in report.Issues)
            {
                text.AppendLine($"{issue.Severity}: {issue.Message}");
            }

            return text.ToString();
        }
    }
}
