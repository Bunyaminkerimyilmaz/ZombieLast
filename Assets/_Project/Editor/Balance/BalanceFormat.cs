using System.Globalization;
using System.Text;

namespace LastHaven.Editor
{
    /// <summary>Denge tablosunda sayı biçimleri (kültürden bağımsız: ondalık ayırıcı nokta) ve ortak satırlar.</summary>
    public static class BalanceFormat
    {
        public const string None = "—";

        /// <summary>En fazla iki ondalık: 2.5, 0.05, 120.</summary>
        public static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>Tam sayıya yuvarlı.</summary>
        public static string Round(double value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }

        public static string Int(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Kesir → "%40" (en fazla bir ondalık).</summary>
        public static string Percent(double fraction)
        {
            return "%" + (fraction * 100d).ToString("0.#", CultureInfo.InvariantCulture);
        }

        /// <summary>Bölüm başlığı ve ardından boş satır.</summary>
        public static void Heading(StringBuilder text, string title)
        {
            text.Append("## ").Append(title).Append('\n').Append('\n');
        }

        /// <summary>Asset bulunamadığında bölümün yazdığı satır.</summary>
        public static void Missing(StringBuilder text, string what)
        {
            text.Append("_").Append(what).Append(" bulunamadı (Tools/Last Haven/Generate Default Data)._\n");
        }
    }
}
