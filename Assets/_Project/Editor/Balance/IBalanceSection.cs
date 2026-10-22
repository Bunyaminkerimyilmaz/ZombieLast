using System.Text;

namespace LastHaven.Editor
{
    /// <summary>
    /// Denge tablosunun bir bölümü (<c>Docs/BALANCE.md</c>). <see cref="BalanceSheetExporter"/> tüm uygulamaları TypeCache ile bulur ve
    /// <see cref="Order"/>'a göre yazar; yeni bölüm eklemek merkezi dosyaya dokunmayı gerektirmez (araya ekleme için 100'lük boşluk).
    /// </summary>
    public interface IBalanceSection
    {
        int Order { get; }

        /// <summary>"## Başlık" ile başlayan markdown'ı ekler.</summary>
        void Append(BalanceSheetContext context, StringBuilder text);
    }
}
