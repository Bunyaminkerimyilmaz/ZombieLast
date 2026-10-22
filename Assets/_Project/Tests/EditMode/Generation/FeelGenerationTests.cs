using LastHaven.Editor;
using LastHaven.Feedback;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Oyun hissi asset'leri (14.1): <see cref="FeedbackConfig"/> ilk oluşturmada pompalı ve ağır düşman listeleriyle gelir; kırmızı
    /// ekran kenarı profili sadece Vignette'i override eder. Asset oluşturmaz (yoksa Inconclusive).
    /// </summary>
    public class FeelGenerationTests
    {
        [Test]
        public void FeedbackConfigAsset_HasShotgunShakeAndHeavyEnemies()
        {
            var config = AssetDatabase.LoadAssetAtPath<FeedbackConfig>(FeedbackFeelAssets.ConfigPath);
            if (config == null)
            {
                Assert.Inconclusive($"{FeedbackFeelAssets.ConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.Greater(config.GetShotShake(WeaponsGenerator.ShotgunId), config.GetShotShake("pistol"), "Pompalı tabancadan sert.");
            Assert.IsTrue(config.IsHeavy(EnemyDefinitions.Tank.Id));
            Assert.IsTrue(config.IsHeavy(NightDefinitions.BruteId));
            Assert.IsTrue(config.IsHeavy(NightDefinitions.BruteFinalId));
            Assert.IsFalse(config.IsHeavy(EnemyDefinitions.Walker.Id));
            Assert.Greater(config.GetKillShake(true, true), config.GetKillShake(false, true), "Ağır ölüm normalden sert.");
            Assert.Greater(config.GetKillHitStop(true, true), 0f, "Ağır ölümde vuruş donması.");
        }

        [Test]
        public void Populate_AddsOnlyRedVignette()
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            try
            {
                FeedbackFeelAssets.Populate(profile);

                Assert.AreEqual(1, profile.components.Count, "Diğer efektler gün/gece profillerinde kalır.");
                Assert.IsTrue(profile.TryGet(out Vignette vignette));
                AssertRedVignette(vignette);
            }
            finally
            {
                foreach (VolumeComponent component in profile.components)
                {
                    Object.DestroyImmediate(component);
                }

                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void VignetteProfileAsset_HasRedVignette()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(FeedbackFeelAssets.VignetteProfilePath);
            if (profile == null)
            {
                Assert.Inconclusive($"{FeedbackFeelAssets.VignetteProfilePath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.IsTrue(profile.TryGet(out Vignette vignette), "Profilde Vignette yok.");
            AssertRedVignette(vignette);
        }

        private static void AssertRedVignette(Vignette vignette)
        {
            Assert.IsTrue(vignette.color.overrideState);
            Assert.IsTrue(vignette.intensity.overrideState);
            Color color = vignette.color.value;
            Assert.Greater(color.r, color.g * 4f, "Kırmızı kenar.");
            Assert.Greater(color.r, color.b * 4f);
            Assert.Greater(vignette.intensity.value, LightingDefinitions.NightPostProcess.VignetteIntensity,
                "Gece kenarından belirgin.");
        }
    }
}
