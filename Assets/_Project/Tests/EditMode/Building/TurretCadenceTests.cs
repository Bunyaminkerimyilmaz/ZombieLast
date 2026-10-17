using LastHaven.Building;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Taretin atış zamanlayıcısı: aralık, FPS'ten bağımsız atış hızı (devreden süre), takılmadan sonra seri atış yok.</summary>
    public class TurretCadenceTests
    {
        [Test]
        public void New_IsReady()
        {
            Assert.IsTrue(new TurretCadence().IsReady);
        }

        [Test]
        public void TryFire_WaitsForInterval()
        {
            var cadence = new TurretCadence();

            Assert.IsTrue(cadence.TryFire(0.2f));
            Assert.IsFalse(cadence.TryFire(0.2f), "Bekleme süresi dolmadı.");

            cadence.Tick(0.19f);
            Assert.IsFalse(cadence.IsReady);

            cadence.Tick(0.02f);
            Assert.IsTrue(cadence.TryFire(0.2f));
        }

        [TestCase(60f)]
        [TestCase(144f)]
        [TestCase(30f)]
        public void FireRate_IsIndependentOfFrameRate(float fps)
        {
            // 6.5 atış/sn (3. seviye): her karede ateş isteği, 10 sn.
            var cadence = new TurretCadence();
            float interval = 1f / 6.5f;
            float deltaTime = 1f / fps;
            int frames = UnityEngine.Mathf.RoundToInt(10f * fps);
            int shots = 0;
            for (int i = 0; i < frames; i++)
            {
                cadence.Tick(deltaTime);
                if (cadence.TryFire(interval))
                {
                    shots++;
                }
            }

            Assert.That(shots, Is.InRange(64, 66), $"{fps} FPS");
        }

        [Test]
        public void LongHitch_DoesNotBankShots()
        {
            var cadence = new TurretCadence();
            cadence.TryFire(0.2f);

            cadence.Tick(5f);

            // Devreden süre en fazla bir aralık: 5 sn'lik takılma 25 atış biriktirmez, sadece bir atışlık pay.
            Assert.IsTrue(cadence.TryFire(0.2f));
            Assert.IsTrue(cadence.TryFire(0.2f), "Devreden bir aralık.");
            Assert.IsFalse(cadence.TryFire(0.2f), "Takılmadan sonra seri atış yok.");
            cadence.Tick(0.19f);
            Assert.IsFalse(cadence.IsReady);
        }

        [Test]
        public void Reset_MakesReady()
        {
            var cadence = new TurretCadence();
            cadence.TryFire(1f);

            cadence.Reset();

            Assert.IsTrue(cadence.IsReady);
        }
    }
}
