using System.Text;
using LastHaven.Combat;

namespace LastHaven.Editor
{
    /// <summary>Denge tablosu: silahlar (GDD §6) ve türetilmiş DPS (ham ve doldurma dahil sürekli).</summary>
    public sealed class WeaponBalanceSection : IBalanceSection
    {
        public int Order => 400;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Silahlar");
            if (context.Weapons.Count == 0)
            {
                BalanceFormat.Missing(text, "WeaponData");
                return;
            }

            var table = new MarkdownTable("Silah", "id", "Hasar × saçma", "Atış/sn", "Ham DPS", "Şarjör", "Doldurma (sn)", "Sürekli DPS",
                "Yedek", "Menzil (m)", "Saçılma (°)");
            foreach (WeaponData weapon in context.Weapons)
            {
                double perShot = (double)weapon.Damage * weapon.PelletCount;
                double rawDps = perShot * weapon.FireRate;
                double cycle = weapon.MagSize / weapon.FireRate + weapon.ReloadTime;
                double sustained = cycle > 0d ? perShot * weapon.MagSize / cycle : rawDps;
                table.AddRow(
                    weapon.DisplayName,
                    "`" + weapon.Id + "`",
                    BalanceFormat.Number(weapon.Damage) + " × " + BalanceFormat.Int(weapon.PelletCount),
                    BalanceFormat.Number(weapon.FireRate),
                    BalanceFormat.Number(rawDps),
                    BalanceFormat.Int(weapon.MagSize),
                    BalanceFormat.Number(weapon.ReloadTime),
                    BalanceFormat.Number(sustained),
                    weapon.HasInfiniteReserve ? "∞" : BalanceFormat.Int(weapon.ReserveAmmo),
                    BalanceFormat.Number(weapon.Range),
                    BalanceFormat.Number(weapon.SpreadAngle));
            }

            table.AppendTo(text);
            text.Append('\n')
                .Append("Sürekli DPS = şarjör hasarı / (şarjörü boşaltma + doldurma). Tüm saçmaların isabet ettiği varsayılır; ")
                .Append("yükseltme ve perk'ler hariç. Yükseltme fiyatları \"Market\", yükseltme yüzdeleri \"Tüm değerler\" bölümünde.\n");
        }
    }
}
