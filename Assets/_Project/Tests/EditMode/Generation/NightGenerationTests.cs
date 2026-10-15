using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Enemies;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Gece üreticisi: GDD §8 tablosu (<see cref="NightDefinitions"/>), zamanlama, "eksikse ekle" doldurucu (<see cref="NightDataFiller"/>)
    /// ve config'e gece bağlama (<see cref="WavesGenerator.LinkNights"/>). Asset oluşturmaz; veriler bellekte.
    /// </summary>
    public class NightGenerationTests
    {
        private const float Tolerance = 0.001f;

        // GDD §8: aralık 0.6–1.2 sn.
        private const float MinInterval = 0.6f;
        private const float MaxInterval = 1.2f;

        // Gecikmeler 0.1 sn'ye yuvarlanır.
        private const float RoundingTolerance = 0.051f;

        private static IEnumerable<NightDefinition> AllNights => NightDefinitions.All;

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                if (_created[i] != null)
                {
                    Object.DestroyImmediate(_created[i]);
                }
            }

            _created.Clear();
        }

        // --- GDD §8 tablosu ---

        [Test]
        public void Definitions_AreTenNights_NumberedInOrder_WithUniqueAssetNames()
        {
            Assert.AreEqual(10, NightDefinitions.All.Count);
            var names = new HashSet<string>();
            for (int i = 0; i < NightDefinitions.All.Count; i++)
            {
                Assert.AreEqual(i + 1, NightDefinitions.All[i].Number);
                Assert.IsTrue(names.Add(NightDefinitions.All[i].AssetName), NightDefinitions.All[i].AssetName);
            }

            Assert.AreEqual("Night01", NightDefinitions.All[0].AssetName);
            Assert.AreEqual("Night10", NightDefinitions.All[9].AssetName);
        }

        [TestCase(1, "N", "walker:20", null, null)]
        [TestCase(2, "NE", "walker:30 runner:8", null, null)]
        [TestCase(3, "NES", "walker:35 runner:15 tank:2", null, null)]
        [TestCase(4, "EW", "walker:40 runner:15 tank:3 spitter:6", "fog", null)]
        [TestCase(5, "NESW", "walker:40 runner:10", "blood_moon", "brute")]
        [TestCase(6, "NS", "walker:50 runner:20 tank:4 exploder:8", null, null)]
        [TestCase(7, "ESW", "walker:55 runner:25 tank:5 spitter:10 exploder:8", "storm", null)]
        [TestCase(8, "NESW", "walker:70 runner:30 tank:6 spitter:12 exploder:12", null, null)]
        [TestCase(9, "NESW", "walker:80 runner:35 tank:8 spitter:15 exploder:15", "fog", null)]
        [TestCase(10, "NESW", "walker:60 runner:30 tank:6 exploder:10", "blood_moon", "brute_final")]
        public void Definition_MatchesGddTable(int number, string directions, string content, string modifierId, string bossId)
        {
            NightDefinition night = NightDefinitions.All[number - 1];
            CompassDirection[] expectedDirections = ParseDirections(directions);
            string[] expectedGroups = content.Split(' ');

            CollectionAssert.AreEqual(expectedDirections, night.Directions);
            Assert.AreEqual(modifierId, night.ModifierId);
            Assert.AreEqual(bossId, night.BossId);
            Assert.AreEqual(expectedGroups.Length, night.Groups.Count);
            for (int i = 0; i < expectedGroups.Length; i++)
            {
                string[] parts = expectedGroups[i].Split(':');
                Assert.AreEqual(parts[0], night.Groups[i].EnemyId);
                Assert.AreEqual(int.Parse(parts[1]), night.Groups[i].Count);
                CollectionAssert.AreEqual(expectedDirections, night.Groups[i].Directions, "Gruplar gecenin yönlerinden gelir.");
            }
        }

        [Test]
        public void Definitions_UseGeneratedEnemyIds_ForExistingTypes()
        {
            Assert.AreEqual(EnemyDefinitions.Walker.Id, NightDefinitions.All[0].Groups[0].EnemyId);
            Assert.AreEqual(EnemyDefinitions.Runner.Id, NightDefinitions.All[1].Groups[1].EnemyId);
            Assert.AreEqual(EnemyDefinitions.Tank.Id, NightDefinitions.All[2].Groups[2].EnemyId);
        }

        // --- Zamanlama ---

        [TestCaseSource(nameof(AllNights))]
        public void Timing_IntervalsInGddRange_FrontGroupsStartImmediately(NightDefinition night)
        {
            foreach (NightGroupDefinition group in night.Groups)
            {
                Assert.That(group.SpawnInterval, Is.InRange(MinInterval, MaxInterval), group.EnemyId);
                if (NightDefinitions.GetProfile(group.EnemyId).Placement == WavePlacement.Front)
                {
                    Assert.AreEqual(0f, group.StartDelay, Tolerance, group.EnemyId);
                }
            }
        }

        [TestCaseSource(nameof(AllNights))]
        public void Timing_HeavyGroupsAndBoss_AreCenteredInMainStream(NightDefinition night)
        {
            float mainWindow = MainWindow(night);
            Assert.Greater(mainWindow, 0f);
            foreach (NightGroupDefinition group in night.Groups)
            {
                if (NightDefinitions.GetProfile(group.EnemyId).Placement != WavePlacement.Middle)
                {
                    continue;
                }

                float window = NightDefinitions.SpawnWindow(group.Count, group.SpawnInterval);
                Assert.Greater(group.StartDelay, 0f, $"{group.EnemyId} gece başında gelmemeli.");
                Assert.AreEqual(mainWindow * 0.5f, group.StartDelay + window * 0.5f, RoundingTolerance, $"{group.EnemyId} ortada olmalı.");
            }

            if (night.BossId != null)
            {
                Assert.AreEqual(mainWindow * 0.5f, night.BossStartDelay, RoundingTolerance);
                CollectionAssert.Contains(night.Directions, night.BossDirection);
            }
        }

        [Test]
        public void GetProfile_UnknownId_Throws()
        {
            Assert.Throws<ArgumentException>(() => NightDefinitions.GetProfile("unknown"));
        }

        // --- NightDataFiller ---

        [Test]
        public void ApplyNew_SetsNumber_AndBossTimingOnlyForBossNights()
        {
            NightData bossNight = CreateNight();
            NightData plainNight = CreateNight();
            float defaultBossDelay = plainNight.BossStartDelay;

            NightDataFiller.ApplyNew(bossNight, NightDefinitions.All[4]);
            NightDataFiller.ApplyNew(plainNight, NightDefinitions.All[0]);

            Assert.AreEqual(5, bossNight.NightNumber);
            Assert.AreEqual(NightDefinitions.All[4].BossStartDelay, bossNight.BossStartDelay, Tolerance);
            Assert.AreEqual(NightDefinitions.All[4].BossDirection, bossNight.BossDirection);
            Assert.IsNull(bossNight.BossData, "Boss verisi ApplyNew'da değil Fill'de bağlanır.");
            Assert.AreEqual(1, plainNight.NightNumber);
            Assert.AreEqual(defaultBossDelay, plainNight.BossStartDelay, Tolerance);
        }

        [Test]
        public void Fill_EmptyNight_AddsAvailableGroups_AndReportsMissingContent()
        {
            NightDefinition definition = NightDefinitions.All[3]; // 4. gece: Tükürücü + Sis henüz yok.
            NightData night = CreateNight();
            WaveContentLookup lookup = Lookup(Enemy("walker"), Enemy("runner"), Enemy("tank"));

            NightFillResult result = NightDataFiller.Fill(night, definition, lookup);

            Assert.AreEqual(3, result.AddedGroups);
            Assert.IsTrue(result.Changed);
            Assert.IsFalse(result.ModifierAssigned);
            CollectionAssert.AreEqual(new[] { "spitter × 6", "olay 'fog'" }, result.Missing);
            Assert.AreEqual(3, night.Groups.Count);
            for (int i = 0; i < night.Groups.Count; i++)
            {
                SpawnGroup group = night.Groups[i];
                NightGroupDefinition expected = definition.Groups[i];
                Assert.AreEqual(expected.EnemyId, group.Enemy.Id);
                Assert.AreEqual(expected.Count, group.Count);
                Assert.AreEqual(expected.SpawnInterval, group.SpawnInterval, Tolerance);
                Assert.AreEqual(expected.StartDelay, group.StartDelay, Tolerance);
                CollectionAssert.AreEqual(expected.Directions, group.Directions);
            }
        }

        [Test]
        public void Fill_SecondRun_ChangesNothing()
        {
            NightData night = CreateNight();
            WaveContentLookup lookup = Lookup(Enemy("walker"), Enemy("runner"));
            NightDataFiller.Fill(night, NightDefinitions.All[1], lookup);

            NightFillResult second = NightDataFiller.Fill(night, NightDefinitions.All[1], lookup);

            Assert.IsFalse(second.Changed);
            Assert.AreEqual(0, second.AddedGroups);
            Assert.IsEmpty(second.Missing);
            Assert.AreEqual(2, night.Groups.Count);
        }

        [Test]
        public void Fill_LaterContent_IsAdded_WithoutTouchingEditedGroups()
        {
            NightDefinition definition = NightDefinitions.All[3];
            NightData night = CreateNight();
            EnemyData walker = Enemy("walker");
            NightDataFiller.Fill(night, definition, Lookup(walker));
            SetGroupCount(night, 0, 5); // Tasarımcı inspector'da değiştirdi.

            // Gün 12: Tükürücü ve Sis eklendi.
            var lookup = new WaveContentLookup(
                new[] { walker, Enemy("runner"), Enemy("tank"), Enemy("spitter") },
                new[] { Modifier("fog") });
            NightFillResult result = NightDataFiller.Fill(night, definition, lookup);

            Assert.AreEqual(3, result.AddedGroups);
            Assert.IsTrue(result.ModifierAssigned);
            Assert.IsEmpty(result.Missing);
            Assert.AreEqual(4, night.Groups.Count);
            Assert.AreSame(walker, night.Groups[0].Enemy);
            Assert.AreEqual(5, night.Groups[0].Count, "Var olan gruba dokunulmamalı.");
            Assert.AreEqual("spitter", night.Groups[3].Enemy.Id);
            Assert.AreEqual("fog", night.Modifier.Id);
        }

        [Test]
        public void Fill_Boss_AssignedOnlyWhenEmpty()
        {
            NightDefinition definition = NightDefinitions.All[4]; // 5. gece: Brute.
            NightData night = CreateNight();
            EnemyData brute = Enemy("brute");

            NightFillResult first = NightDataFiller.Fill(night, definition, Lookup(Enemy("walker"), Enemy("runner"), brute));

            Assert.IsTrue(first.BossAssigned);
            Assert.AreSame(brute, night.BossData);
            CollectionAssert.AreEqual(new[] { "olay 'blood_moon'" }, first.Missing);

            EnemyData customBoss = Enemy("custom_boss");
            SetReference(night, "_bossData", customBoss);
            NightFillResult second = NightDataFiller.Fill(night, definition, Lookup(brute));

            Assert.IsFalse(second.BossAssigned);
            Assert.AreSame(customBoss, night.BossData, "Elle atanmış boss ezilmemeli.");
        }

        [Test]
        public void Fill_MissingBoss_IsReported()
        {
            NightFillResult result = NightDataFiller.Fill(CreateNight(), NightDefinitions.All[9], Lookup(Enemy("walker")));

            CollectionAssert.Contains(result.Missing, "boss 'brute_final'");
            CollectionAssert.Contains(result.Missing, "runner × 30");
        }

        [Test]
        public void Fill_Modifier_KeepsExisting()
        {
            NightData night = CreateNight();
            NightModifierData custom = Modifier("custom");
            SetReference(night, "_modifier", custom);

            NightFillResult result = NightDataFiller.Fill(night, NightDefinitions.All[3],
                new WaveContentLookup(null, new[] { Modifier("fog") }));

            Assert.IsFalse(result.ModifierAssigned);
            Assert.AreSame(custom, night.Modifier);
        }

        [Test]
        public void Lookup_DuplicateId_KeepsFirst_WithWarning()
        {
            EnemyData first = Enemy("walker");
            EnemyData second = Enemy("walker");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("'walker' id'si"));

            WaveContentLookup lookup = Lookup(first, second, Enemy(string.Empty));

            Assert.IsTrue(lookup.TryGetEnemy("walker", out EnemyData found));
            Assert.AreSame(first, found);
            Assert.IsFalse(lookup.TryGetEnemy(string.Empty, out _));
            Assert.IsFalse(lookup.TryGetEnemy("WALKER", out _), "Id'ler ordinal karşılaştırılır.");
        }

        // --- WavesGenerator.LinkNights ---

        [Test]
        public void LinkNights_EmptyConfig_LinksAllInOrder_ThenNothing()
        {
            WaveConfig config = Create<WaveConfig>();
            List<NightData> nights = CreateNights(3);

            Assert.AreEqual(3, WavesGenerator.LinkNights(config, nights));
            CollectionAssert.AreEqual(nights, config.Nights);
            Assert.AreEqual(0, WavesGenerator.LinkNights(config, nights));
            Assert.AreEqual(3, config.Nights.Count);
        }

        [Test]
        public void LinkNights_FillsEmptySlot_AndAppendsMissingTail()
        {
            WaveConfig config = Create<WaveConfig>();
            List<NightData> nights = CreateNights(3);
            SetNights(config, nights[0], null);

            Assert.AreEqual(2, WavesGenerator.LinkNights(config, nights));
            CollectionAssert.AreEqual(nights, config.Nights);
        }

        [Test]
        public void LinkNights_KeepsUserReplacementAndOrder()
        {
            WaveConfig config = Create<WaveConfig>();
            List<NightData> nights = CreateNights(3);
            NightData custom = CreateNight();
            SetNights(config, custom, nights[2], nights[1]);

            Assert.AreEqual(0, WavesGenerator.LinkNights(config, nights));
            CollectionAssert.AreEqual(new[] { custom, nights[2], nights[1] }, config.Nights);
        }

        // --- Yardımcılar ---

        private static float MainWindow(NightDefinition night)
        {
            float window = 0f;
            foreach (NightGroupDefinition group in night.Groups)
            {
                if (NightDefinitions.GetProfile(group.EnemyId).Placement == WavePlacement.Front)
                {
                    window = Mathf.Max(window, NightDefinitions.SpawnWindow(group.Count, group.SpawnInterval));
                }
            }

            return window;
        }

        private static CompassDirection[] ParseDirections(string letters)
        {
            var directions = new CompassDirection[letters.Length];
            for (int i = 0; i < letters.Length; i++)
            {
                directions[i] = letters[i] switch
                {
                    'N' => CompassDirection.North,
                    'E' => CompassDirection.East,
                    'S' => CompassDirection.South,
                    'W' => CompassDirection.West,
                    _ => throw new ArgumentException(letters),
                };
            }

            return directions;
        }

        private T Create<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            _created.Add(asset);
            return asset;
        }

        private NightData CreateNight()
        {
            return Create<NightData>();
        }

        private List<NightData> CreateNights(int count)
        {
            var nights = new List<NightData>(count);
            for (int i = 0; i < count; i++)
            {
                nights.Add(CreateNight());
            }

            return nights;
        }

        private EnemyData Enemy(string id)
        {
            var data = Create<EnemyData>();
            data.name = id;
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private NightModifierData Modifier(string id)
        {
            var data = Create<NightModifierData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private static WaveContentLookup Lookup(params EnemyData[] enemies)
        {
            return new WaveContentLookup(enemies, null);
        }

        private static void SetGroupCount(NightData night, int index, int count)
        {
            var serialized = new SerializedObject(night);
            serialized.FindProperty("_groups").GetArrayElementAtIndex(index).FindPropertyRelative("_count").intValue = count;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetNights(WaveConfig config, params NightData[] nights)
        {
            var serialized = new SerializedObject(config);
            SerializedProperty list = serialized.FindProperty("_nights");
            list.arraySize = nights.Length;
            for (int i = 0; i < nights.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = nights[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
