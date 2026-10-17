using System.Collections.Generic;
using System.Reflection;
using LastHaven.Building;
using LastHaven.Combat;
using LastHaven.Editor;
using LastHaven.Enemies;
using LastHaven.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Validate Project'in zorunlu referans kuralları (<see cref="RequiredReferenceValidator"/>). Asset oluşturmaz.</summary>
    public class RequiredReferenceValidatorTests
    {
        private const BindingFlags PrivateField = BindingFlags.Instance | BindingFlags.NonPublic;

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
                if (_created[i] != null)
                {
                    Object.DestroyImmediate(_created[i]);
                }
            }

            _created.Clear();
        }

        [Test]
        public void IsOptional_TooltipMarkerOrMediaType()
        {
            Assert.IsTrue(RequiredReferenceValidator.IsOptional(Field<EnemyData>("_deathVfx")), "Tooltip: Opsiyonel");
            Assert.IsTrue(RequiredReferenceValidator.IsOptional(Field<WeaponData>("_fireSound")), "AudioClip");
            Assert.IsTrue(RequiredReferenceValidator.IsOptional(Field<BuildableData>("_icon")), "Sprite");
            Assert.IsFalse(RequiredReferenceValidator.IsOptional(Field<EnemyData>("_prefab")));
            Assert.IsFalse(RequiredReferenceValidator.IsOptional(Field<LootConfig>("_lootTable")));
        }

        [Test]
        public void ValidateAsset_EnemyData_OnlyEmptyPrefabIsError()
        {
            var data = Create<EnemyData>();

            RequiredReferenceValidator.ValidateAsset(data, _report);

            Assert.AreEqual(1, _report.ErrorCount, Describe());
            StringAssert.Contains("'_prefab'", _report.Issues[0].Message);
            Assert.AreSame(data, _report.Issues[0].Context);
        }

        [Test]
        public void ValidateAsset_FilledReference_IsClean()
        {
            var data = Create<EnemyData>();
            var prefab = new GameObject("EnemyPrefab");
            _created.Add(prefab);
            SetReference(data, "_prefab", prefab.AddComponent<Enemy>());

            RequiredReferenceValidator.ValidateAsset(data, _report);

            Assert.IsTrue(_report.IsClean, Describe());
        }

        [Test]
        public void ValidateAsset_LootConfig_ReportsEveryEmptyRequiredField()
        {
            var config = Create<LootConfig>();

            RequiredReferenceValidator.ValidateAsset(config, _report);

            string all = Describe();
            foreach (string field in new[] { "_lootTable", "_cratePrefab", "_fuelPickupPrefab", "_ammoPickupPrefab" })
            {
                StringAssert.Contains($"'{field}'", all);
            }
        }

        [Test]
        public void ValidateAsset_NestedSerializableInList_IsWalked()
        {
            var config = Create<EnemySpawnerConfig>();
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_enemies").arraySize = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            RequiredReferenceValidator.ValidateAsset(config, _report);

            Assert.AreEqual(2, _report.ErrorCount, Describe());
            StringAssert.Contains("'_enemies[0]._data'", _report.Issues[0].Message);
            StringAssert.Contains("'_enemies[1]._data'", _report.Issues[1].Message);
        }

        [Test]
        public void ValidateAsset_EmptyElementOfReferenceList_IsNotAnIssue()
        {
            var config = Create<BuildConfig>();
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_slots").arraySize = 3;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            RequiredReferenceValidator.ValidateAsset(config, _report);

            StringAssert.DoesNotContain("_slots", Describe(), "Boş inşa yuvası bilinçli.");
            StringAssert.Contains("'_ghostMaterial'", Describe());
        }

        [Test]
        public void ValidateAsset_DestroyedReference_IsMissingError()
        {
            var config = Create<BuildConfig>();
            var material = new Material(Shader.Find("Hidden/InternalErrorShader"));
            SetReference(config, "_ghostMaterial", material);
            Object.DestroyImmediate(material);

            RequiredReferenceValidator.ValidateAsset(config, _report);

            StringAssert.Contains("kayıp", Describe());
        }

        [Test]
        public void DescribePath_ShortensArrayParts()
        {
            Assert.AreEqual("_enemies[2]._data", RequiredReferenceValidator.DescribePath("_enemies.Array.data[2]._data"));
            Assert.AreEqual("_prefab", RequiredReferenceValidator.DescribePath("_prefab"));
        }

        [Test]
        public void IsProjectType_OnlyLastHavenNamespaces()
        {
            Assert.IsTrue(RequiredReferenceValidator.IsProjectType(typeof(EnemyData)));
            Assert.IsFalse(RequiredReferenceValidator.IsProjectType(typeof(Material)));
            Assert.IsFalse(RequiredReferenceValidator.IsProjectType(typeof(string)));
        }

        private static FieldInfo Field<T>(string name)
        {
            FieldInfo field = typeof(T).GetField(name, PrivateField);
            Assert.IsNotNull(field, $"{typeof(T).Name}.{name}");
            return field;
        }

        private static void SetReference(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private T Create<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = typeof(T).Name;
            _created.Add(asset);
            return asset;
        }

        private string Describe()
        {
            var lines = new List<string>();
            foreach (ValidationIssue issue in _report.Issues)
            {
                lines.Add($"{issue.Severity}: {issue.Message}");
            }

            return string.Join("\n", lines);
        }
    }
}
