using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Countdown: süre dolunca bir kez true; durdurulunca bitmiş sayılmaz.</summary>
    public class CountdownTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Tick_FinishesOnceWhenDurationElapses()
        {
            var countdown = new Countdown();
            countdown.Start(3f);

            Assert.IsFalse(countdown.Tick(2f));
            Assert.AreEqual(1f, countdown.Remaining, Tolerance);
            Assert.AreEqual(2f / 3f, countdown.Progress, Tolerance);

            Assert.IsTrue(countdown.Tick(1.5f));
            Assert.IsFalse(countdown.IsRunning);
            Assert.AreEqual(0f, countdown.Remaining);
            Assert.IsFalse(countdown.Tick(1f), "Bittikten sonra tekrar true dönmez.");
        }

        [Test]
        public void NotStarted_NeverFinishes()
        {
            var countdown = new Countdown();

            Assert.IsFalse(countdown.IsRunning);
            Assert.IsFalse(countdown.Tick(10f));
        }

        [Test]
        public void Stop_PreventsFinish()
        {
            var countdown = new Countdown();
            countdown.Start(1f);

            countdown.Stop();

            Assert.IsFalse(countdown.IsRunning);
            Assert.IsFalse(countdown.Tick(2f));
        }

        [Test]
        public void Start_Restarts()
        {
            var countdown = new Countdown();
            countdown.Start(1f);
            countdown.Tick(0.9f);

            countdown.Start(2f);

            Assert.AreEqual(2f, countdown.Remaining);
            Assert.IsFalse(countdown.Tick(1.5f));
        }

        [Test]
        public void ZeroOrNegativeDuration_FinishesOnFirstTick()
        {
            var countdown = new Countdown();
            countdown.Start(-1f);

            Assert.AreEqual(0f, countdown.Duration);
            Assert.AreEqual(1f, countdown.Progress);
            Assert.IsTrue(countdown.Tick(0f));
        }
    }
}
