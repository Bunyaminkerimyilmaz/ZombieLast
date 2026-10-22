using System.Text;
using LastHaven.Progression;

namespace LastHaven.Editor
{
    /// <summary>
    /// Denge tablosu: seviye eğrisi (GDD §13) ve gece başına XP'yle ulaşılan seviye (tüm öldürmeler oyuncudan sayılırsa; üst sınır),
    /// perk'lerin nadirlik ve yığın değerleri.
    /// </summary>
    public sealed class ProgressionBalanceSection : IBalanceSection
    {
        public int Order => 700;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Seviye ve perk'ler");
            if (context.Progression == null)
            {
                BalanceFormat.Missing(text, "ProgressionConfig");
                return;
            }

            ProgressionConfig config = context.Progression;
            LevelCurve curve = config.CreateLevelCurve();
            text.Append("Sonraki seviye için XP = ").Append(BalanceFormat.Int(curve.BaseXp)).Append(" + ")
                .Append(BalanceFormat.Int(curve.XpPerLevel)).Append(" × seviye. Başlangıç seviyesi ")
                .Append(BalanceFormat.Int(config.StartingLevel)).Append(config.PlayerKillsOnly ? "; sadece oyuncunun öldürdükleri XP verir" : string.Empty)
                .Append(". Seviye başına ").Append(BalanceFormat.Int(config.OfferSize)).Append(" kart.\n\n");

            var nights = new MarkdownTable("Gece", "Gece XP", "Kümülatif XP", "Gece sonu seviye (en çok)", "Toplam perk kartı (en çok)");
            long cumulative = 0;
            foreach (BalanceSheetContext.NightRow night in context.Nights)
            {
                cumulative += night.Totals.TotalXp;
                int level = LevelAt(curve, config.StartingLevel, cumulative);
                nights.AddRow(
                    BalanceFormat.Int(night.Number),
                    BalanceFormat.Int(night.Totals.TotalXp),
                    BalanceFormat.Int(cumulative),
                    BalanceFormat.Int(level),
                    BalanceFormat.Int(level - config.StartingLevel));
            }

            nights.AppendTo(text);
            AppendPerks(context, config, text);
        }

        private static void AppendPerks(BalanceSheetContext context, ProgressionConfig config, StringBuilder text)
        {
            text.Append('\n');
            if (context.Perks == null)
            {
                BalanceFormat.Missing(text, "PerkCatalog");
                return;
            }

            var table = new MarkdownTable("Perk", "id", "Nadirlik", "Ağırlık", "Maks. yığın", "Açıklama");
            int totalStacks = 0;
            foreach (PerkData perk in context.Perks.Perks)
            {
                if (perk == null)
                {
                    continue;
                }

                totalStacks += perk.MaxStacks;
                table.AddRow(perk.DisplayName, "`" + perk.Id + "`", perk.Rarity.ToString(), BalanceFormat.Int(config.GetRarityWeight(perk.Rarity)),
                    BalanceFormat.Int(perk.MaxStacks), perk.Description);
            }

            table.AppendTo(text);
            text.Append('\n').Append("Tüm perk'lerin toplam yığını: ").Append(BalanceFormat.Int(totalStacks)).Append(".\n");
        }

        /// <summary>Başlangıç seviyesinden <paramref name="totalXp"/> ile ulaşılan seviye (devreden XP, çoklu seviye).</summary>
        private static int LevelAt(LevelCurve curve, int startingLevel, long totalXp)
        {
            int level = startingLevel;
            long remaining = totalXp;
            while (remaining >= curve.XpToNext(level))
            {
                remaining -= curve.XpToNext(level);
                level++;
            }

            return level;
        }
    }
}
