using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Vuruş donması (14.1): saf <see cref="HitStopTimer"/> (uzatır, kısaltmaz) ve <see cref="PauseService"/>'te duraklatmayla
    /// birlikte çalışması (zaman ölçeğinin tek sahibi; duraklatma donmayı yutar, duraklıyken donma istenmez).
    /// </summary>
    public class HitStopTests
    {
        private GameObject _host;
        private PauseService _service;
        private float _savedTimeScale;

        [SetUp]
        public void SetUp()
        {
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            _host = new GameObject("PauseServiceTest");
            _service = _host.AddComponent<PauseService>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Time.timeScale = _savedTimeScale;
        }

        [Test]
        public void Timer_StartsOnFirstRequest_EndsOnceAfterDuration()
        {
            var timer = new HitStopTimer();

            Assert.IsTrue(timer.Request(0.06f), "İlk istek donmayı başlatır.");
            Assert.IsTrue(timer.IsActive);
            Assert.IsFalse(timer.Tick(0.05f));
            Assert.IsTrue(timer.Tick(0.02f), "Süre dolunca bir kez true.");
            Assert.IsFalse(timer.IsActive);
            Assert.IsFalse(timer.Tick(0.1f));
        }

        [Test]
        public void Timer_OverlappingRequest_ExtendsButNeverShortens()
        {
            var timer = new HitStopTimer();
            timer.Request(0.1f);
            timer.Tick(0.05f);

            Assert.IsFalse(timer.Request(0.02f), "Sürerken gelen istek donmayı yeniden başlatmaz.");
            Assert.AreEqual(0.05f, timer.Remaining, 0.0001f, "Kısa istek kalanı kısaltmaz.");
            timer.Request(0.08f);
            Assert.AreEqual(0.08f, timer.Remaining, 0.0001f, "Uzun istek kalanı uzatır.");
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Timer_NonPositiveDuration_Ignored(float duration)
        {
            var timer = new HitStopTimer();

            Assert.IsFalse(timer.Request(duration));
            Assert.IsFalse(timer.IsActive);
        }

        [Test]
        public void Service_HitStop_FreezesThenRestoresTimeScale()
        {
            Time.timeScale = 0.8f;

            _service.HitStop(0.06f);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(_service.IsHitStopped);
            Assert.IsFalse(_service.IsPaused, "Donma duraklatma sayılmaz.");

            _service.Advance(0.1f);
            Assert.AreEqual(0.8f, Time.timeScale, 0.0001f, "Donma bitince önceki ölçek döner.");
            Assert.IsFalse(_service.IsHitStopped);
        }

        [Test]
        public void Service_HitStopWhilePaused_Ignored()
        {
            var cards = new object();
            _service.Pause(cards);

            _service.HitStop(0.06f);
            Assert.IsFalse(_service.IsHitStopped);

            _service.Resume(cards);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void Service_PauseDuringHitStop_KeepsFrozenUntilResume_RestoresOriginalScale()
        {
            Time.timeScale = 0.8f;
            var cards = new object();
            _service.HitStop(0.06f);

            _service.Pause(cards);
            Assert.IsFalse(_service.IsHitStopped, "Duraklatma donmayı yutar.");
            _service.Advance(1f);
            Assert.AreEqual(0f, Time.timeScale, "Donmanın süresi dolsa da duraklatma sürer.");

            _service.Resume(cards);
            Assert.AreEqual(0.8f, Time.timeScale, 0.0001f, "Donma öncesi ölçek geri gelir (0 değil).");
        }
    }
}
