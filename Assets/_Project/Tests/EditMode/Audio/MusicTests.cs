using LastHaven.Audio;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Müzik seçimi (<see cref="MusicSelector"/>: evre + boss) ve iki kaynak arası geçiş (<see cref="MusicCrossfade"/>) (14.2).</summary>
    public class MusicTests
    {
        private const float Tolerance = 0.0001f;

        [TestCase(GamePhase.Boot, false, MusicCue.None)]
        [TestCase(GamePhase.Day, false, MusicCue.Day)]
        [TestCase(GamePhase.Day, true, MusicCue.Day)]
        [TestCase(GamePhase.Dusk, false, MusicCue.Night)]
        [TestCase(GamePhase.Night, false, MusicCue.Night)]
        [TestCase(GamePhase.Night, true, MusicCue.Boss)]
        [TestCase(GamePhase.Dawn, false, MusicCue.Day)]
        [TestCase(GamePhase.GameOver, true, MusicCue.GameOver)]
        [TestCase(GamePhase.Victory, false, MusicCue.Victory)]
        public void Select_PhaseAndBoss(GamePhase phase, bool bossActive, MusicCue expected)
        {
            Assert.AreEqual(expected, MusicSelector.Select(phase, bossActive));
        }

        [Test]
        public void Selector_BossLifecycle()
        {
            var selector = new MusicSelector();
            var brute = new object();
            selector.SetPhase(GamePhase.Night);

            Assert.AreEqual(MusicCue.Night, selector.Current);

            selector.BossSpawned(brute);
            Assert.AreEqual(MusicCue.Boss, selector.Current);

            selector.BossSpawned(brute);
            selector.BossDefeated(brute);
            Assert.AreEqual(MusicCue.Night, selector.Current, "Aynı boss iki kez sayılmaz.");
        }

        [Test]
        public void Selector_GameOverWinsOverBoss_AndDawnClearsBosses()
        {
            var selector = new MusicSelector();
            selector.SetPhase(GamePhase.Night);
            selector.BossSpawned(new object());

            selector.SetPhase(GamePhase.GameOver);
            Assert.AreEqual(MusicCue.GameOver, selector.Current);

            selector.SetPhase(GamePhase.Dawn);
            Assert.AreEqual(0, selector.ActiveBossCount);
            selector.SetPhase(GamePhase.Night);
            Assert.AreEqual(MusicCue.Night, selector.Current);
        }

        [Test]
        public void Crossfade_InitialState_Settled()
        {
            var fade = new MusicCrossfade(2f);

            Assert.IsFalse(fade.IsFading);
            Assert.AreEqual(0, fade.ActiveDeck);
            Assert.AreEqual(1f, fade.GetWeight(0), Tolerance);
            Assert.AreEqual(0f, fade.GetWeight(1), Tolerance);
        }

        [Test]
        public void Crossfade_Begin_FlipsDeckAndFadesOverDuration()
        {
            var fade = new MusicCrossfade(2f);

            int deck = fade.Begin();

            Assert.AreEqual(1, deck);
            Assert.IsTrue(fade.IsFading);
            Assert.AreEqual(0f, fade.GetWeight(1), Tolerance, "Gelen sessiz başlar.");
            Assert.AreEqual(1f, fade.GetWeight(0), Tolerance, "Giden tam başlar.");

            fade.Tick(1f);
            float half = UnityEngine.Mathf.Sqrt(0.5f);
            Assert.AreEqual(half, fade.GetWeight(1), Tolerance, "Eşit güç: yarıda ikisi de √½.");
            Assert.AreEqual(half, fade.GetWeight(0), Tolerance);

            fade.Tick(5f);
            Assert.IsFalse(fade.IsFading);
            Assert.AreEqual(1f, fade.GetWeight(1), Tolerance);
            Assert.AreEqual(0f, fade.GetWeight(0), Tolerance);
        }

        [Test]
        public void Crossfade_InterruptedMidway_OutgoingStartsFromCurrentWeight()
        {
            var fade = new MusicCrossfade(2f);
            fade.Begin();
            fade.Tick(0.5f);
            float incomingBefore = fade.GetWeight(1);

            int deck = fade.Begin();

            Assert.AreEqual(0, deck);
            Assert.AreEqual(incomingBefore, fade.GetWeight(1), Tolerance, "Ses sıçramaz.");
            Assert.AreEqual(0f, fade.GetWeight(0), Tolerance);
        }

        [Test]
        public void Crossfade_ZeroDuration_IsInstant()
        {
            var fade = new MusicCrossfade(0f);

            fade.Begin();

            Assert.IsFalse(fade.IsFading);
            Assert.AreEqual(1f, fade.GetWeight(1), Tolerance);
            Assert.AreEqual(0f, fade.GetWeight(0), Tolerance);
        }
    }
}
