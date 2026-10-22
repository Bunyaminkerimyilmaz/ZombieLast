using System.Text;
using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>
    /// Denge tablosu: geceler (GDD §8). Gece başına toplam düşman canı, taban para ödülü, XP, son spawn anı, dalgaya yetişmek için gereken
    /// en az DPS ve kümülatif toplamlar. Adetler gece olayının çarpanıyla (Kan Ayı ×1.5, boss hariç), para olayın jeton çarpanıyla.
    /// </summary>
    public sealed class NightBalanceSection : IBalanceSection
    {
        public int Order => 200;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Geceler");
            if (context.Waves == null)
            {
                BalanceFormat.Missing(text, "WaveConfig");
                return;
            }

            var table = new MarkdownTable("Gece", "Yönler", "Olay", "İçerik", "Düşman", "Toplam can", "Para (taban)", "XP",
                "Son spawn (sn)", "Gereken DPS", "Kümülatif can", "Kümülatif para");
            double cumulativeHealth = 0d;
            long cumulativeMoney = 0;
            foreach (BalanceSheetContext.NightRow night in context.Nights)
            {
                NightTotals totals = night.Totals;
                cumulativeHealth += totals.TotalHealth;
                cumulativeMoney += totals.TotalMoney;
                table.AddRow(
                    BalanceFormat.Int(night.Number),
                    DescribeDirections(night.Info),
                    night.Info.HasEvent ? night.Info.EventName : BalanceFormat.None,
                    totals.DescribeContents(),
                    BalanceFormat.Int(totals.EnemyCount),
                    BalanceFormat.Round(totals.TotalHealth),
                    BalanceFormat.Int(totals.TotalMoney),
                    BalanceFormat.Int(totals.TotalXp),
                    BalanceFormat.Number(totals.SpawnDuration),
                    BalanceFormat.Round(totals.HealthPerSecond),
                    BalanceFormat.Round(cumulativeHealth),
                    BalanceFormat.Int(cumulativeMoney));
            }

            table.AppendTo(text);
            text.Append('\n')
                .Append("- Para (taban) = Σ ödül × gecenin jeton çarpanı. Kombo (×1.5 / ×2 / ×3), Açgözlü perk'i ve toplanmayıp kaybolan ")
                .Append("jetonlar hesaba katılmaz; gerçek gelir bunun altında da üstünde de olabilir.\n")
                .Append("- Son spawn = gece başından son düşmanın doğma anı (grup gecikmesi + (adet − 1) × aralık). Canlı düşman tavanı (")
                .Append(BalanceFormat.Int(context.Waves.MaxAliveEnemies))
                .Append(") dolarsa gece uzar.\n")
                .Append("- Gereken DPS = toplam can / son spawn: düşmanlar birikmeden ölmesi için oyuncu + taretlerin ortalama vermesi gereken ")
                .Append("en az hasar (ıskalama, geri itilme ve yürüme süresi hariç).\n");
        }

        private static string DescribeDirections(NightInfo info)
        {
            if (info.Directions.Count == 0)
            {
                return BalanceFormat.None;
            }

            var text = new StringBuilder();
            for (int i = 0; i < info.Directions.Count; i++)
            {
                if (i > 0)
                {
                    text.Append(", ");
                }

                text.Append(info.Directions[i]);
            }

            return text.ToString();
        }
    }
}
