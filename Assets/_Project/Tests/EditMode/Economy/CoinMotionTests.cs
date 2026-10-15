using LastHaven.Economy;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// CoinMotion (7.2): yay çizerek saçılma, yerde dönme, ömür (GDD §10: 12 sn, son 3 sn yanıp söner) ve mıknatıs
    /// (yarıçapa girince hızlanarak uçar, değince toplanır).
    /// </summary>
    public class CoinMotionTests
    {
        private const float Tolerance = 0.001f;
        private const float Lifetime = 12f;
        private const float BlinkDuration = 3f;
        private const float Step = 0.02f;
        private const float Radius = 3f;

        private static readonly Vector3 Start = new Vector3(0f, 0.8f, 0f);
        private static readonly Vector3 Landing = new Vector3(1f, 0.3f, 0f);

        private CoinMotionSettings _settings;
        private CoinMotion _motion;

        private static MagnetTarget FarAway => new MagnetTarget(new Vector3(50f, 1f, 0f), Radius, true);

        [SetUp]
        public void SetUp()
        {
            _settings = new CoinMotionSettings();
            _motion = new CoinMotion();
            _motion.Launch(_settings, Start, Landing, Lifetime, BlinkDuration, 30f);
        }

        [Test]
        public void Launch_StartsScatteringAtStart()
        {
            Assert.AreEqual(CoinState.Scattering, _motion.State);
            Assert.AreEqual(Start, _motion.Position);
            Assert.AreEqual(30f, _motion.SpinAngle, Tolerance);
            Assert.IsTrue(_motion.IsVisible);
            Assert.AreEqual(Lifetime, _motion.RemainingLifetime, Tolerance);
        }

        [Test]
        public void Scatter_ArcsAboveStraightLine_ThenLands()
        {
            Assert.AreEqual(CoinStep.None, _motion.Tick(_settings.ArcDuration * 0.5f, FarAway));

            Vector3 straight = Vector3.Lerp(Start, Landing, 0.5f);
            Assert.AreEqual(CoinState.Scattering, _motion.State);
            Assert.AreEqual(straight.x, _motion.Position.x, Tolerance);
            Assert.AreEqual(straight.y + _settings.ArcHeight, _motion.Position.y, Tolerance, "Yayın tepesi ortada.");

            _motion.Tick(_settings.ArcDuration * 0.5f, FarAway);

            Assert.AreEqual(CoinState.Resting, _motion.State);
            Assert.AreEqual(Landing, _motion.Position);
        }

        [Test]
        public void Resting_SpinsInPlace()
        {
            Land();
            Vector3 position = _motion.Position;
            float angle = _motion.SpinAngle;

            _motion.Tick(0.1f, FarAway);

            Assert.AreEqual(position, _motion.Position);
            Assert.AreEqual(Mathf.Repeat(angle + _settings.SpinSpeed * 0.1f, 360f), _motion.SpinAngle, Tolerance);
        }

        [Test]
        public void Lifetime_ExpiresAfterTwelveSeconds()
        {
            Assert.AreEqual(CoinStep.None, Run(Lifetime - 0.1f, FarAway));
            Assert.AreEqual(CoinState.Resting, _motion.State);

            Assert.AreEqual(CoinStep.Expired, Run(0.2f, FarAway));
            Assert.AreEqual(CoinState.Inactive, _motion.State);
            Assert.AreEqual(CoinStep.None, _motion.Tick(Step, FarAway), "Sonuç bir kez gelir.");
        }

        [Test]
        public void Blink_OnlyInLastThreeSeconds()
        {
            // Kalan ömür 3 sn'nin biraz üstüne inene kadar hep görünür.
            while (_motion.RemainingLifetime > BlinkDuration + Step * 2f)
            {
                _motion.Tick(Step, FarAway);
                Assert.IsTrue(_motion.IsVisible, $"Kalan {_motion.RemainingLifetime:0.00} sn'de görünmeli.");
                Assert.IsFalse(_motion.IsBlinking);
            }

            int hidden = 0;
            int shown = 0;
            while (_motion.Tick(Step, FarAway) == CoinStep.None)
            {
                if (!_motion.IsBlinking)
                {
                    continue;
                }

                if (_motion.IsVisible)
                {
                    shown++;
                }
                else
                {
                    hidden++;
                }
            }

            Assert.Greater(hidden, 0, "Son 3 sn'de kapalı kareler olmalı.");
            Assert.Greater(shown, 0, "Son 3 sn'de açık kareler olmalı.");
        }

        [Test]
        public void Magnet_InRadius_AttractsAfterLanding()
        {
            var magnet = new MagnetTarget(new Vector3(2.5f, 1f, 0f), Radius, true);

            _motion.Tick(Step, magnet);
            Assert.AreEqual(CoinState.Scattering, _motion.State, "Yay sırasında mıknatısa kapılmaz.");

            Land(magnet);
            _motion.Tick(Step, magnet);

            Assert.AreEqual(CoinState.Attracted, _motion.State);
            Assert.Greater(_motion.Speed, 0f);
        }

        [Test]
        public void Magnet_OutOfRadius_StaysResting()
        {
            var magnet = new MagnetTarget(new Vector3(Landing.x + Radius + 0.1f, 1f, 0f), Radius, true);
            Land(magnet);

            _motion.Tick(Step, magnet);

            Assert.AreEqual(CoinState.Resting, _motion.State);
        }

        [Test]
        public void Magnet_RadiusIsHorizontal()
        {
            var magnet = new MagnetTarget(new Vector3(Landing.x, 20f, Landing.z), Radius, true);
            Land(magnet);

            _motion.Tick(Step, magnet);

            Assert.AreEqual(CoinState.Attracted, _motion.State, "Yükseklik farkı yarıçapa sayılmaz.");
        }

        [Test]
        public void Magnet_DeadPlayer_DoesNotAttract()
        {
            var magnet = new MagnetTarget(new Vector3(2f, 1f, 0f), Radius, false);
            Land(magnet);

            _motion.Tick(Step, magnet);

            Assert.AreEqual(CoinState.Resting, _motion.State);
        }

        [Test]
        public void Attracted_Accelerates_ThenCollected()
        {
            var magnet = new MagnetTarget(new Vector3(Landing.x + 2.9f, 1f, 0f), Radius, true);
            Land(magnet);

            _motion.Tick(Step, magnet);
            float firstSpeed = _motion.Speed;
            _motion.Tick(Step, magnet);

            Assert.Greater(_motion.Speed, firstSpeed, "Uçarken hızlanmalı.");
            Assert.AreEqual(CoinStep.Collected, Run(2f, magnet));
            Assert.AreEqual(CoinState.Inactive, _motion.State);
            Assert.AreEqual(magnet.Position, _motion.Position, "Toplandığı yer oyuncunun toplama noktası.");
        }

        [Test]
        public void Attracted_SpeedCappedAtMax()
        {
            var magnet = new MagnetTarget(new Vector3(Landing.x + 2.9f, 1f, 0f), Radius, true);
            Land(magnet);
            _motion.Tick(Step, magnet);

            // Hedef hızla uzaklaşır: jeton yetişemez, hız tavana dayanır.
            for (int i = 0; i < 100 && _motion.State == CoinState.Attracted; i++)
            {
                var moving = new MagnetTarget(magnet.Position + Vector3.right * (i * 1f), Radius, true);
                _motion.Tick(Step, moving);
            }

            Assert.AreEqual(_settings.AttractMaxSpeed, _motion.Speed, Tolerance);
        }

        [Test]
        public void Attracted_DoesNotExpire_AndIsVisible()
        {
            Run(Lifetime - 1f, FarAway); // yanıp sönüyor
            var magnet = new MagnetTarget(new Vector3(Landing.x + 2.9f, 1f, 0f), Radius, true);
            _motion.Tick(Step, magnet);
            Assert.AreEqual(CoinState.Attracted, _motion.State);

            // Ömür bitmiş olsa da uçan jeton varır.
            var dead = new MagnetTarget(magnet.Position, Radius, false);
            CoinStep result = Run(5f, dead);

            Assert.AreEqual(CoinStep.Collected, result);
        }

        [Test]
        public void Attracted_IsVisibleWhileFlying()
        {
            Run(Lifetime - 1f, FarAway);
            var magnet = new MagnetTarget(new Vector3(Landing.x + 2.9f, 1f, 0f), Radius, true);

            _motion.Tick(Step, magnet);

            Assert.IsTrue(_motion.IsVisible);
            Assert.IsFalse(_motion.IsBlinking);
        }

        [Test]
        public void Stop_MakesInactive_TickDoesNothing()
        {
            _motion.Stop();

            Assert.AreEqual(CoinState.Inactive, _motion.State);
            Assert.AreEqual(CoinStep.None, _motion.Tick(Lifetime * 2f, FarAway));
            Assert.IsTrue(_motion.IsVisible);
        }

        [Test]
        public void Launch_Again_Restarts()
        {
            Run(Lifetime + 1f, FarAway);
            Assert.AreEqual(CoinState.Inactive, _motion.State);

            _motion.Launch(_settings, Start, Landing, Lifetime, BlinkDuration);

            Assert.AreEqual(CoinState.Scattering, _motion.State);
            Assert.AreEqual(Lifetime, _motion.RemainingLifetime, Tolerance);
        }

        private void Land()
        {
            Land(FarAway);
        }

        private void Land(MagnetTarget magnet)
        {
            while (_motion.State == CoinState.Scattering)
            {
                _motion.Tick(Step, magnet);
            }

            Assert.AreEqual(CoinState.Resting, _motion.State);
        }

        private CoinStep Run(float seconds, MagnetTarget magnet)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                CoinStep step = _motion.Tick(Step, magnet);
                if (step != CoinStep.None)
                {
                    return step;
                }
            }

            return CoinStep.None;
        }
    }
}
