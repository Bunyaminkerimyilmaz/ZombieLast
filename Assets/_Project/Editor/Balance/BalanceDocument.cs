using System;

namespace LastHaven.Editor
{
    /// <summary>
    /// <c>Docs/BALANCE.md</c>'nin yapısı (saf): üretilen tablolar iki işaret arasında durur ve her dışa aktarımda yeniden yazılır;
    /// işaretlerin dışı (elle yazılan analiz ve öneriler) korunur. Dosya yoksa başlık + tablolar + boş analiz bölümüyle oluşur;
    /// işaret yoksa tablolar başa eklenir, eski metin altta kalır. İşaretler bozuksa (biri eksik, sıra ters) birleştirme yapılmaz.
    /// </summary>
    public static class BalanceDocument
    {
        public const string BeginMarker = "<!-- balance:auto:begin -->";
        public const string EndMarker = "<!-- balance:auto:end -->";

        public const string Header =
            "# Denge Tablosu\n\n"
            + "> `Tools/Last Haven/Export Balance Sheet` üretir. İşaretler arasındaki tablolar her dışa aktarımda veriden (ScriptableObject'ler) "
            + "yeniden yazılır; elle düzenleme, değeri SO'da değiştirip menüyü çalıştır. İşaretlerin dışındaki metin (analiz) korunur.\n";

        public const string AnalysisPlaceholder =
            "## Analiz ve öneriler\n\n_Henüz yazılmadı. Tabloları okuyup buraya yaz; dışa aktarım bu bölüme dokunmaz._\n";

        /// <summary>
        /// Mevcut metinle yeni tablo bloğunu birleştirir. <paramref name="existing"/> null/boşsa yeni belge kurar. İşaretler bozuksa
        /// false döner ve <paramref name="merged"/> null olur.
        /// </summary>
        public static bool TryMerge(string existing, string generated, out string merged)
        {
            string block = BeginMarker + "\n" + Normalize(generated).Trim('\n') + "\n" + EndMarker;
            if (string.IsNullOrWhiteSpace(existing))
            {
                merged = Header + "\n" + block + "\n\n" + AnalysisPlaceholder;
                return true;
            }

            string text = Normalize(existing);
            int begin = text.IndexOf(BeginMarker, StringComparison.Ordinal);
            int end = text.IndexOf(EndMarker, StringComparison.Ordinal);
            if (begin < 0 && end < 0)
            {
                merged = Header + "\n" + block + "\n\n" + text;
                return true;
            }

            if (begin < 0 || end < begin || text.IndexOf(BeginMarker, begin + 1, StringComparison.Ordinal) >= 0)
            {
                merged = null;
                return false;
            }

            merged = text.Substring(0, begin) + block + text.Substring(end + EndMarker.Length);
            return true;
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}
