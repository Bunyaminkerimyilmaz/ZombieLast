using System;
using System.Collections.Generic;
using System.Text;

namespace LastHaven.Editor
{
    /// <summary>
    /// Markdown tablosu (saf): başlıklar ve satırlar; hücrelerdeki "|" kaçırılır, satır sonları boşluğa döner. Sütun sayısı başlıkla aynı
    /// olmalı. Denge tablosu (<see cref="BalanceSheetExporter"/>) kullanır.
    /// </summary>
    public sealed class MarkdownTable
    {
        private readonly string[] _headers;
        private readonly List<string[]> _rows = new List<string[]>();

        public MarkdownTable(params string[] headers)
        {
            if (headers == null || headers.Length == 0)
            {
                throw new ArgumentException("En az bir sütun olmalı.", nameof(headers));
            }

            _headers = headers;
        }

        public int RowCount => _rows.Count;

        /// <summary>Satır ekler. Hücre sayısı sütun sayısından farklıysa <see cref="ArgumentException"/>.</summary>
        public MarkdownTable AddRow(params string[] cells)
        {
            if (cells == null || cells.Length != _headers.Length)
            {
                throw new ArgumentException($"{_headers.Length} hücre bekleniyordu, {cells?.Length ?? 0} geldi.", nameof(cells));
            }

            _rows.Add(cells);
            return this;
        }

        /// <summary>Başlık, ayraç ve satırları ekler (her satır kendi satırında, sonunda boş satır yok).</summary>
        public void AppendTo(StringBuilder text)
        {
            AppendLine(text, _headers);
            text.Append('|');
            for (int i = 0; i < _headers.Length; i++)
            {
                text.Append("---|");
            }

            text.AppendLine();
            for (int i = 0; i < _rows.Count; i++)
            {
                AppendLine(text, _rows[i]);
            }
        }

        /// <summary>Hücre metni: "|" kaçırılır, satır sonları boşluk olur, null boş.</summary>
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("|", "\\|").Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
        }

        private static void AppendLine(StringBuilder text, string[] cells)
        {
            text.Append('|');
            for (int i = 0; i < cells.Length; i++)
            {
                text.Append(' ').Append(Escape(cells[i])).Append(" |");
            }

            text.AppendLine();
        }
    }
}
