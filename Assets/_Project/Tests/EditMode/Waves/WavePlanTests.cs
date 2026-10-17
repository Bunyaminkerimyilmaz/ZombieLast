using System.Collections.Generic;
using System.Text.RegularExpressions;
using LastHaven.Core;
using LastHaven.Enemies;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// WavePlan: NightData → WaveRunner grupları (geçersiz grupları atlama, boss) ve istihbarat özeti (NightInfo).
    /// Ayrıca dalga verisi varsayılanları (GDD §8). Asset oluşturmaz.
    /// </summary>
    public class WavePlanTests
    {
        private const float Tolerance = 0.0001f;

        private readonly List<Object> _created = new List<Object>();
        private readonly List<WaveGroup> _groups = new List<WaveGroup>();

        [SetUp]
        public void SetUp()
        {
            _groups.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                Object.DestroyImmediate(_created[i]);
            }

            _created.Clear();
        }

        // --- BuildGroups ---

        [Test]
        public void BuildGroups_ConvertsValidGroups()
        {
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            NightData night = CreateNight(1,
                new SpawnGroup(walker, 20, 0.8f, 3f, new[] { CompassDirection.North, CompassDirection.East }));

            WavePlan.BuildGroups(night, _groups);

            Assert.AreEqual(1, _groups.Count);
            WaveGroup group = _groups[0];
            Assert.AreSame(walker, group.Enemy);
            Assert.AreEqual(20, group.Count);
            Assert.AreEqual(0.8f, group.SpawnInterval, Tolerance);
            Assert.AreEqual(3f, group.StartDelay, Tolerance);
            CollectionAssert.AreEqual(new[] { CompassDirection.North, CompassDirection.East }, group.Directions);
            Assert.IsFalse(group.IsBoss);
        }

        [Test]
        public void BuildGroups_SkipsInvalidGroups_WithWarning()
        {
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            NightData night = CreateNight(1,
                new SpawnGroup(null, 5, 1f, 0f, new[] { CompassDirection.North }),
                new SpawnGroup(walker, 0, 1f, 0f, new[] { CompassDirection.North }),
                new SpawnGroup(walker, 5, 1f, 0f, new CompassDirection[0]),
                new SpawnGroup(walker, 7, 1f, 0f, new[] { CompassDirection.South }));

            LogAssert.Expect(LogType.Warning, new Regex("0\\. grup atlandı \\(düşman verisi yok\\)"));
            LogAssert.Expect(LogType.Warning, new Regex("1\\. grup atlandı \\(adet 0\\)"));
            LogAssert.Expect(LogType.Warning, new Regex("2\\. grup atlandı \\(yön yok\\)"));

            WavePlan.BuildGroups(night, _groups);

            Assert.AreEqual(1, _groups.Count);
            Assert.AreEqual(7, _groups[0].Count);
        }

        [Test]
        public void BuildGroups_AppendsBossAsSingleGroup()
        {
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            EnemyData brute = CreateEnemy("brute", "Brute");
            NightData night = CreateNight(5, new SpawnGroup(walker, 40, 0.9f, 0f, new[] { CompassDirection.North }));
            SetBoss(night, brute, 12f, CompassDirection.West);

            WavePlan.BuildGroups(night, _groups);

            Assert.AreEqual(2, _groups.Count);
            WaveGroup boss = _groups[1];
            Assert.AreSame(brute, boss.Enemy);
            Assert.IsTrue(boss.IsBoss);
            Assert.AreEqual(1, boss.Count);
            Assert.AreEqual(12f, boss.StartDelay, Tolerance);
            CollectionAssert.AreEqual(new[] { CompassDirection.West }, boss.Directions);
        }

        // --- BuildInfo ---

        [Test]
        public void BuildInfo_SumsCountsPerType_InFirstAppearanceOrder()
        {
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            EnemyData runner = CreateEnemy("runner", "Koşucu");
            NightData night = CreateNight(2,
                new SpawnGroup(walker, 10, 1f, 0f, new[] { CompassDirection.North }),
                new SpawnGroup(runner, 5, 1f, 0f, new[] { CompassDirection.East }),
                new SpawnGroup(walker, 7, 1f, 0f, new[] { CompassDirection.East }));

            NightInfo info = WavePlan.BuildInfo(1, night);

            Assert.AreEqual(1, info.NightIndex);
            Assert.AreEqual(2, info.NightNumber);
            Assert.AreEqual(2, info.Enemies.Count);
            AssertEnemy(info.Enemies[0], "walker", "Yürüyen", 17, isBoss: false);
            AssertEnemy(info.Enemies[1], "runner", "Koşucu", 5, isBoss: false);
            Assert.AreEqual(22, info.TotalEnemies);
            Assert.IsFalse(info.HasBoss);
        }

        [Test]
        public void BuildInfo_Directions_AreDistinct_InCompassOrder_IncludingBoss()
        {
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            NightData night = CreateNight(1,
                new SpawnGroup(walker, 1, 1f, 0f, new[] { CompassDirection.West, CompassDirection.North }),
                new SpawnGroup(walker, 1, 1f, 0f, new[] { CompassDirection.North }));
            SetBoss(night, CreateEnemy("brute", "Brute"), 0f, CompassDirection.East);

            NightInfo info = WavePlan.BuildInfo(0, night);

            CollectionAssert.AreEqual(new[] { CompassDirection.North, CompassDirection.East, CompassDirection.West }, info.Directions);
            Assert.IsTrue(info.HasDirection(CompassDirection.East), "Boss yönü de saldırı yönü.");
            Assert.IsFalse(info.HasDirection(CompassDirection.South));
        }

        [Test]
        public void NightInfo_Empty_HasNoDirection()
        {
            var info = new NightInfo(0, 1, null, null, null);

            foreach (CompassDirection direction in CompassDirections.All)
            {
                Assert.IsFalse(info.HasDirection(direction));
            }
        }

        [Test]
        public void BuildInfo_Boss_IsSeparateRow()
        {
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            NightData night = CreateNight(5, new SpawnGroup(walker, 40, 1f, 0f, new[] { CompassDirection.North }));
            SetBoss(night, CreateEnemy("brute", "Brute"), 0f, CompassDirection.North);

            NightInfo info = WavePlan.BuildInfo(4, night);

            Assert.IsTrue(info.HasBoss);
            Assert.AreEqual(2, info.Enemies.Count);
            AssertEnemy(info.Enemies[1], "brute", "Brute", 1, isBoss: true);
            Assert.AreEqual(41, info.TotalEnemies);
        }

        [Test]
        public void BuildInfo_EventName_ComesFromModifier()
        {
            NightData night = CreateNight(4);
            var fog = ScriptableObject.CreateInstance<NightModifierData>();
            _created.Add(fog);
            SetString(fog, "_displayName", "Sis");
            SetReference(night, "_modifier", fog);

            NightInfo info = WavePlan.BuildInfo(3, night);

            Assert.IsTrue(info.HasEvent);
            Assert.AreEqual("Sis", info.EventName);
        }

        [TestCase(40, 1.5f, 60)]
        [TestCase(10, 1.5f, 15)]
        [TestCase(5, 1.5f, 8)]
        [TestCase(3, 1.5f, 5)]
        [TestCase(1, 0.1f, 1)]
        [TestCase(0, 1.5f, 0)]
        [TestCase(20, 1f, 20)]
        public void ScaleCount_RoundsHalfUp_AtLeastOne(int count, float multiplier, int expected)
        {
            Assert.AreEqual(expected, WavePlan.ScaleCount(count, multiplier));
        }

        [Test]
        public void BloodMoon_ScalesGroupsAndIntel_NotBoss()
        {
            // GDD §8 Gece 5 + §14 Kan Ayı: 40 Yürüyen, 10 Koşucu ×1.5; Brute tek kalır.
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            EnemyData runner = CreateEnemy("runner", "Koşucu");
            EnemyData brute = CreateEnemy("brute", "Brute");
            NightData night = CreateNight(5,
                new SpawnGroup(walker, 40, 0.7f, 0f, new[] { CompassDirection.North }),
                new SpawnGroup(runner, 10, 0.9f, 0f, new[] { CompassDirection.East }));
            SetReference(night, "_bossData", brute);
            var bloodMoon = ScriptableObject.CreateInstance<NightModifierData>();
            _created.Add(bloodMoon);
            LastHaven.Editor.NightModifiersGenerator.Apply(bloodMoon, LastHaven.Editor.NightModifierDefinitions.BloodMoon);
            SetReference(night, "_modifier", bloodMoon);

            var groups = new List<WaveGroup>();
            WavePlan.BuildGroups(night, groups);
            NightInfo info = WavePlan.BuildInfo(4, night);

            Assert.AreEqual(60, groups[0].Count);
            Assert.AreEqual(15, groups[1].Count);
            Assert.AreEqual(1, groups[2].Count, "Boss çarpılmaz.");
            Assert.IsTrue(groups[2].IsBoss);
            Assert.AreEqual(0.7f, groups[0].SpawnInterval, 0.0001f, "Aralık değişmez; gece uzar.");
            Assert.AreEqual(76, info.TotalEnemies, "İstihbarat çarpılmış adedi gösterir.");
            Assert.AreEqual("Kan Ayı", info.EventName);
            Assert.AreEqual(LastHaven.Editor.NightModifierDefinitions.BloodMoon.Description, info.EventDescription);
        }

        [Test]
        public void BuildInfo_NoModifier_HasNoEvent()
        {
            NightInfo info = WavePlan.BuildInfo(0, CreateNight(1));

            Assert.IsFalse(info.HasEvent);
            Assert.AreEqual(string.Empty, info.EventName);
            Assert.AreEqual(string.Empty, info.EventDescription);
            Assert.IsEmpty(info.Directions);
            Assert.IsEmpty(info.Enemies);
        }

        [Test]
        public void BuildInfo_IgnoresInvalidGroups_Silently()
        {
            EnemyData walker = CreateEnemy("walker", "Yürüyen");
            NightData night = CreateNight(1,
                new SpawnGroup(null, 5, 1f, 0f, new[] { CompassDirection.South }),
                new SpawnGroup(walker, 3, 1f, 0f, new[] { CompassDirection.North }));

            NightInfo info = WavePlan.BuildInfo(0, night);

            LogAssert.NoUnexpectedReceived();
            Assert.AreEqual(3, info.TotalEnemies);
            CollectionAssert.AreEqual(new[] { CompassDirection.North }, info.Directions);
        }

        // --- Varsayılanlar (GDD §8) ---

        [Test]
        public void WaveConfig_Defaults_MatchGdd()
        {
            var config = ScriptableObject.CreateInstance<WaveConfig>();
            _created.Add(config);

            Assert.AreEqual(120, config.MaxAliveEnemies);
            Assert.IsEmpty(config.Nights);
        }

        [Test]
        public void SpawnGroup_DefaultInterval_IsInGddRange()
        {
            var group = new SpawnGroup();

            Assert.That(group.SpawnInterval, Is.InRange(0.6f, 1.2f));
            Assert.AreEqual(0f, group.StartDelay);
            Assert.IsNotNull(group.Directions);
        }

        private static void AssertEnemy(NightEnemyInfo enemy, string id, string displayName, int count, bool isBoss)
        {
            Assert.AreEqual(id, enemy.EnemyId);
            Assert.AreEqual(displayName, enemy.DisplayName);
            Assert.AreEqual(count, enemy.Count);
            Assert.AreEqual(isBoss, enemy.IsBoss);
        }

        private EnemyData CreateEnemy(string id, string displayName)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            _created.Add(data);
            SetString(data, "_id", id);
            SetString(data, "_displayName", displayName);
            return data;
        }

        /// <summary>Üreticinin (5.3) kullanacağı serileşmiş alan adlarıyla gece verisi kurar.</summary>
        private NightData CreateNight(int nightNumber, params SpawnGroup[] groups)
        {
            var night = ScriptableObject.CreateInstance<NightData>();
            _created.Add(night);

            var serialized = new SerializedObject(night);
            serialized.FindProperty("_nightNumber").intValue = nightNumber;
            SerializedProperty list = serialized.FindProperty("_groups");
            list.arraySize = groups.Length;
            for (int i = 0; i < groups.Length; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_enemy").objectReferenceValue = groups[i].Enemy;
                element.FindPropertyRelative("_count").intValue = groups[i].Count;
                element.FindPropertyRelative("_spawnInterval").floatValue = groups[i].SpawnInterval;
                element.FindPropertyRelative("_startDelay").floatValue = groups[i].StartDelay;

                SerializedProperty directions = element.FindPropertyRelative("_directions");
                directions.arraySize = groups[i].Directions.Count;
                for (int d = 0; d < groups[i].Directions.Count; d++)
                {
                    directions.GetArrayElementAtIndex(d).enumValueIndex = (int)groups[i].Directions[d];
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return night;
        }

        private static void SetBoss(NightData night, EnemyData boss, float startDelay, CompassDirection direction)
        {
            var serialized = new SerializedObject(night);
            serialized.FindProperty("_bossData").objectReferenceValue = boss;
            serialized.FindProperty("_bossStartDelay").floatValue = startDelay;
            serialized.FindProperty("_bossDirection").enumValueIndex = (int)direction;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetString(Object target, string property, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
