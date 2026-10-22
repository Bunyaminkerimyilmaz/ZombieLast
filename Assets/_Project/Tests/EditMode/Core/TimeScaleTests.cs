using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Debug zaman ölçeği (15.1): <see cref="PauseService"/> <see cref="ITimeScaleService"/> olarak normal ölçeği değiştirir; duraklıyken
    /// veya donmuşken yeni ölçek saklanır, devam edince yazılır.
    /// </summary>
    public class TimeScaleTests
    {
        private GameObject _host;
        private PauseService _service;
        private float _savedTimeScale;

        [SetUp]
        public void SetUp()
        {
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            _host = new GameObject("TimeScaleTest");
            _service = _host.AddComponent<PauseService>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Time.timeScale = _savedTimeScale;
        }

        [Test]
        public void SetTimeScale_WhileRunning_AppliesImmediately()
        {
            _service.SetTimeScale(2f);

            Assert.AreEqual(2f, Time.timeScale);
            Assert.AreEqual(2f, _service.TimeScale);
        }

        [Test]
        public void SetTimeScale_WhilePaused_AppliesOnResume()
        {
            var menu = new object();
            _service.Pause(menu);

            _service.SetTimeScale(4f);
            Assert.AreEqual(0f, Time.timeScale, "Duraklıyken oyun donuk kalır.");
            Assert.AreEqual(4f, _service.TimeScale);

            _service.Resume(menu);
            Assert.AreEqual(4f, Time.timeScale);
        }

        [Test]
        public void Pause_KeepsDebugScale()
        {
            _service.SetTimeScale(0.5f);
            var menu = new object();

            _service.Pause(menu);
            _service.Resume(menu);

            Assert.AreEqual(0.5f, Time.timeScale);
        }

        [Test]
        public void SetTimeScale_DuringHitStop_AppliesWhenFreezeEnds()
        {
            _service.HitStop(0.06f);

            _service.SetTimeScale(2f);
            Assert.AreEqual(0f, Time.timeScale);

            _service.Advance(0.1f);
            Assert.AreEqual(2f, Time.timeScale);
        }

        [Test]
        public void SetTimeScale_IgnoresNonPositive()
        {
            _service.SetTimeScale(2f);

            _service.SetTimeScale(0f);
            _service.SetTimeScale(-1f);
            _service.SetTimeScale(float.NaN);

            Assert.AreEqual(2f, Time.timeScale);
            Assert.AreEqual(2f, _service.TimeScale);
        }
    }
}
