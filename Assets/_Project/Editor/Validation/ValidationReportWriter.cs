using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary><see cref="ValidationReport"/>'u konsola (asset'e tıklanabilir) ve markdown metnine yazar.</summary>
    public static class ValidationReportWriter
    {
        private const string LogPrefix = "[Validate] ";

        /// <summary>Her sorun kendi satırında (hata kırmızı, uyarı sarı), sonda özet satırı.</summary>
        public static void LogToConsole(ValidationReport report, string reportPath)
        {
            foreach (ValidationIssue issue in report.Issues)
            {
                string message = $"{LogPrefix}{issue.Section}: {issue.Message}";
                if (issue.Severity == ValidationSeverity.Error)
                {
                    Debug.LogError(message, issue.Context);
                }
                else
                {
                    Debug.LogWarning(message, issue.Context);
                }
            }

            string summary = $"{LogPrefix}{Summary(report)} Rapor: {reportPath}";
            if (report.ErrorCount > 0)
            {
                Debug.LogError(summary);
            }
            else
            {
                Debug.Log(summary);
            }
        }

        /// <summary>Docs/VALIDATION_REPORT.md içeriği: özet, bölüm başına notlar ve sorun tablosu.</summary>
        public static string ToMarkdown(ValidationReport report, DateTime time)
        {
            var text = new StringBuilder();
            text.AppendLine("# Doğrulama Raporu");
            text.AppendLine();
            text.AppendLine("> `Tools/Last Haven/Validate Project` üretir; elle düzenleme, menüyü yeniden çalıştır.");
            text.AppendLine($"> Son çalıştırma: {time.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}");
            text.AppendLine();
            text.AppendLine($"**Sonuç:** {Summary(report)}");

            foreach (string section in report.Sections)
            {
                text.AppendLine();
                text.AppendLine($"## {section}");
                text.AppendLine();
                foreach (string note in report.GetNotes(section))
                {
                    text.AppendLine($"- {note}");
                }

                int errors = report.Count(ValidationSeverity.Error, section);
                int warnings = report.Count(ValidationSeverity.Warning, section);
                if (errors + warnings == 0)
                {
                    text.AppendLine("- Sorun yok.");
                    continue;
                }

                text.AppendLine();
                text.AppendLine("| Önem | Asset | Sorun |");
                text.AppendLine("|---|---|---|");
                // Önce hatalar, sonra uyarılar; her grup bulunma sırasıyla.
                AppendRows(text, report, section, ValidationSeverity.Error);
                AppendRows(text, report, section, ValidationSeverity.Warning);
            }

            return text.ToString();
        }

        private static string Summary(ValidationReport report)
        {
            return report.IsClean
                ? "Temiz — 0 hata, 0 uyarı."
                : $"{report.ErrorCount} hata, {report.WarningCount} uyarı.";
        }

        private static void AppendRows(StringBuilder text, ValidationReport report, string section, ValidationSeverity severity)
        {
            foreach (ValidationIssue issue in report.Issues)
            {
                if (issue.Severity != severity || issue.Section != section)
                {
                    continue;
                }

                string label = severity == ValidationSeverity.Error ? "Hata" : "Uyarı";
                string asset = string.IsNullOrEmpty(issue.AssetPath) ? "—" : $"`{issue.AssetPath}`";
                text.AppendLine($"| {label} | {asset} | {EscapeCell(issue.Message)} |");
            }
        }

        private static string EscapeCell(string value)
        {
            return value.Replace("|", "\\|").Replace("\r", string.Empty).Replace("\n", " ");
        }
    }
}
