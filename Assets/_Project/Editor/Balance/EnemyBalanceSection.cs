using System.Text;
using LastHaven.Enemies;

namespace LastHaven.Editor
{
    /// <summary>Denge tablosu: düşman tipleri (GDD §7) ve türetilmiş değerler (DPS, 100 can başına para).</summary>
    public sealed class EnemyBalanceSection : IBalanceSection
    {
        public int Order => 100;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Düşmanlar");
            if (context.Enemies.Count == 0)
            {
                BalanceFormat.Missing(text, "EnemyData");
                return;
            }

            var table = new MarkdownTable("Düşman", "id", "Can", "Hız (m/sn)", "Hasar", "Saldırı/sn", "DPS", "Menzil (m)", "Yapıya ×",
                "Aggro (m)", "Para", "XP", "Para / 100 can", "İlk gece");
            foreach (EnemyData enemy in context.Enemies)
            {
                int firstNight = context.FirstNightOf(enemy);
                table.AddRow(
                    enemy.DisplayName,
                    "`" + enemy.Id + "`",
                    BalanceFormat.Number(enemy.MaxHealth),
                    BalanceFormat.Number(enemy.MoveSpeed),
                    BalanceFormat.Number(enemy.Damage),
                    BalanceFormat.Number(enemy.AttackRate),
                    BalanceFormat.Number(enemy.Damage * enemy.AttackRate),
                    BalanceFormat.Number(enemy.AttackRange),
                    BalanceFormat.Number(enemy.StructureDamageMultiplier),
                    BalanceFormat.Number(enemy.AggroRadius),
                    BalanceFormat.Int(enemy.MoneyReward),
                    BalanceFormat.Int(enemy.XpReward),
                    BalanceFormat.Number(100d * enemy.MoneyReward / enemy.MaxHealth),
                    firstNight > 0 ? BalanceFormat.Int(firstNight) : BalanceFormat.None);
            }

            table.AppendTo(text);
            text.Append('\n')
                .Append("DPS = hasar × saldırı/sn (tek hedefe, yapı çarpanı hariç). Patlayan tek seferlik alan hasarı verir (DPS anlamsız). ")
                .Append("Saldırı biçimi (yakın / mermi / patlama / yer darbesi) ve diğer alanlar \"Tüm değerler\" bölümünde.\n");
        }
    }
}
