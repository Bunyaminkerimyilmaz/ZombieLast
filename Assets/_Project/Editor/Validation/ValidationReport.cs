using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    public enum ValidationSeverity
    {
        Warning,
        Error,
    }

    /// <summary>Doğrulamada bulunan tek bir sorun.</summary>
    public readonly struct ValidationIssue
    {
        public ValidationIssue(ValidationSeverity severity, string section, string message, Object context)
        {
            Severity = severity;
            Section = section;
            Message = message;
            Context = context;
            AssetPath = context != null ? AssetDatabase.GetAssetPath(context) : string.Empty;
        }

        public ValidationSeverity Severity { get; }

        /// <summary>Sorunu bulan adımın adı (<see cref="IValidationStep.Name"/>).</summary>
        public string Section { get; }

        public string Message { get; }

        /// <summary>Konsolda tıklanınca seçilecek asset; null olabilir.</summary>
        public Object Context { get; }

        /// <summary>Context'in asset yolu; asset değilse boş.</summary>
        public string AssetPath { get; }
    }

    /// <summary>
    /// Doğrulama sonuçları: bölüm (adım) başına sorunlar ve "ne kontrol edildi" notları. Adımlar buraya yazar; konsola ve
    /// markdown rapora <see cref="ValidationReportWriter"/> aktarır.
    /// </summary>
    public sealed class ValidationReport
    {
        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();
        private readonly List<string> _sections = new List<string>();
        private readonly Dictionary<string, List<string>> _notes = new Dictionary<string, List<string>>();

        /// <summary>Şu an çalışan adımın adı; eklenen sorun ve notlar bu bölüme girer.</summary>
        public string CurrentSection { get; private set; } = string.Empty;

        public IReadOnlyList<ValidationIssue> Issues => _issues;

        /// <summary>Bölümler, açılma sırasıyla.</summary>
        public IReadOnlyList<string> Sections => _sections;

        public int ErrorCount { get; private set; }

        public int WarningCount { get; private set; }

        public bool IsClean => ErrorCount == 0 && WarningCount == 0;

        /// <summary>Yeni bölüm açar (aynı adla tekrar açılırsa ona devam eder).</summary>
        public void BeginSection(string name)
        {
            CurrentSection = name ?? string.Empty;
            if (!_sections.Contains(CurrentSection))
            {
                _sections.Add(CurrentSection);
            }
        }

        public void Error(string message, Object context = null)
        {
            Add(ValidationSeverity.Error, message, context);
        }

        public void Warning(string message, Object context = null)
        {
            Add(ValidationSeverity.Warning, message, context);
        }

        /// <summary>Bölüme bilgi satırı (ör. "10 NightData kontrol edildi"). Sorun sayılmaz.</summary>
        public void Note(string message)
        {
            if (!_notes.TryGetValue(CurrentSection, out List<string> notes))
            {
                notes = new List<string>();
                _notes.Add(CurrentSection, notes);
            }

            notes.Add(message);
        }

        public IReadOnlyList<string> GetNotes(string section)
        {
            return _notes.TryGetValue(section, out List<string> notes) ? notes : (IReadOnlyList<string>)System.Array.Empty<string>();
        }

        /// <summary>Belirli önem düzeyindeki sorun sayısı; <paramref name="section"/> null ise tüm bölümler.</summary>
        public int Count(ValidationSeverity severity, string section = null)
        {
            int count = 0;
            for (int i = 0; i < _issues.Count; i++)
            {
                if (_issues[i].Severity == severity && (section == null || _issues[i].Section == section))
                {
                    count++;
                }
            }

            return count;
        }

        private void Add(ValidationSeverity severity, string message, Object context)
        {
            _issues.Add(new ValidationIssue(severity, CurrentSection, message, context));
            if (severity == ValidationSeverity.Error)
            {
                ErrorCount++;
            }
            else
            {
                WarningCount++;
            }
        }
    }
}
