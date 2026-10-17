using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Gece olayı üreticisi: GDD §14 değerleri, id'ler gecelerin bağladıklarıyla aynı; asset'ler gecelere bağlı (Rebuild All sonrası).</summary>
    public class NightModifierGenerationTests
    {
        private const float Tolerance = 0.0001f;

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [Test]
        public void Ids_MatchNightDefinitions()
        {
            Assert.AreEqual(NightDefinitions.BloodMoonId, NightModifierDefinitions.BloodMoon.Id);
            Assert.AreEqual(NightDefinitions.FogId, NightModifierDefinitions.Fog.Id);
            Assert.AreEqual(NightDefinitions.StormId, NightModifierDefinitions.Storm.Id);
        }

        [Test]
        public void BloodMoon_MatchesGdd()
        {
            NightModifierSettings settings = Create(NightModifierDefinitions.BloodMoon).ToSettings();

            Assert.AreEqual("Kan Ayı", settings.DisplayName);
            Assert.IsNotEmpty(settings.Description);
            Assert.AreEqual(1.5f, settings.EnemyCountMultiplier, Tolerance, "Düşman sayısı ×1.5.");
            Assert.AreEqual(2f, settings.CoinMultiplier, Tolerance, "Jeton değeri ×2.");
            Assert.AreEqual(1f, settings.EnemySpeedMultiplier, Tolerance);
            Assert.IsTrue(settings.OverridesLight, "Kırmızı ay ışığı.");
            Assert.Greater(settings.LightColor.r, settings.LightColor.g * 3f, "Kırmızı.");
            Assert.IsFalse(settings.OverridesFog);
            Assert.IsFalse(settings.HasPowerOutages);
        }

        [Test]
        public void Fog_MatchesGdd()
        {
            NightModifierSettings settings = Create(NightModifierDefinitions.Fog).ToSettings();

            Assert.AreEqual("Sis", settings.DisplayName);
            Assert.AreEqual(1.1f, settings.EnemySpeedMultiplier, Tolerance, "Düşman hızı +%10.");
            Assert.IsTrue(settings.OverridesFog, "Yoğun sis.");
            Assert.Greater(settings.FogDensity, 0.02f, "Gece sisinden (0.02) yoğun.");
            Assert.AreEqual(1f, settings.EnemyCountMultiplier, Tolerance);
            Assert.AreEqual(1f, settings.CoinMultiplier, Tolerance);
            Assert.IsFalse(settings.OverridesLight);
        }

        [Test]
        public void Storm_MatchesGdd()
        {
            NightModifierSettings settings = Create(NightModifierDefinitions.Storm).ToSettings();

            Assert.AreEqual("Fırtına", settings.DisplayName);
            Assert.IsTrue(settings.HasPowerOutages);
            Assert.AreEqual(25f, settings.OutageInterval, Tolerance, "Her 25 sn'de.");
            Assert.AreEqual(5f, settings.OutageDuration, Tolerance, "5 sn kesinti.");
            Assert.IsFalse(settings.OverridesFog);
            Assert.IsFalse(settings.OverridesLight);
        }

        [TestCase("Night04", "fog")]
        [TestCase("Night05", "blood_moon")]
        [TestCase("Night07", "storm")]
        [TestCase("Night09", "fog")]
        [TestCase("Night10", "blood_moon")]
        public void Assets_NightsHaveGddEvents(string nightAsset, string modifierId)
        {
            var night = AssetDatabase.LoadAssetAtPath<NightData>($"{WavesGenerator.NightsFolder}/{nightAsset}.asset");
            if (night == null)
            {
                Assert.Inconclusive("Gece verisi yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.IsNotNull(night.Modifier, $"{nightAsset}: GDD §8 olayı bağlı değil.");
            Assert.AreEqual(modifierId, night.Modifier.Id);
        }

        private NightModifierData Create(NightModifierDefinition definition)
        {
            var data = ScriptableObject.CreateInstance<NightModifierData>();
            _created.Add(data);
            NightModifiersGenerator.Apply(data, definition);
            return data;
        }
    }
}
