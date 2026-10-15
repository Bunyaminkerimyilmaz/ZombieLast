using LastHaven.Core;
using LastHaven.GameFlow;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// DayNightBlend: alacakaranlık boyunca geceye, şafakta gündüze doğrusal geçiş; gece evresi bitmemiş geçişi tamamlar;
    /// açılış ve oyun bitti ışığı değiştirmez. Smoothstep uçlarda 0/1, ortada 0.5.
    /// </summary>
    public class DayNightBlendTests
    {
        private const float Tolerance = 0.0001f;
        private const float DuskDuration = 4f;
        private const float DawnDuration = 3f;

        [Test]
        public void StartsAtDay_NotTransitioning()
        {
            var blend = new DayNightBlend(DawnDuration);

            Assert.AreEqual(0f, blend.Value);
            Assert.AreEqual(0f, blend.Eased);
            Assert.IsFalse(blend.IsTransitioning);
            Assert.IsFalse(blend.Tick(1f), "Geçiş yokken değer değişmez.");
        }

        [Test]
        public void Dusk_ReachesNightOverPhaseDuration()
        {
            var blend = new DayNightBlend(DawnDuration);

            Assert.IsTrue(blend.OnPhaseChanged(GamePhase.Dusk, DuskDuration));
            Assert.AreEqual(0f, blend.Value, "Alacakaranlık gündüzden başlar.");
            Assert.AreEqual(1f, blend.Target);

            Assert.IsTrue(blend.Tick(2f));
            Assert.AreEqual(0.5f, blend.Value, Tolerance);
            Assert.AreEqual(0.5f, blend.Eased, Tolerance);

            Assert.IsTrue(blend.Tick(2f));
            Assert.AreEqual(1f, blend.Value, Tolerance, "4 sn'de gece.");
            Assert.IsFalse(blend.IsTransitioning);
            Assert.IsFalse(blend.Tick(1f));
        }

        [Test]
        public void Dawn_ReturnsToDayOverDawnDuration()
        {
            var blend = NightBlend();

            Assert.IsTrue(blend.OnPhaseChanged(GamePhase.Dawn, 0f));
            blend.Tick(1.5f);
            Assert.AreEqual(0.5f, blend.Value, Tolerance);

            blend.Tick(1.5f);
            Assert.AreEqual(0f, blend.Value, Tolerance);
            Assert.IsFalse(blend.IsTransitioning);
        }

        [Test]
        public void Night_CompletesUnfinishedDusk()
        {
            var blend = new DayNightBlend(DawnDuration);
            blend.OnPhaseChanged(GamePhase.Dusk, DuskDuration);
            blend.Tick(3.9f);

            Assert.IsTrue(blend.OnPhaseChanged(GamePhase.Night, 0f));

            Assert.AreEqual(1f, blend.Value, "Gece başında geçiş tamamlanır.");
            Assert.IsFalse(blend.IsTransitioning);
        }

        [Test]
        public void ZeroDuskDuration_SnapsToNight()
        {
            var blend = new DayNightBlend(DawnDuration);

            blend.OnPhaseChanged(GamePhase.Dusk, 0f);

            Assert.AreEqual(1f, blend.Value);
            Assert.IsFalse(blend.IsTransitioning);
        }

        [Test]
        public void ZeroDawnDuration_SnapsToDay()
        {
            var blend = new DayNightBlend(0f);
            blend.OnPhaseChanged(GamePhase.Dusk, 0f);

            blend.OnPhaseChanged(GamePhase.Dawn, 0f);

            Assert.AreEqual(0f, blend.Value);
        }

        [Test]
        public void Victory_ReturnsToDay()
        {
            var blend = NightBlend();

            Assert.IsTrue(blend.OnPhaseChanged(GamePhase.Victory, 0f));
            blend.Tick(DawnDuration);

            Assert.AreEqual(0f, blend.Value, Tolerance);
        }

        [TestCase(GamePhase.GameOver)]
        [TestCase(GamePhase.Boot)]
        public void UnchangedPhase_KeepsRunningTransition(GamePhase phase)
        {
            var blend = new DayNightBlend(DawnDuration);
            blend.OnPhaseChanged(GamePhase.Dusk, DuskDuration);
            blend.Tick(1f);

            Assert.IsFalse(blend.OnPhaseChanged(phase, 0f));
            Assert.AreEqual(1f, blend.Target, "Hedef değişmez.");

            blend.Tick(3f);
            Assert.AreEqual(1f, blend.Value, Tolerance, "Süren geçiş tamamlanır.");
        }

        [Test]
        public void ReversalMidway_TakesProportionalTime()
        {
            var blend = new DayNightBlend(DawnDuration);
            blend.OnPhaseChanged(GamePhase.Dusk, DuskDuration);
            blend.Tick(2f);

            blend.OnPhaseChanged(GamePhase.Dawn, 0f);
            blend.Tick(1.5f);

            Assert.AreEqual(0f, blend.Value, Tolerance, "Yarı yoldan dönüş şafak süresinin yarısında biter.");
        }

        [Test]
        public void Tick_IgnoresNonPositiveDelta()
        {
            var blend = new DayNightBlend(DawnDuration);
            blend.OnPhaseChanged(GamePhase.Dusk, DuskDuration);

            Assert.IsFalse(blend.Tick(0f));
            Assert.IsFalse(blend.Tick(-1f));
            Assert.AreEqual(0f, blend.Value);
        }

        [Test]
        public void NegativeDawnDuration_TreatedAsInstant()
        {
            var blend = new DayNightBlend(-2f);
            blend.OnPhaseChanged(GamePhase.Dusk, 0f);

            blend.OnPhaseChanged(GamePhase.Dawn, 0f);

            Assert.AreEqual(0f, blend.Value);
        }

        private static DayNightBlend NightBlend()
        {
            var blend = new DayNightBlend(DawnDuration);
            blend.OnPhaseChanged(GamePhase.Dusk, 0f);
            Assert.AreEqual(1f, blend.Value);
            return blend;
        }
    }
}
