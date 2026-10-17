using System.Collections.Generic;
using LastHaven.Editor;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Validate Project'in 11.3 bölümleri projedeki gerçek asset'ler üzerinde: zorunlu referanslar, Visual, Build Settings,
    /// düşman bağlantıları hatasız olmalı (uyarılar bilinen maddeler olabilir). Asset'ler yoksa Rebuild All çalıştır.
    /// </summary>
    public class ProjectValidationAssetTests
    {
        private static IEnumerable<IValidationStep> Steps
        {
            get
            {
                yield return new RequiredReferencesValidationStep();
                yield return new PrefabVisualValidationStep();
                yield return new BuildSettingsValidationStep();
                yield return new EnemyLinksValidationStep();
            }
        }

        [TestCaseSource(nameof(Steps))]
        public void Step_HasNoErrors(IValidationStep step)
        {
            var report = new ValidationReport();
            report.BeginSection(step.Name);

            step.Validate(report);

            Assert.AreEqual(0, report.ErrorCount, Describe(report));
        }

        [Test]
        public void AllSteps_AreDiscoveredInOrder()
        {
            List<IValidationStep> steps = StepDiscovery.CreateAll<IValidationStep>(step => step.Order);
            var names = new List<string>();
            foreach (IValidationStep step in steps)
            {
                names.Add(step.GetType().Name);
            }

            CollectionAssert.IsSubsetOf(
                new[] { nameof(RequiredReferencesValidationStep), nameof(PrefabVisualValidationStep),
                        nameof(BuildSettingsValidationStep), nameof(EnemyLinksValidationStep), nameof(WavesValidationStep) },
                names);
            Assert.Less(names.IndexOf(nameof(RequiredReferencesValidationStep)), names.IndexOf(nameof(WavesValidationStep)));
        }

        private static string Describe(ValidationReport report)
        {
            var lines = new List<string>();
            foreach (ValidationIssue issue in report.Issues)
            {
                lines.Add($"{issue.Severity}: {issue.Message} ({issue.AssetPath})");
            }

            return string.Join("\n", lines);
        }
    }
}
