using LastHaven.Building;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// PowerOutageSchedule (GDD §14 Fırtına: her 25 sn'de 5 sn kesinti): ilk periyotta yok, 25–30 / 50–55 kesik, durdurunca biter;
    /// PowerRule kesintide gücü keser.
    /// </summary>
    public class PowerOutageScheduleTests
    {
        [TestCase(0f, false)]
        [TestCase(24.9f, false)]
        [TestCase(25f, true)]
        [TestCase(29.9f, true)]
        [TestCase(30f, false)]
        [TestCase(49.9f, false)]
        [TestCase(52f, true)]
        [TestCase(55.1f, false)]
        public void IsOutAt_StormPattern(float elapsed, bool expected)
        {
            Assert.AreEqual(expected, PowerOutageSchedule.IsOutAt(elapsed, 25f, 5f));
        }

        [Test]
        public void IsOutAt_ZeroOrLongDuration()
        {
            Assert.IsFalse(PowerOutageSchedule.IsOutAt(60f, 25f, 0f), "Süre 0: kesinti yok.");
            Assert.IsFalse(PowerOutageSchedule.IsOutAt(60f, 0f, 5f), "Periyot 0: kesinti yok.");
            Assert.IsTrue(PowerOutageSchedule.IsOutAt(40f, 10f, 15f), "Süre periyottan uzun: ilk periyottan sonra hep kesik.");
            Assert.IsFalse(PowerOutageSchedule.IsOutAt(9f, 10f, 15f));
        }

        [Test]
        public void Tick_ReportsChangesOnly()
        {
            var schedule = new PowerOutageSchedule();
            schedule.Start(25f, 5f);

            Assert.IsTrue(schedule.IsRunning);
            Assert.IsFalse(schedule.Tick(20f));
            Assert.IsTrue(schedule.Tick(5f), "25. sn: kesinti başlar.");
            Assert.IsTrue(schedule.IsOut);
            Assert.IsFalse(schedule.Tick(2f), "Sürerken değişiklik yok.");
            Assert.IsTrue(schedule.Tick(3f), "30. sn: güç döner.");
            Assert.IsFalse(schedule.IsOut);
        }

        [Test]
        public void Stop_EndsOutage_AndNotRunningIgnoresTicks()
        {
            var schedule = new PowerOutageSchedule();
            Assert.IsFalse(schedule.Tick(100f), "Başlamadan çalışmaz.");

            schedule.Start(25f, 5f);
            schedule.Tick(26f);
            Assert.IsTrue(schedule.Stop(), "Kesinti sürüyordu.");
            Assert.IsFalse(schedule.IsOut);
            Assert.IsFalse(schedule.IsRunning);
            Assert.IsFalse(schedule.Tick(25f));
            Assert.IsFalse(schedule.Stop(), "Zaten bitmiş.");
        }

        [Test]
        public void Start_Restarts()
        {
            var schedule = new PowerOutageSchedule();
            schedule.Start(25f, 5f);
            schedule.Tick(26f);

            schedule.Start(25f, 5f);

            Assert.IsFalse(schedule.IsOut);
            Assert.IsFalse(schedule.Tick(10f), "Yeni gecede saat baştan.");
        }

        [Test]
        public void PowerRule_OutageCutsPower()
        {
            Assert.IsFalse(PowerRule.IsPowered(true, 50, outage: true));
            Assert.IsTrue(PowerRule.IsPowered(true, 50, outage: false));
        }
    }
}
