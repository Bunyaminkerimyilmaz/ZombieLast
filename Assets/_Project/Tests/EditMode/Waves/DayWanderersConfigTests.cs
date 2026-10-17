using LastHaven.Editor;
using LastHaven.Enemies;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Gündüz gezginleri (11.2, GDD §9): varsayılanlar (3–6 Yürüyen, dış halka), günlük sayı seçimi ve gezinme ayarları; üretilen
    /// asset Yürüyen'e bağlı. Varsayılan testleri asset oluşturmaz.
    /// </summary>
    public class DayWanderersConfigTests
    {
        private DayWanderersConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<DayWanderersConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Defaults_FollowGdd()
        {
            Assert.AreEqual(3, _config.MinCount);
            Assert.AreEqual(6, _config.MaxCount);

            WanderSettings settings = _config.CreateWanderSettings(Vector3.zero);
            Assert.AreEqual(20f, settings.MinRadius, "GDD §17 dış halkası (18 m) + aggro üsse taşmasın.");
            Assert.AreEqual(38f, settings.MaxRadius);
            Assert.LessOrEqual(settings.MinRadius - 6f, 14f, "Aggro yarıçapı üs alanına (14 m) girmiyor.");
        }

        [Test]
        public void PickCount_CoversWholeRangeInclusive()
        {
            Assert.AreEqual(3, _config.PickCount(0f));
            Assert.AreEqual(4, _config.PickCount(0.25f));
            Assert.AreEqual(5, _config.PickCount(0.5f));
            Assert.AreEqual(6, _config.PickCount(0.99f));
            Assert.AreEqual(6, _config.PickCount(1f), "1 dahil olsa da üst sınırı aşmaz.");
            Assert.AreEqual(3, _config.PickCount(-1f));
        }

        [Test]
        public void PickCount_MaxBelowMin_UsesMin()
        {
            var serialized = new SerializedObject(_config);
            serialized.FindProperty("_minCount").intValue = 4;
            serialized.FindProperty("_maxCount").intValue = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(4, _config.MaxCount);
            Assert.AreEqual(4, _config.PickCount(0.9f));
        }

        [Test]
        public void CreateWanderSettings_IsCenteredOnBase()
        {
            var center = new Vector3(2f, 0f, -1f);

            WanderSettings settings = _config.CreateWanderSettings(center);

            Assert.AreEqual(center, settings.Center);
            Assert.AreEqual(4f, settings.MinLegDistance);
            Assert.AreEqual(10f, settings.MaxLegDistance);
            Assert.AreEqual(1f, settings.MinPause);
            Assert.AreEqual(3f, settings.MaxPause);
            Assert.AreEqual(12f, settings.MaxLegDuration);
        }

        [Test]
        public void GeneratedAsset_LinksWalker()
        {
            var config = AssetDatabase.LoadAssetAtPath<DayWanderersConfig>(DayWanderersGenerator.ConfigPath);
            if (config == null)
            {
                Assert.Inconclusive($"{DayWanderersGenerator.ConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.IsNotNull(config.Enemy);
            Assert.AreEqual(EnemyDefinitions.Walker.Id, config.Enemy.Id);
        }
    }
}
