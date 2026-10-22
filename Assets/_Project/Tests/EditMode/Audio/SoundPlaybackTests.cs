using System.Collections.Generic;
using LastHaven.Audio;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Ses çalma sınırları (<see cref="SoundGate"/>) ve kopya süreleri (<see cref="VoiceTimers{TVoice}"/>) (14.2).</summary>
    public class SoundPlaybackTests
    {
        private const float Tolerance = 0.0001f;

        private readonly object _shot = new object();
        private readonly object _coin = new object();

        // --- SoundGate ---

        [Test]
        public void Gate_FirstPlayAllowed_NullRejected()
        {
            var gate = new SoundGate(8);

            Assert.IsTrue(gate.CanPlay(_shot, 1, 10f, 0f), "İlk çalışta bekleme yok.");
            Assert.IsFalse(gate.CanPlay(null, 0, 0f, 0f));
        }

        [Test]
        public void Gate_MaxSimultaneous_BlocksUntilFinished()
        {
            var gate = new SoundGate(8);
            gate.Started(_shot, 0f);
            gate.Started(_shot, 0f);

            Assert.IsFalse(gate.CanPlay(_shot, 2, 0f, 1f));
            Assert.IsTrue(gate.CanPlay(_coin, 2, 0f, 1f), "Sınır ses başına.");

            gate.Finished(_shot);

            Assert.IsTrue(gate.CanPlay(_shot, 2, 0f, 1f));
            Assert.AreEqual(1, gate.GetActive(_shot));
        }

        [Test]
        public void Gate_ZeroMaxSimultaneous_IsUnlimited()
        {
            var gate = new SoundGate(100);
            for (int i = 0; i < 20; i++)
            {
                gate.Started(_shot, 0f);
            }

            Assert.IsTrue(gate.CanPlay(_shot, 0, 0f, 0f));
        }

        [Test]
        public void Gate_Cooldown_UsesLastStart()
        {
            var gate = new SoundGate(8);
            gate.Started(_shot, 1f);

            Assert.IsFalse(gate.CanPlay(_shot, 0, 0.25f, 1.125f));
            Assert.IsTrue(gate.CanPlay(_shot, 0, 0.25f, 1.25f), "Tam bekleme sonunda çalabilir.");
            Assert.IsTrue(gate.CanPlay(_shot, 0, 0f, 1f), "Bekleme 0 = yok.");
        }

        [Test]
        public void Gate_GlobalCap_CountsAllSounds()
        {
            var gate = new SoundGate(2);
            gate.Started(_shot, 0f);
            gate.Started(_coin, 0f);

            Assert.AreEqual(2, gate.ActiveCount);
            Assert.IsFalse(gate.CanPlay(new object(), 0, 0f, 5f));

            gate.Finished(_coin);

            Assert.AreEqual(1, gate.ActiveCount);
            Assert.IsTrue(gate.CanPlay(new object(), 0, 0f, 5f));
        }

        [Test]
        public void Gate_FinishedWithoutStart_IsIgnored()
        {
            var gate = new SoundGate(4);
            gate.Finished(_shot);
            gate.Started(_coin, 0f);
            gate.Finished(_coin);
            gate.Finished(_coin);

            Assert.AreEqual(0, gate.ActiveCount);
            Assert.AreEqual(0, gate.GetActive(_coin));
        }

        [Test]
        public void Gate_MaxVoices_AtLeastOne()
        {
            Assert.AreEqual(1, new SoundGate(0).MaxVoices);
        }

        // --- VoiceTimers ---

        [Test]
        public void Timers_ExpireInOrderOfDuration()
        {
            var timers = new VoiceTimers<string>();
            var expired = new List<VoiceTimers<string>.Entry>();
            timers.Add("a", _shot, 0.5f);
            timers.Add("b", _coin, 1f);
            timers.Add("c", _shot, 0.25f);

            timers.Tick(0.375f, expired);

            Assert.AreEqual(1, expired.Count);
            Assert.AreEqual("c", expired[0].Voice);
            Assert.AreSame(_shot, expired[0].Sound);
            Assert.AreEqual(2, timers.Count);

            expired.Clear();
            timers.Tick(0.125f, expired);

            Assert.AreEqual(1, expired.Count, "Tam sıfırda dolar.");
            Assert.AreEqual("a", expired[0].Voice);

            expired.Clear();
            timers.Tick(0.375f, expired);
            Assert.AreEqual(0, expired.Count);
            Assert.AreEqual(1, timers.Count);

            timers.Tick(0.25f, expired);
            Assert.AreEqual("b", expired[0].Voice);
            Assert.AreEqual(0, timers.Count);
        }

        [Test]
        public void Timers_SwapRemoval_KeepsOthersTicking()
        {
            var timers = new VoiceTimers<string>();
            var expired = new List<VoiceTimers<string>.Entry>();
            timers.Add("long", _shot, 2f);
            timers.Add("short", _shot, 0.125f);
            timers.Add("mid", _coin, 1f);

            timers.Tick(0.5f, expired);
            expired.Clear();
            timers.Tick(0.625f, expired);

            Assert.AreEqual(1, expired.Count);
            Assert.AreEqual("mid", expired[0].Voice, "Yer değiştiren kopya aynı karede iki kez sayılmadı (1 - 1.125 < 0).");
            Assert.AreEqual(1, timers.Count);
        }

        [Test]
        public void Timers_Clear_ReturnsAll()
        {
            var timers = new VoiceTimers<string>();
            var removed = new List<VoiceTimers<string>.Entry>();
            timers.Add("a", _shot, 1f);
            timers.Add("b", _coin, 1f);

            timers.Clear(removed);

            Assert.AreEqual(2, removed.Count);
            Assert.AreEqual(0, timers.Count);
            Assert.AreEqual(1f, removed[0].Remaining, Tolerance);
        }
    }
}
