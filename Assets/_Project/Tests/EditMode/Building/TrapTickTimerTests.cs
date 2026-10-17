using LastHaven.Building;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Tuzak tik zamanlaması (GDD §11, 0.5 sn): ilk bakış hemen, boşken kısa aralıkla bakılır, doluyken aralıkla tik, taşan süre
    /// devreder ama en fazla bir aralık.
    /// </summary>
    public class TrapTickTimerTests
    {
        private const float Interval = 0.5f;
        private const float IdleScan = 0.1f;

        [Test]
        public void AfterReset_ChecksImmediately()
        {
            var timer = new TrapTickTimer();
            timer.Reset();

            Assert.IsTrue(timer.Advance(0.016f));
        }

        [Test]
        public void WhileOccupied_TicksEveryInterval()
        {
            var timer = new TrapTickTimer();
            int ticks = 0;
            for (int frame = 0; frame < 90; frame++) // 90 × 0.02 = 1.8 sn
            {
                if (timer.Advance(0.02f))
                {
                    timer.MarkTick(Interval);
                    ticks++;
                }
            }

            Assert.AreEqual(4, ticks, "0, 0.5, 1.0, 1.5 sn.");
        }

        [Test]
        public void WhileEmpty_RechecksAfterIdleInterval_AndFirstEntrantTicksAtOnce()
        {
            var timer = new TrapTickTimer();
            Assert.IsTrue(timer.Advance(0.02f));
            timer.MarkIdle(IdleScan);

            Assert.IsFalse(timer.Advance(0.05f));
            Assert.IsTrue(timer.Advance(0.06f), "Boşken 0.1 sn'de bir bakılır.");
            timer.MarkTick(Interval); // biri girdi: hemen tik

            Assert.IsFalse(timer.Advance(0.3f));
            Assert.IsTrue(timer.Advance(0.25f));
        }

        [Test]
        public void LeavingAndReentering_DoesNotShortenInterval()
        {
            var timer = new TrapTickTimer();
            timer.Advance(0.02f);
            timer.MarkTick(Interval);

            Assert.IsFalse(timer.Advance(0.2f), "Çıkıp girse de bekleme sürer.");
            Assert.IsFalse(timer.Advance(0.2f));
            Assert.IsTrue(timer.Advance(0.12f));
        }

        [Test]
        public void LongHitch_CarriesAtMostOneInterval()
        {
            var timer = new TrapTickTimer();
            timer.Advance(0.02f);
            timer.MarkTick(Interval);

            Assert.IsTrue(timer.Advance(3f));
            timer.MarkTick(Interval);

            Assert.LessOrEqual(timer.Cooldown, Interval);
            Assert.GreaterOrEqual(timer.Cooldown, 0f, "Birikmiş tikler arka arkaya atılmaz.");
            Assert.IsTrue(timer.Advance(0.02f), "En fazla bir tik hemen.");
            timer.MarkTick(Interval);
            Assert.IsFalse(timer.Advance(0.02f));
        }
    }
}
