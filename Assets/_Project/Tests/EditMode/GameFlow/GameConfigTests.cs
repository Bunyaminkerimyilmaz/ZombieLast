using LastHaven.Core;
using LastHaven.GameFlow;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>GameConfig varsayılanları (GDD §3, §5, §9) ve erken hazır bonusu hesabı (GameFlowSettings).</summary>
    public class GameConfigTests
    {
        private const float Tolerance = 0.0001f;

        private GameConfig _config;

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }
        }

        [Test]
        public void Defaults_MatchGdd()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();

            Assert.AreEqual(120f, _config.DayDuration, Tolerance);
            Assert.AreEqual(4f, _config.DuskDuration, Tolerance);
            Assert.AreEqual(15f, _config.FinalWarningDuration, Tolerance);
            Assert.AreEqual(5, _config.EarlyReadyBonusPerStep);
            Assert.AreEqual(10f, _config.EarlyReadyStepSeconds, Tolerance);
            Assert.AreEqual(10f, _config.PlayerRespawnDelay, Tolerance);
            Assert.AreEqual(3f, _config.DawnLightTransitionDuration, Tolerance);
        }

        [Test]
        public void RespawnOffset_IsBesideGenerator()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();
            IGameRules rules = _config;

            Vector3 offset = rules.PlayerRespawnOffset;
            float horizontal = new Vector2(offset.x, offset.z).magnitude;

            // Jeneratör 2×2 m (kenarı merkezden 1 m); oyuncu yarıçapı 0.5 m.
            Assert.Greater(horizontal, 1.5f, "Doğma noktası jeneratörün içinde olmamalı.");
            Assert.LessOrEqual(horizontal, 5f, "Doğma noktası jeneratörün yanında olmalı.");
            Assert.AreEqual(0f, offset.y, Tolerance);
        }

        [Test]
        public void CreateFlowSettings_CopiesValues()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();

            GameFlowSettings settings = _config.CreateFlowSettings();

            Assert.AreEqual(_config.DayDuration, settings.DayDuration, Tolerance);
            Assert.AreEqual(_config.DuskDuration, settings.DuskDuration, Tolerance);
            Assert.AreEqual(_config.FinalWarningDuration, settings.FinalWarningDuration, Tolerance);
            Assert.AreEqual(_config.EarlyReadyBonusPerStep, settings.EarlyReadyBonusPerStep);
            Assert.AreEqual(_config.EarlyReadyStepSeconds, settings.EarlyReadyStepSeconds, Tolerance);
        }

        [TestCase(120f, 60)]
        [TestCase(119.5f, 60)] // HUD'da 120
        [TestCase(119f, 55)]
        [TestCase(110f, 55)]
        [TestCase(109.2f, 55)] // HUD'da 110
        [TestCase(109f, 50)]
        [TestCase(10f, 5)]
        [TestCase(9.5f, 5)] // HUD'da 10
        [TestCase(9f, 0)]
        [TestCase(0f, 0)]
        [TestCase(-5f, 0)]
        public void EarlyReadyBonus_FivePerTenSecondsLeft(float remaining, int expected)
        {
            var settings = new GameFlowSettings(120f, 4f, 15f, 5, 10f);

            Assert.AreEqual(expected, settings.CalculateEarlyReadyBonus(remaining));
        }

        [Test]
        public void EarlyReadyBonus_ZeroStep_GivesNoBonus()
        {
            var settings = new GameFlowSettings(120f, 4f, 15f, 5, 0f);

            Assert.AreEqual(0, settings.CalculateEarlyReadyBonus(120f));
        }

        [Test]
        public void Settings_ClampNegativeValues()
        {
            var settings = new GameFlowSettings(-1f, -2f, -3f, -4, -5f);

            Assert.AreEqual(0f, settings.DayDuration, Tolerance);
            Assert.AreEqual(0f, settings.DuskDuration, Tolerance);
            Assert.AreEqual(0f, settings.FinalWarningDuration, Tolerance);
            Assert.AreEqual(0, settings.EarlyReadyBonusPerStep);
            Assert.AreEqual(0f, settings.EarlyReadyStepSeconds, Tolerance);
        }
    }
}
