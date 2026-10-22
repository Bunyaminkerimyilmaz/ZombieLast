using System;
using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Seviye kartlarının akışı (13.3, GDD §13): bekleyen seviye kuyruktan alınır, kartlar çekilir ve oyun duraklar; seçim giriş
    /// gecikmesinden sonra kabul edilir, perk alınır; seçim animasyonundan sonra sıradaki seviye gelir ya da oyun devam eder.
    /// </summary>
    public class LevelUpSequenceTests
    {
        private const float InputDelay = 0.4f;
        private const float ConfirmDuration = 0.6f;

        private FakeExperienceService _experience;
        private FakePerkService _perks;
        private FakePauseService _pause;
        private LevelUpSequence _sequence;

        [SetUp]
        public void SetUp()
        {
            _experience = new FakeExperienceService();
            _perks = new FakePerkService();
            _perks.Add("quick_trigger", 3);
            _perks.Add("heavy_rounds", 3);
            _perks.Add("thick_skin", 3);
            _perks.Add("vampire", 1, PerkRarity.Rare);
            _pause = new FakePauseService();
            _sequence = new LevelUpSequence(_experience, _perks, _pause, InputDelay, ConfirmDuration);
        }

        [Test]
        public void TryOpen_NoPendingLevel_StaysClosed()
        {
            Assert.IsFalse(_sequence.TryOpen());
            Assert.AreEqual(LevelUpStage.Closed, _sequence.Stage);
            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void TryOpen_PendingLevel_DequeuesDrawsThreeCardsAndPauses()
        {
            _experience.LevelUp();

            Assert.IsTrue(_sequence.TryOpen());

            Assert.AreEqual(LevelUpStage.Choosing, _sequence.Stage);
            Assert.AreEqual(2, _sequence.Level);
            Assert.AreEqual(3, _sequence.Offer.Count, "GDD §13: 3 kart.");
            Assert.AreEqual(0, _experience.PendingLevelUps, "Seviye kuyruktan alındı.");
            Assert.IsTrue(_pause.IsPausedBy(_sequence), "Seviye atlayınca oyun duraklar.");
        }

        [Test]
        public void TryOpen_WhileOpen_ReturnsFalse()
        {
            _experience.LevelUp();
            _experience.LevelUp();
            _sequence.TryOpen();

            Assert.IsFalse(_sequence.TryOpen());
            Assert.AreEqual(1, _experience.PendingLevelUps, "İkinci seviye kuyrukta bekler.");
        }

        [Test]
        public void Choose_DuringInputDelay_Ignored()
        {
            Open();

            Assert.IsFalse(_sequence.AcceptsChoice);
            Assert.IsFalse(_sequence.Choose(0), "Ateş / atılma tuşuna basılıyken kazara seçilmez.");
            _sequence.Tick(InputDelay * 0.5f);
            Assert.IsFalse(_sequence.Choose(0));
            CollectionAssert.IsEmpty(_perks.Applied);
        }

        [Test]
        public void Choose_AfterInputDelay_AppliesPerkAndConfirms()
        {
            Open();
            _sequence.Tick(InputDelay);

            Assert.IsTrue(_sequence.AcceptsChoice);
            Assert.IsTrue(_sequence.Choose(1));

            Assert.AreEqual(LevelUpStage.Confirming, _sequence.Stage);
            Assert.AreEqual(1, _sequence.ChosenIndex);
            CollectionAssert.AreEqual(new[] { _sequence.Offer[1] }, _perks.Applied, "Perk seçilince hemen alınır.");
            Assert.IsTrue(_pause.IsPaused, "Seçim animasyonu boyunca oyun duraklı.");
        }

        [Test]
        public void Choose_Twice_SecondIgnored()
        {
            OpenReady();
            _sequence.Choose(0);

            Assert.IsFalse(_sequence.Choose(1));
            Assert.AreEqual(1, _perks.Applied.Count);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void Choose_InvalidIndex_Ignored(int index)
        {
            OpenReady();

            Assert.IsFalse(_sequence.Choose(index));
            Assert.AreEqual(LevelUpStage.Choosing, _sequence.Stage);
        }

        [Test]
        public void Confirm_LastLevel_ClosesAndResumes()
        {
            OpenReady();
            _sequence.Choose(0);

            Assert.AreEqual(LevelUpTickResult.None, _sequence.Tick(ConfirmDuration * 0.5f), "Kısa animasyon sürüyor.");
            Assert.IsTrue(_pause.IsPaused);
            Assert.AreEqual(LevelUpTickResult.Closed, _sequence.Tick(ConfirmDuration));

            Assert.AreEqual(LevelUpStage.Closed, _sequence.Stage);
            Assert.IsFalse(_pause.IsPaused, "Kart seçilince oyun devam eder.");
            CollectionAssert.IsEmpty(_sequence.Offer);
        }

        [Test]
        public void Confirm_MoreLevelsQueued_ShowsNextLevelWithoutResuming()
        {
            _experience.LevelUp();
            _experience.LevelUp();
            _sequence.TryOpen();
            _sequence.Tick(InputDelay);
            _sequence.Choose(0);

            Assert.AreEqual(LevelUpTickResult.NextOffer, _sequence.Tick(ConfirmDuration));

            Assert.AreEqual(3, _sequence.Level, "Kuyruktaki sıradaki seviye.");
            Assert.AreEqual(LevelUpStage.Choosing, _sequence.Stage);
            Assert.AreEqual(-1, _sequence.ChosenIndex);
            Assert.IsFalse(_sequence.AcceptsChoice, "Yeni kartlarda da giriş gecikmesi var.");
            Assert.IsTrue(_pause.IsPaused, "Seviyeler arasında oyun devam etmez.");
        }

        [Test]
        public void FewerPerksLeft_ShowsFewerCards()
        {
            var perks = new FakePerkService();
            perks.Add("adrenaline");
            var sequence = new LevelUpSequence(_experience, perks, _pause, 0f, 0f);
            _experience.LevelUp();

            Assert.IsTrue(sequence.TryOpen());
            Assert.AreEqual(1, sequence.Offer.Count, "Maks. yığındakiler çıkmaz; kalan kadar kart.");
        }

        [Test]
        public void NoPerkLeft_LevelSkippedWithoutOpening()
        {
            var sequence = new LevelUpSequence(_experience, new FakePerkService(), _pause, InputDelay, ConfirmDuration);
            int skipped = 0;
            sequence.LevelSkipped += _ => skipped++;
            _experience.LevelUp();
            _experience.LevelUp();

            Assert.IsFalse(sequence.TryOpen());
            Assert.AreEqual(2, skipped, "Alınabilecek perk yoksa seviyeler kartsız geçer.");
            Assert.AreEqual(0, _experience.PendingLevelUps);
            Assert.IsFalse(_pause.IsPaused);
        }

        [Test]
        public void Close_WhileChoosing_ResumesWithoutPerk()
        {
            OpenReady();

            _sequence.Close();

            Assert.AreEqual(LevelUpStage.Closed, _sequence.Stage);
            Assert.IsFalse(_pause.IsPaused);
            CollectionAssert.IsEmpty(_perks.Applied);
        }

        [Test]
        public void OtherPauseSource_KeepsGamePausedAfterCardsClose()
        {
            var menu = new object();
            _pause.Pause(menu);
            OpenReady();
            _sequence.Choose(0);

            _sequence.Tick(ConfirmDuration);

            Assert.IsTrue(_pause.IsPaused, "Birden fazla kaynak: menü hâlâ duraklatıyor.");
            Assert.IsFalse(_pause.IsPausedBy(_sequence));
        }

        [Test]
        public void NoPauseService_StillWorks()
        {
            var sequence = new LevelUpSequence(_experience, _perks, null, 0f, 0f);
            _experience.LevelUp();

            Assert.IsTrue(sequence.TryOpen());
            Assert.IsTrue(sequence.Choose(0), "Gecikme 0: hemen seçilebilir.");
            Assert.AreEqual(LevelUpTickResult.Closed, sequence.Tick(0f));
        }

        [Test]
        public void Constructor_RequiresServices()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelUpSequence(null, _perks, _pause, 0f, 0f));
            Assert.Throws<ArgumentNullException>(() => new LevelUpSequence(_experience, null, _pause, 0f, 0f));
        }

        private void Open()
        {
            _experience.LevelUp();
            Assert.IsTrue(_sequence.TryOpen());
        }

        private void OpenReady()
        {
            Open();
            _sequence.Tick(InputDelay);
        }
    }
}
