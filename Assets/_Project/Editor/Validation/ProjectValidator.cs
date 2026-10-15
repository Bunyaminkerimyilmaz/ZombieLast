using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Tools/Last Haven/Validate Project: tüm <see cref="IValidationStep"/>'leri sırayla çalıştırır, sonuçları konsola ve
    /// <c>Docs/VALIDATION_REPORT.md</c>'ye yazar. Bir adım exception atarsa hata olarak rapora girer, diğerleri devam eder.
    /// </summary>
    public static class ProjectValidator
    {
        public const string ReportFileName = "VALIDATION_REPORT.md";

        /// <summary>Raporun tam yolu: proje kökü/Docs/VALIDATION_REPORT.md.</summary>
        public static string ReportPath =>
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Docs", ReportFileName);

        /// <summary>Adımları çalıştırır; log basmaz, dosya yazmaz.</summary>
        public static ValidationReport Run()
        {
            var report = new ValidationReport();
            List<IValidationStep> steps = StepDiscovery.CreateAll<IValidationStep>(step => step.Order);
            foreach (IValidationStep step in steps)
            {
                report.BeginSection(step.Name);
                try
                {
                    step.Validate(report);
                }
                catch (Exception exception)
                {
                    report.Error($"Doğrulama adımı çöktü: {exception.GetType().Name}: {exception.Message}");
                    Debug.LogException(exception);
                }
            }

            return report;
        }

        /// <summary>Çalıştırır, konsola ve rapor dosyasına yazar.</summary>
        public static ValidationReport RunAndPublish()
        {
            ValidationReport report = Run();
            string path = ReportPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, ValidationReportWriter.ToMarkdown(report, DateTime.Now), new UTF8Encoding(false));
            ValidationReportWriter.LogToConsole(report, path);
            return report;
        }
    }
}
