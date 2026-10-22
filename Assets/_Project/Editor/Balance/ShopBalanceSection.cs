using System.Text;
using LastHaven.Economy;

namespace LastHaven.Editor
{
    /// <summary>Denge tablosu: market öğeleri (GDD §6, §10) katalog sırasıyla; seviye fiyatları ve hepsinin toplamı.</summary>
    public sealed class ShopBalanceSection : IBalanceSection
    {
        public int Order => 600;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Market");
            if (context.Shop == null)
            {
                BalanceFormat.Missing(text, "ShopCatalog");
                return;
            }

            var table = new MarkdownTable("Öğe", "Sekme", "Etki", "Hedef", "Değer", "Fiyatlar", "Tüm seviyeler", "Ön koşul");
            foreach (ShopItemData item in context.Shop.Items)
            {
                if (item == null)
                {
                    continue;
                }

                int total = 0;
                var prices = new StringBuilder();
                for (int i = 0; i < item.Prices.Count; i++)
                {
                    if (i > 0)
                    {
                        prices.Append(" / ");
                    }

                    prices.Append(BalanceFormat.Int(item.Prices[i]));
                    total += item.Prices[i];
                }

                table.AddRow(
                    item.DisplayName,
                    item.Category.ToString(),
                    item.Effect.ToString(),
                    string.IsNullOrEmpty(item.TargetId) ? BalanceFormat.None : "`" + item.TargetId + "`",
                    BalanceFormat.Number(item.Value),
                    prices.Length > 0 ? prices.ToString() : BalanceFormat.None,
                    item.IsRepeatable ? "tekrar alınır" : BalanceFormat.Int(total),
                    string.IsNullOrEmpty(item.RequiredItemId) ? BalanceFormat.None : "`" + item.RequiredItemId + "`");
            }

            table.AppendTo(text);
        }
    }
}
