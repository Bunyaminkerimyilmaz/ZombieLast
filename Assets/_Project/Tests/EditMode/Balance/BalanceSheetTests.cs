using System;
using System.Text;
using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Denge tablosu (15.1): markdown tablosu, belge birleştirme (analiz korunur), gece toplamları, sandık beklentisi, ham değer dökümü
    /// ve projenin gerçek verisiyle tüm bölümler.
    /// </summary>
    public class BalanceSheetTests
    {
        // --- MarkdownTable ---

        [Test]
        public void Table_WritesHeaderSeparatorAndRows_EscapingCells()
        {
            var table = new MarkdownTable("A", "B");
            table.AddRow("1", "x|y").AddRow("2", "satır\nsonu");
            var text = new StringBuilder();

            table.AppendTo(text);

            string[] lines = text.ToString().Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
            CollectionAssert.AreEqual(new[] { "| A | B |", "|---|---|", "| 1 | x\\|y |", "| 2 | satır sonu |" }, lines);
            Assert.AreEqual(2, table.RowCount);
        }

        [Test]
        public void Table_RejectsWrongCellCount()
        {
            var table = new MarkdownTable("A", "B");

            Assert.Throws<ArgumentException>(() => table.AddRow("1"));
            Assert.Throws<ArgumentException>(() => new MarkdownTable());
        }

        // --- BalanceDocument ---

        [Test]
        public void Document_New_HasHeaderMarkersAndAnalysisPlaceholder()
        {
            Assert.IsTrue(BalanceDocument.TryMerge(null, "## T\r\n\r\n| a |\r\n", out string merged));

            StringAssert.StartsWith(BalanceDocument.Header, merged);
            StringAssert.Contains(BalanceDocument.BeginMarker + "\n## T\n\n| a |\n" + BalanceDocument.EndMarker, merged);
            StringAssert.EndsWith(BalanceDocument.AnalysisPlaceholder, merged);
            StringAssert.DoesNotContain("\r", merged);
        }

        [Test]
        public void Document_ReplacesOnlyBetweenMarkers_KeepingAnalysis()
        {
            string existing = "# Başlık\r\n\r\n" + BalanceDocument.BeginMarker + "\r\neski\r\n" + BalanceDocument.EndMarker
                              + "\r\n\r\n## Analiz ve öneriler\r\n\r\nElle yazılan.\r\n";

            Assert.IsTrue(BalanceDocument.TryMerge(existing, "yeni", out string merged));

            Assert.AreEqual("# Başlık\n\n" + BalanceDocument.BeginMarker + "\nyeni\n" + BalanceDocument.EndMarker
                            + "\n\n## Analiz ve öneriler\n\nElle yazılan.\n", merged);
        }

        [Test]
        public void Document_WithoutMarkers_PrependsTablesAndKeepsOldText()
        {
            Assert.IsTrue(BalanceDocument.TryMerge("Eski notlar.\n", "tablo", out string merged));

            StringAssert.StartsWith(BalanceDocument.Header, merged);
            StringAssert.EndsWith(BalanceDocument.EndMarker + "\n\nEski notlar.\n", merged);
        }

        [Test]
        public void Document_BrokenMarkers_AreNotMerged()
        {
            Assert.IsFalse(BalanceDocument.TryMerge(BalanceDocument.BeginMarker + "\nyarım", "x", out string merged));
            Assert.IsNull(merged);
            Assert.IsFalse(BalanceDocument.TryMerge(BalanceDocument.EndMarker + "\n" + BalanceDocument.BeginMarker, "x", out _));
            Assert.IsFalse(BalanceDocument.TryMerge(
                BalanceDocument.BeginMarker + BalanceDocument.BeginMarker + BalanceDocument.EndMarker, "x", out _));
        }

        // --- NightTotals ---

        [Test]
        public void NightTotals_SumsHealthMoneyXp_AndSpawnDuration()
        {
            var totals = new NightTotals(2f);
            totals.AddGroup("Yürüyen", 30, 60f, 10, 1, 0f, 0.6f);
            totals.AddGroup("Tank", 2, 400f, 60, 5, 20f, 3f);
            totals.AddGroup("Brute", 1, 3000f, 500, 30, 25f, 0f, isBoss: true);
            totals.AddGroup("Yok", 0, 999f, 999, 999, 99f, 9f);

            Assert.AreEqual(33, totals.EnemyCount);
            Assert.AreEqual(1, totals.BossCount);
            Assert.AreEqual(30 * 60 + 2 * 400 + 3000, totals.TotalHealth, 0.001);
            Assert.AreEqual((30 * 10 + 2 * 60 + 500) * 2, totals.TotalMoney);
            Assert.AreEqual(30 + 10 + 30, totals.TotalXp);
            Assert.AreEqual(25f, totals.SpawnDuration, 0.0001f, "En geç spawn: Yürüyen 17.4, Tank 23, Brute 25 sn.");
        }

        [Test]
        public void NightTotals_SpawnDurationIsLatestSpawn_AndHealthPerSecond()
        {
            var totals = new NightTotals();
            totals.AddGroup("A", 11, 10f, 1, 1, 0f, 1f);
            totals.AddGroup("B", 1, 100f, 1, 1, 20f, 0f);

            Assert.AreEqual(20f, totals.SpawnDuration, 0.0001f);
            Assert.AreEqual(210d / 20d, totals.HealthPerSecond, 0.0001);
            Assert.AreEqual(0d, new NightTotals().HealthPerSecond);
        }

        [Test]
        public void NightTotals_DescribesContentsInFirstSeenOrder()
        {
            var totals = new NightTotals();
            totals.AddGroup("Yürüyen", 20, 60f, 10, 1, 0f, 1f);
            totals.AddGroup("Koşucu", 8, 35f, 12, 1, 0f, 1f);
            totals.AddGroup("Yürüyen", 10, 60f, 10, 1, 5f, 1f);
            totals.AddGroup("Brute", 1, 3000f, 500, 30, 10f, 0f, isBoss: true);

            Assert.AreEqual("30 Yürüyen, 8 Koşucu, Boss: Brute", totals.DescribeContents());
        }

        // --- LootExpectation ---

        [Test]
        public void Loot_ExpectedIsWeightedMidpoint_PerType()
        {
            var options = new[]
            {
                new LootRewardOption(LootRewardType.Money, 30, 60, 3f),
                new LootRewardOption(LootRewardType.Fuel, 15, 15, 1f),
            };

            Assert.AreEqual(45d * 0.75d, LootExpectation.Expected(options, LootRewardType.Money), 0.0001);
            Assert.AreEqual(15d * 0.25d, LootExpectation.Expected(options, LootRewardType.Fuel), 0.0001);
            Assert.AreEqual(0d, LootExpectation.Expected(options, LootRewardType.AmmoPack));
            Assert.AreEqual(0d, LootExpectation.Expected(new[] { new LootRewardOption(LootRewardType.Money, 1, 1, 0f) }, LootRewardType.Money));
        }

        [Test]
        public void Loot_FarFractionIsRingAreaShare()
        {
            // GDD §9: 22–38 m halka, uzak ≥ 32 m → (38² − 32²) / (38² − 22²) = 420 / 960.
            Assert.AreEqual(420d / 960d, LootExpectation.FarFraction(22f, 38f, 32f), 0.0001);
            Assert.AreEqual(1d, LootExpectation.FarFraction(22f, 38f, 10f));
            Assert.AreEqual(0d, LootExpectation.FarFraction(22f, 38f, 50f));
        }

        // --- SerializedValueTable ---

        [Test]
        public void RawValues_CleanPath_RemovesArrayDataAndUnderscores()
        {
            Assert.AreEqual("levels[0].cost", SerializedValueTable.CleanPath("_levels.Array.data[0]._cost"));
            Assert.AreEqual("maxHealth", SerializedValueTable.CleanPath("_maxHealth"));
        }

        [Test]
        public void RawValues_DumpsVisibleFields()
        {
            var generator = ScriptableObject.CreateInstance<GeneratorData>();
            try
            {
                var text = new StringBuilder();
                SerializedValueTable.Build(generator).AppendTo(text);

                StringAssert.Contains("| maxHealth | 1500 |", text.ToString());
                StringAssert.Contains("| fuelCapacity | 150 |", text.ToString());
                StringAssert.DoesNotContain("m_Script", text.ToString());
            }
            finally
            {
                Object.DestroyImmediate(generator);
            }
        }

        // --- Projenin verisiyle ---

        [Test]
        public void ProjectData_AllSectionsWritten_NightOneMatchesGdd()
        {
            var context = new BalanceSheetContext();

            string tables = BalanceSheetExporter.BuildTables(context, out int sections);

            Assert.AreEqual(8, sections);
            foreach (string heading in new[] { "## Düşmanlar", "## Geceler", "## Gece başına kazanılabilir para", "## Silahlar", "## Yapılar",
                         "## Market", "## Seviye ve perk'ler", "## Tüm değerler (ham)" })
            {
                StringAssert.Contains(heading, tables);
            }

            StringAssert.DoesNotContain("bulunamadı", tables, "Tüm veri asset'leri yüklenmeli.");
            StringAssert.DoesNotContain("hata verdi", tables);
            Assert.AreEqual(context.Waves.Nights.Count, context.Nights.Count);

            // GDD §7, §8: 1. gece 20 Yürüyen (60 can, 10 para, 1 XP).
            NightTotals first = context.Nights[0].Totals;
            Assert.AreEqual(20, first.EnemyCount);
            Assert.AreEqual(1200d, first.TotalHealth, 0.001);
            Assert.AreEqual(200, first.TotalMoney);
            Assert.AreEqual(20, first.TotalXp);
        }
    }
}
