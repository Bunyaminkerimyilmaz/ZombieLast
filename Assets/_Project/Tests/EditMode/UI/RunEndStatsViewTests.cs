using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Oyun Bitti / Zafer istatistikleri (14.3, GDD §15): gece, öldürme, kazanılan para, en yüksek kombo, perk'ler.</summary>
    public class RunEndStatsViewTests
    {
        private sealed class FakeRunStats : IRunStats
        {
            public int NightKills { get; set; }
            public int TotalKills { get; set; }
            public int NightMoneyEarned { get; set; }
            public int TotalMoneyEarned { get; set; }
            public int NightBestCombo { get; set; }
            public int BestCombo { get; set; }
            public int LastInterest { get; set; }
            public int TotalInterest { get; set; }
        }

        private Label _night;
        private Label _kills;
        private Label _money;
        private Label _combo;
        private VisualElement _perks;
        private Label _noPerks;
        private RunEndStatsView _view;

        [SetUp]
        public void SetUp()
        {
            _night = new Label();
            _kills = new Label();
            _money = new Label();
            _combo = new Label();
            _perks = new VisualElement();
            _noPerks = new Label();
            _view = new RunEndStatsView(_night, _kills, _money, _combo, _perks, _noPerks);
        }

        [Test]
        public void Show_WritesStats()
        {
            var stats = new FakeRunStats { TotalKills = 412, TotalMoneyEarned = 3150, BestCombo = 27, NightKills = 9 };

            _view.Show(7, 10, stats, null);

            Assert.AreEqual(HudFormat.OfTotal(7, 10), _night.text);
            Assert.AreEqual("412", _kills.text);
            Assert.AreEqual("3150", _money.text, "Kazanılan para = run boyunca toplanan jetonlar.");
            Assert.AreEqual("27", _combo.text);
        }

        [Test]
        public void Show_WithoutServices_IsZeroAndNoPerks()
        {
            _view.Show(1, 10, null, null);

            Assert.AreEqual("0", _kills.text);
            Assert.AreEqual("0", _money.text);
            Assert.AreEqual(0, _view.PerkCount);
            Assert.IsTrue(_perks.ClassListContains(UIClassNames.Hidden));
            Assert.IsFalse(_noPerks.ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void Show_PerksInAcquisitionOrder_WithRarityAndStacks()
        {
            var perks = new FakePerkService();
            perks.Add("thick_skin", 3).DisplayName = "Kalın Deri";
            perks.Add("ricochet", 1, PerkRarity.Epic).DisplayName = "Seken Mermi";
            perks.ApplyPerk("ricochet");
            perks.ApplyPerk("thick_skin");
            perks.ApplyPerk("thick_skin");

            _view.Show(3, 10, new FakeRunStats(), perks);

            Assert.AreEqual(2, _view.PerkCount);
            VisualElement first = _perks[0];
            Assert.IsTrue(first.ClassListContains(PerkStyle.RarityClass(RunEndPerkChip.ChipClass, PerkRarity.Epic)));
            Assert.AreEqual("Seken Mermi", first.Q<Label>(className: RunEndPerkChip.NameClass).text);
            Assert.AreEqual(HudFormat.OfTotal(2, 3), _perks[1].Q<Label>(className: RunEndPerkChip.StacksClass).text);
            Assert.IsFalse(_perks.ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(_noPerks.ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void Show_Twice_DoesNotDuplicateChips()
        {
            var perks = new FakePerkService();
            perks.Add("greedy", 2);
            perks.ApplyPerk("greedy");

            _view.Show(2, 10, null, perks);
            _view.Show(2, 10, null, perks);

            Assert.AreEqual(1, _view.PerkCount);
        }
    }
}
