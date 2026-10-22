using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Tools/Last Haven/Export Balance Sheet: oynanış SO'larını okuyup <c>Docs/BALANCE.md</c>'ye tablolar halinde yazar (salt okur, asset
    /// değiştirmez). Bölümler <see cref="IBalanceSection"/> uygulamalarıdır. Dosyadaki işaretlerin dışı (elle yazılan analiz) korunur
    /// (<see cref="BalanceDocument"/>). Zaman damgası yazılmaz: veri değişmedikçe dosya da değişmez.
    /// </summary>
    public static class BalanceSheetExporter
    {
        public const string FileName = "BALANCE.md";

        private const string LogPrefix = "[Last Haven] ";

        /// <summary>Proje kökü/Docs/BALANCE.md.</summary>
        public static string DocumentPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Docs", FileName);

        /// <summary>Tabloları üretip dosyaya yazar. İşaretler bozuksa yazmaz ve false döner.</summary>
        public static bool Export()
        {
            string generated = BuildTables(new BalanceSheetContext(), out int sectionCount);
            string path = DocumentPath;
            string existing = File.Exists(path) ? File.ReadAllText(path) : null;
            if (!BalanceDocument.TryMerge(existing, generated, out string merged))
            {
                Debug.LogError($"{LogPrefix}{path}: '{BalanceDocument.BeginMarker}' / '{BalanceDocument.EndMarker}' işaretleri bozuk "
                               + "(biri eksik, ters sırada veya iki kez). Düzeltip tekrar çalıştır; dosyaya dokunulmadı.");
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, merged, new UTF8Encoding(false));
            Debug.Log($"{LogPrefix}Denge tablosu yazıldı ({sectionCount} bölüm): {path}");
            return true;
        }

        /// <summary>Tüm bölümlerin markdown'ı (işaretsiz).</summary>
        public static string BuildTables(BalanceSheetContext context, out int sectionCount)
        {
            var text = new StringBuilder();
            sectionCount = 0;
            foreach (IBalanceSection section in StepDiscovery.CreateAll<IBalanceSection>(s => s.Order))
            {
                if (sectionCount > 0)
                {
                    text.Append('\n');
                }

                try
                {
                    section.Append(context, text);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    text.Append("\n_Bu bölüm hata verdi: ").Append(MarkdownTable.Escape(exception.Message)).Append("_\n");
                }

                sectionCount++;
            }

            return text.ToString();
        }
    }
}
