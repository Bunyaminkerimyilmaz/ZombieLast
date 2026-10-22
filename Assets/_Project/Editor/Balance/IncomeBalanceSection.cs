using System.Text;
using LastHaven.Core;
using LastHaven.Economy;

namespace LastHaven.Editor
{
    /// <summary>
    /// Denge tablosu: gece başına kazanılabilir para (öldürme + bir önceki gündüzün sandıkları ve gezginleri + erken hazır + şafak faizi)
    /// ve yakıt ihtiyacı (GDD §9, §10, §12). Sandık ve gezgin beklenen değerdir; erken hazır ve faiz üst sınırdır.
    /// </summary>
    public sealed class IncomeBalanceSection : IBalanceSection
    {
        public int Order => 300;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Gece başına kazanılabilir para");
            if (context.Economy == null || context.Game == null)
            {
                BalanceFormat.Missing(text, "EconomyConfig / GameConfig");
                return;
            }

            double crateMoney = ExpectedPerDay(context, LootRewardType.Money);
            double crateFuel = ExpectedPerDay(context, LootRewardType.Fuel);
            double wandererMoney = context.Wanderers != null && context.Wanderers.Enemy != null
                ? (context.Wanderers.MinCount + context.Wanderers.MaxCount) * 0.5d * context.Wanderers.Enemy.MoneyReward
                : 0d;
            int earlyReadyMax = context.Game.CreateFlowSettings().CalculateEarlyReadyBonus(context.Game.DayDuration);
            int interestCap = context.Economy.InterestCap;

            AppendAssumptions(context, text, crateMoney, crateFuel, wandererMoney, earlyReadyMax);

            var table = new MarkdownTable("Gece", "Öldürme (taban)", "Sandık (beklenen)", "Gezgin (beklenen)", "Erken hazır (en çok)",
                "Faiz (en çok)", "Toplam", "Kümülatif (başlangıç dahil)");
            double cumulative = context.Economy.StartingMoney;
            int count = context.Nights.Count;
            for (int i = 0; i < count; i++)
            {
                BalanceSheetContext.NightRow night = context.Nights[i];
                // Son gecenin şafağı yok (zafer): faiz ödenmez.
                int interest = i < count - 1 ? interestCap : 0;
                double total = night.Totals.TotalMoney + crateMoney + wandererMoney + earlyReadyMax + interest;
                cumulative += total;
                table.AddRow(
                    BalanceFormat.Int(night.Number),
                    BalanceFormat.Int(night.Totals.TotalMoney),
                    BalanceFormat.Round(crateMoney),
                    BalanceFormat.Round(wandererMoney),
                    BalanceFormat.Int(earlyReadyMax),
                    BalanceFormat.Int(interest),
                    BalanceFormat.Round(total),
                    BalanceFormat.Round(cumulative));
            }

            table.AppendTo(text);
            AppendFuel(context, text, crateFuel);
        }

        private static void AppendAssumptions(BalanceSheetContext context, StringBuilder text, double crateMoney, double crateFuel,
            double wandererMoney, int earlyReadyMax)
        {
            EconomyConfig economy = context.Economy;
            text.Append("- Başlangıç: ").Append(BalanceFormat.Int(economy.StartingMoney)).Append(" para, ")
                .Append(BalanceFormat.Int(economy.StartingFuel)).Append(" yakıt.\n")
                .Append("- Sandık: günde ").Append(context.Loot != null ? BalanceFormat.Int(context.Loot.CratesPerDay) : BalanceFormat.None)
                .Append(" tane; günlük beklenen ").Append(BalanceFormat.Round(crateMoney)).Append(" para + ")
                .Append(BalanceFormat.Number(crateFuel)).Append(" yakıt (uzak payı halkanın alanından; aralığın ortası). ")
                .Append("Hepsinin açılabildiği varsayılır.\n")
                .Append("- Gezgin: günde ortalama ")
                .Append(context.Wanderers != null ? BalanceFormat.Number((context.Wanderers.MinCount + context.Wanderers.MaxCount) * 0.5d)
                    : BalanceFormat.None)
                .Append(" düşman, ").Append(BalanceFormat.Round(wandererMoney)).Append(" para (öldürülürse; gece başlayınca dalgaya katılır).\n")
                .Append("- Erken hazır en çok ").Append(BalanceFormat.Int(earlyReadyMax))
                .Append(" para (gündüz hiç kullanılmadan); sandık/gezgin geliriyle birbirini kısar, toplam bu yüzden üst sınırdır.\n")
                .Append("- Faiz: şafakta paranın ").Append(BalanceFormat.Percent(economy.InterestRate)).Append("'u, en çok ")
                .Append(BalanceFormat.Int(economy.InterestCap)).Append(" (tavan için şafakta ≥ ")
                .Append(economy.InterestRate > 0f ? BalanceFormat.Round(economy.InterestCap / economy.InterestRate) : BalanceFormat.None)
                .Append(" para gerekir).\n\n");
        }

        private static void AppendFuel(BalanceSheetContext context, StringBuilder text, double crateFuel)
        {
            text.Append('\n').Append("### Yakıt").Append('\n').Append('\n');
            if (context.Generator == null)
            {
                BalanceFormat.Missing(text, "GeneratorData");
                return;
            }

            float baseRate = context.Generator.BaseFuelConsumption;
            float turretRate = context.Generator.FuelConsumptionPerTurret;
            text.Append("Tüketim sadece gece: taban ").Append(BalanceFormat.Number(baseRate)).Append("/sn + taret başına ")
                .Append(BalanceFormat.Number(turretRate)).Append("/sn. Kapasite ").Append(BalanceFormat.Int(context.Generator.FuelCapacity))
                .Append(". Acil yakıt ").Append(BalanceFormat.Int(context.Generator.EmergencyFuelAmount)).Append(" birim / ")
                .Append(BalanceFormat.Int(context.Generator.EmergencyFuelPrice)).Append(" para. Gece süresi en az son spawn anıdır; ")
                .Append("gerçek gece daha uzundur (son düşmanların ölmesi).\n\n");

            var table = new MarkdownTable("Gece", "Son spawn (sn)", "Yakıt (taretsiz, en az)", "Taret başına +", "Sandık yakıtı (beklenen, önceki gün)");
            foreach (BalanceSheetContext.NightRow night in context.Nights)
            {
                float duration = night.Totals.SpawnDuration;
                table.AddRow(
                    BalanceFormat.Int(night.Number),
                    BalanceFormat.Number(duration),
                    BalanceFormat.Number(baseRate * duration),
                    BalanceFormat.Number(turretRate * duration),
                    BalanceFormat.Number(crateFuel));
            }

            table.AppendTo(text);
        }

        /// <summary>Günlük sandık sayısı × (yakın/uzak payına göre) sandık başı beklenen miktar.</summary>
        private static double ExpectedPerDay(BalanceSheetContext context, LootRewardType type)
        {
            LootConfig loot = context.Loot;
            LootTableData table = loot != null ? loot.LootTable : null;
            if (table == null)
            {
                return 0d;
            }

            LootRing ring = loot.CreateRing();
            double far = LootExpectation.FarFraction(ring.MinRadius, ring.MaxRadius, table.FarDistance);
            double perCrate = (1d - far) * LootExpectation.Expected(table.NearRewards, type)
                              + far * LootExpectation.Expected(table.FarRewards, type);
            return loot.CratesPerDay * perCrate;
        }
    }
}
