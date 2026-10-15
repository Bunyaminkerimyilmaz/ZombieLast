using LastHaven.Economy;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>EconomyConfig varsayılanları (GDD §10) ve üretilen asset (7.1).</summary>
    public class EconomyConfigTests
    {
        private const float Tolerance = 0.0001f;

        private EconomyConfig _config;

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
            _config = ScriptableObject.CreateInstance<EconomyConfig>();

            Assert.AreEqual(300, _config.StartingMoney);
            Assert.AreEqual(100, _config.StartingFuel);
            Assert.AreEqual(0.1f, _config.InterestRate, Tolerance);
            Assert.AreEqual(50, _config.InterestCap);
            Assert.AreEqual(12f, _config.CoinLifetime, Tolerance);
            Assert.AreEqual(3f, _config.CoinBlinkDuration, Tolerance);
            Assert.AreEqual(2.5f, _config.ComboWindow, Tolerance);
            Assert.AreEqual(0.5f, _config.SellRefundRate, Tolerance);
        }

        [Test]
        public void ComboTiers_MatchGdd_InAscendingOrder()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();

            Assert.AreEqual(3, _config.ComboTiers.Count);
            AssertTier(_config.ComboTiers[0], 5, 1.5f);
            AssertTier(_config.ComboTiers[1], 15, 2f);
            AssertTier(_config.ComboTiers[2], 30, 3f);
        }

        [Test]
        public void CoinDefaults_SplitOneToFive()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();

            Assert.AreEqual(1, _config.MinCoinsPerDrop);
            Assert.AreEqual(5, _config.MaxCoinsPerDrop);
            Assert.Greater(_config.TargetCoinValue, 0);
            Assert.Greater(_config.CoinPrewarm, 0);
            Assert.IsNotNull(_config.CoinMotion);
            Assert.Less(_config.CoinBlinkDuration, _config.CoinLifetime);
        }

        [Test]
        public void CreateCoinSplit_CopiesValues()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();

            CoinSplit split = _config.CreateCoinSplit();

            Assert.AreEqual(_config.MinCoinsPerDrop, split.MinCoins);
            Assert.AreEqual(_config.MaxCoinsPerDrop, split.MaxCoins);
            Assert.AreEqual(_config.TargetCoinValue, split.TargetCoinValue);
            Assert.AreEqual(2, split.GetCount(10), "Yürüyen (10 para) 2 jeton.");
        }

        [Test]
        public void CoinMotionDefaults_AreConsistent()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();
            CoinMotionSettings motion = _config.CoinMotion;

            Assert.LessOrEqual(motion.ScatterMinDistance, motion.ScatterMaxDistance);
            Assert.Less(motion.ScatterMaxDistance, 3f, "Saçılma GDD §5 mıknatıs yarıçapından (3 m) kısa olmalı.");
            Assert.LessOrEqual(motion.AttractStartSpeed, motion.AttractMaxSpeed);
            Assert.Greater(motion.AttractMaxSpeed, 6f, "Uçan jeton oyuncunun yürüme hızını (6 m/sn) geçmeli.");
            Assert.Greater(motion.BlinkFrequency, 0f);
        }

        [Test]
        public void CreateComboTracker_UsesWindowAndTiers()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();

            ComboTracker tracker = _config.CreateComboTracker();

            Assert.AreEqual(_config.ComboWindow, tracker.Window, Tolerance);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(1f, tracker.RegisterKill(), Tolerance);
            }

            Assert.AreEqual(1.5f, tracker.RegisterKill(), Tolerance, "5. öldürme ×1.5.");
        }

        [Test]
        public void CreateInterestRule_CopiesValues()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();

            InterestRule rule = _config.CreateInterestRule();

            Assert.AreEqual(_config.InterestRate, rule.Rate, Tolerance);
            Assert.AreEqual(_config.InterestCap, rule.Cap);
            Assert.AreEqual(30, rule.Calculate(_config.StartingMoney));
        }

        [Test]
        public void Asset_Exists()
        {
            var asset = AssetDatabase.LoadAssetAtPath<EconomyConfig>(EconomyGenerator.ConfigPath);
            if (asset == null)
            {
                Assert.Inconclusive($"{EconomyGenerator.ConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.Greater(asset.ComboTiers.Count, 0);
        }

        private static void AssertTier(ComboTier tier, int kills, float multiplier)
        {
            Assert.AreEqual(kills, tier.Kills);
            Assert.AreEqual(multiplier, tier.Multiplier, Tolerance);
        }
    }
}
