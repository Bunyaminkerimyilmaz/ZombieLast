using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>ProgressionConfig varsayılanları (GDD §13) ve üretilen asset (13.1).</summary>
    public class ProgressionConfigTests
    {
        private ProgressionConfig _config;

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
            _config = ScriptableObject.CreateInstance<ProgressionConfig>();

            Assert.AreEqual(10, _config.BaseXp);
            Assert.AreEqual(6, _config.XpPerLevel);
            Assert.AreEqual(1, _config.StartingLevel);
            Assert.IsFalse(_config.PlayerKillsOnly, "GDD: düşman öldürünce otomatik kazanılır (kaynak fark etmez).");
        }

        [TestCase(PerkRarity.Common, 60)]
        [TestCase(PerkRarity.Rare, 30)]
        [TestCase(PerkRarity.Epic, 10)]
        public void RarityWeights_MatchGdd(PerkRarity rarity, int expected)
        {
            _config = ScriptableObject.CreateInstance<ProgressionConfig>();

            Assert.AreEqual(expected, _config.GetRarityWeight(rarity));
            Assert.AreEqual(3, _config.RarityWeights.Count);
        }

        [Test]
        public void GetRarityWeight_UnknownRarity_IsZero()
        {
            _config = ScriptableObject.CreateInstance<ProgressionConfig>();

            Assert.AreEqual(0, _config.GetRarityWeight((PerkRarity)99));
        }

        [Test]
        public void CreateExperienceTracker_UsesCurveAndStartingLevel()
        {
            _config = ScriptableObject.CreateInstance<ProgressionConfig>();

            ExperienceTracker tracker = _config.CreateExperienceTracker();

            Assert.AreEqual(1, tracker.Level);
            Assert.AreEqual(16, tracker.XpToNextLevel);
            Assert.AreEqual(22, _config.CreateLevelCurve().XpToNext(2));
        }

        [Test]
        public void Asset_Exists()
        {
            var asset = AssetDatabase.LoadAssetAtPath<ProgressionConfig>(ProgressionGenerator.ConfigPath);
            if (asset == null)
            {
                Assert.Inconclusive($"{ProgressionGenerator.ConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.Greater(asset.CreateLevelCurve().XpToNext(asset.StartingLevel), 0);
            Assert.Greater(asset.RarityWeights.Count, 0);
        }
    }
}
