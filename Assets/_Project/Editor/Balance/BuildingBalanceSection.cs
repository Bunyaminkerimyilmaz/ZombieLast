using System.Text;
using LastHaven.Building;

namespace LastHaven.Editor
{
    /// <summary>Denge tablosu: yapılar ve seviyeleri (GDD §11): maliyet, toplam harcama, satış iadesi, can ve statlar; jeneratör canı.</summary>
    public sealed class BuildingBalanceSection : IBalanceSection
    {
        public int Order => 500;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Yapılar");
            if (context.Buildables.Count == 0)
            {
                BalanceFormat.Missing(text, "BuildableData");
                return;
            }

            var table = new MarkdownTable("Yapı", "Boyut", "Seviye", "Ad", "Can", "Maliyet", "Toplam harcama", "Satış iadesi", "Statlar");
            foreach (BuildableData buildable in context.Buildables)
            {
                int spent = 0;
                for (int i = 0; i < buildable.LevelCount; i++)
                {
                    BuildableLevel level = buildable.GetLevel(i);
                    if (level == null)
                    {
                        continue;
                    }

                    spent += level.Cost;
                    table.AddRow(
                        i == 0 ? buildable.DisplayName : string.Empty,
                        i == 0 ? BalanceFormat.Int(buildable.Footprint.x) + "×" + BalanceFormat.Int(buildable.Footprint.y) : string.Empty,
                        BalanceFormat.Int(i + 1),
                        level.DisplayName,
                        BalanceFormat.Number(level.MaxHealth),
                        BalanceFormat.Int(level.Cost),
                        BalanceFormat.Int(spent),
                        context.Economy != null ? BalanceFormat.Int(context.Economy.GetSellRefund(spent)) : BalanceFormat.None,
                        DescribeStats(level));
                }
            }

            table.AppendTo(text);
            text.Append('\n');
            if (context.Generator != null)
            {
                text.Append("Jeneratör: ").Append(BalanceFormat.Number(context.Generator.MaxHealth)).Append(" can. ");
            }

            text.Append("Statlar seviyenin kendi değerleridir (çarpan değil). Taret DPS = hasar × atış hızı (tek hedef, perk'siz).\n");
        }

        private static string DescribeStats(BuildableLevel level)
        {
            if (level.Stats.Count == 0)
            {
                return BalanceFormat.None;
            }

            var text = new StringBuilder();
            for (int i = 0; i < level.Stats.Count; i++)
            {
                if (i > 0)
                {
                    text.Append(", ");
                }

                text.Append(level.Stats[i].Stat).Append(' ').Append(BalanceFormat.Number(level.Stats[i].Value));
            }

            if (level.TryGetStat(BuildableStat.Damage, out float damage) && level.TryGetStat(BuildableStat.FireRate, out float fireRate))
            {
                text.Append(" (DPS ").Append(BalanceFormat.Number(damage * fireRate)).Append(')');
            }

            return text.ToString();
        }
    }
}
