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
    /// Validate Project'in gece kuralları (<see cref="NightDataValidator"/>), ortak grup kuralı (<see cref="SpawnGroupValidation"/>),
    /// rapor sayımı ve markdown çıktısı. Asset oluşturmaz.
    /// </summary>
    public class NightDataValidatorTests
    {
        private static readonly CompassDirection[] North = { CompassDirection.North };

        private readonly List<Object> _created = new List<Object>();
        private ValidationReport _report;

        [SetUp]
        public void SetUp()
        {
            _report = new ValidationReport();
            _report.BeginSection("Test");
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

        // --- Ortak grup kuralı ---

        [Test]
        public void GetProblem_ChecksInOrder()
        {
            EnemyData walker = CreateEnemy();

            Assert.AreEqual(SpawnGroupProblem.MissingGroup, SpawnGroupValidation.GetProblem(null));
            Assert.AreEqual(SpawnGroupProblem.MissingEnemy, SpawnGroupValidation.GetProblem(new SpawnGroup(null, 0, 1f, 0f, null)));
            Assert.AreEqual(SpawnGroupProblem.ZeroCount, SpawnGroupValidation.GetProblem(new SpawnGroup(walker, 0, 1f, 0f, null)));
            Assert.AreEqual(SpawnGroupProblem.ZeroCount, SpawnGroupValidation.GetProblem(new SpawnGroup(walker, -3, 1f, 0f, North)));
            Assert.AreEqual(SpawnGroupProblem.NoDirection, SpawnGroupValidation.GetProblem(new SpawnGroup(walker, 5, 1f, 0f, null)));
            Assert.AreEqual(SpawnGroupProblem.None, SpawnGroupValidation.GetProblem(new SpawnGroup(walker, 5, 1f, 0f, North)));
        }

        [Test]
        public void Describe_EveryProblem_HasText()
        {
            foreach (SpawnGroupProblem problem in Enum.GetValues(typeof(SpawnGroupProblem)))
            {
                Assert.IsNotEmpty(SpawnGroupValidation.Describe(problem));
            }
        }

        // --- NightData ---

        [Test]
        public void ValidateNight_ValidNight_HasNoIssues()
        {
            NightData night = CreateNight(1, new SpawnGroup(CreateEnemy(), 20, 0.7f, 0f, North));

            NightDataValidator.ValidateNight(night, _report);

            Assert.IsTrue(_report.IsClean);
        }

        [Test]
        public void ValidateNight_NoGroups_IsError()
        {
            NightData night = CreateNight(1);

            NightDataValidator.ValidateNight(night, _report);

            Assert.AreEqual(1, _report.ErrorCount);
            StringAssert.Contains("hiç grup yok", _report.Issues[0].Message);
            Assert.AreSame(night, _report.Issues[0].Context);
            Assert.AreEqual("Test", _report.Issues[0].Section);
        }

        [Test]
        public void ValidateNight_OnlyBoss_IsValid()
        {
            NightData night = CreateNight(1);
            var serialized = new SerializedObject(night);
            serialized.FindProperty("_bossData").objectReferenceValue = CreateEnemy();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            NightDataValidator.ValidateNight(night, _report);

            Assert.IsTrue(_report.IsClean);
        }

        [Test]
        public void ValidateNight_EachBrokenGroup_IsOneError()
        {
            EnemyData walker = CreateEnemy();
            NightData night = CreateNight(1,
                new SpawnGroup(null, 5, 1f, 0f, North),
                new SpawnGroup(walker, 0, 1f, 0f, North),
                new SpawnGroup(walker, 5, 1f, 0f, null),
                new SpawnGroup(walker, 5, 1f, 0f, North));

            NightDataValidator.ValidateNight(night, _report);

            Assert.AreEqual(3, _report.ErrorCount);
            Assert.AreEqual(0, _report.WarningCount);
            StringAssert.Contains("grup [0] düşman verisi yok", _report.Issues[0].Message);
            StringAssert.Contains("grup [1] adet 0", _report.Issues[1].Message);
            StringAssert.Contains("grup [2] yön yok", _report.Issues[2].Message);
        }

        // --- WaveConfig ---

        [Test]
        public void ValidateConfig_EmptyList_IsError()
        {
            NightDataValidator.ValidateConfig(Create<WaveConfig>(), _report);

            Assert.AreEqual(1, _report.ErrorCount);
        }

        [Test]
        public void ValidateConfig_NullSlot_IsError_WrongNumberAndDuplicate_AreWarnings()
        {
            NightData first = CreateNight(1);
            NightData wrongNumber = CreateNight(7);
            WaveConfig config = CreateConfig(first, null, wrongNumber, first);

            NightDataValidator.ValidateConfig(config, _report);

            Assert.AreEqual(1, _report.ErrorCount);
            StringAssert.Contains("gece [1] boş", _report.Issues[0].Message);
            Assert.AreEqual(3, _report.WarningCount); // [2] numara 7; [3] numara 1 ≠ 4 ve tekrar.
        }

        [Test]
        public void ValidateConfig_OrderedNights_IsClean()
        {
            WaveConfig config = CreateConfig(CreateNight(1), CreateNight(2));

            NightDataValidator.ValidateConfig(config, _report);

            Assert.IsTrue(_report.IsClean);
        }

        // --- Rapor ---

        [Test]
        public void Report_CountsPerSection_AndMarkdownListsIssues()
        {
            _report.Error("ilk hata");
            _report.BeginSection("İkinci");
            _report.Note("2 şey kontrol edildi.");
            _report.Warning("bir | uyarı");

            Assert.AreEqual(1, _report.Count(ValidationSeverity.Error, "Test"));
            Assert.AreEqual(0, _report.Count(ValidationSeverity.Error, "İkinci"));
            Assert.AreEqual(1, _report.Count(ValidationSeverity.Warning));
            CollectionAssert.AreEqual(new[] { "Test", "İkinci" }, _report.Sections);

            string markdown = ValidationReportWriter.ToMarkdown(_report, new DateTime(2026, 10, 15, 9, 5, 0));

            StringAssert.Contains("1 hata, 1 uyarı.", markdown);
            StringAssert.Contains("2026-10-15 09:05", markdown);
            StringAssert.Contains("## İkinci", markdown);
            StringAssert.Contains("- 2 şey kontrol edildi.", markdown);
            StringAssert.Contains("| Hata | — | ilk hata |", markdown);
            StringAssert.Contains("| Uyarı | — | bir \\| uyarı |", markdown);
        }

        [Test]
        public void Report_Clean_SaysSo()
        {
            string markdown = ValidationReportWriter.ToMarkdown(_report, DateTime.Now);

            StringAssert.Contains("Temiz — 0 hata, 0 uyarı.", markdown);
            StringAssert.Contains("- Sorun yok.", markdown);
        }

        // --- Yardımcılar ---

        private T Create<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            _created.Add(asset);
            return asset;
        }

        private EnemyData CreateEnemy()
        {
            return Create<EnemyData>();
        }

        private NightData CreateNight(int number, params SpawnGroup[] groups)
        {
            NightData night = Create<NightData>();
            night.name = $"Night{number:00}";
            var serialized = new SerializedObject(night);
            serialized.FindProperty("_nightNumber").intValue = number;
            SerializedProperty list = serialized.FindProperty("_groups");
            list.arraySize = groups.Length;
            for (int i = 0; i < groups.Length; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_enemy").objectReferenceValue = groups[i].Enemy;
                element.FindPropertyRelative("_count").intValue = groups[i].Count;
                SerializedProperty directions = element.FindPropertyRelative("_directions");
                directions.arraySize = groups[i].Directions.Count;
                for (int j = 0; j < groups[i].Directions.Count; j++)
                {
                    directions.GetArrayElementAtIndex(j).enumValueIndex = (int)groups[i].Directions[j];
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return night;
        }

        private WaveConfig CreateConfig(params NightData[] nights)
        {
            WaveConfig config = Create<WaveConfig>();
            config.name = "WaveConfig";
            var serialized = new SerializedObject(config);
            SerializedProperty list = serialized.FindProperty("_nights");
            list.arraySize = nights.Length;
            for (int i = 0; i < nights.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = nights[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }
    }
}
