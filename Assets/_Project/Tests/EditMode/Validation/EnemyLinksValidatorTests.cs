using System.Collections.Generic;
using LastHaven.Editor;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Validate Project'in düşman verisi ↔ prefab ↔ spawner kuralları (<see cref="EnemyLinksValidator"/>). Asset oluşturmaz.</summary>
    public class EnemyLinksValidatorTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<EnemyData, string> _usedBy = new Dictionary<EnemyData, string>();
        private ValidationReport _report;

        [SetUp]
        public void SetUp()
        {
            _report = new ValidationReport();
            _report.BeginSection("Test");
            _usedBy.Clear();
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

        [Test]
        public void LinkedAndRegistered_IsClean()
        {
            EnemyData walker = CreateLinked("walker");
            _usedBy.Add(walker, "Night01");

            Validate(new[] { walker }, CreateConfig(walker));

            Assert.IsTrue(_report.IsClean, Describe());
        }

        [Test]
        public void PrefabWithoutData_IsError()
        {
            EnemyData walker = CreateData("walker");
            SetReference(walker, "_prefab", CreatePrefab(null));

            Validate(new[] { walker }, CreateConfig(walker));

            Assert.AreEqual(1, _report.ErrorCount, Describe());
        }

        [Test]
        public void PrefabLinkedToOtherData_IsError()
        {
            EnemyData walker = CreateLinked("walker");
            EnemyData brute = CreateData("brute");
            SetReference(brute, "_prefab", walker.Prefab);

            Validate(new[] { walker, brute }, CreateConfig(walker, brute));

            Assert.AreEqual(1, _report.ErrorCount, Describe());
            Assert.AreSame(brute, _report.Issues[0].Context);
        }

        [Test]
        public void EmptyPrefab_IsLeftToRequiredReferences()
        {
            EnemyData walker = CreateData("walker");

            Validate(new[] { walker }, CreateConfig(walker));

            Assert.IsTrue(_report.IsClean, Describe());
        }

        [Test]
        public void DuplicateOrEmptyId_IsError()
        {
            EnemyData a = CreateLinked("walker");
            EnemyData b = CreateLinked("walker");
            EnemyData c = CreateLinked(string.Empty);

            Validate(new[] { a, b, c }, CreateConfig(a, b));

            // b: id tekrarı (veride) + config'te ikinci satır; c: boş id.
            Assert.AreEqual(3, _report.ErrorCount, Describe());
        }

        [Test]
        public void UsedButNotRegistered_IsError_UnusedIsWarning()
        {
            EnemyData walker = CreateLinked("walker");
            EnemyData spitter = CreateLinked("spitter");
            EnemyData exploder = CreateLinked("exploder");
            _usedBy.Add(spitter, "Night04");

            Validate(new[] { walker, spitter, exploder }, CreateConfig(walker));

            Assert.AreEqual(1, _report.ErrorCount, Describe());
            Assert.AreEqual(1, _report.WarningCount, Describe());
            StringAssert.Contains("Night04", Describe());
        }

        [Test]
        public void MissingConfig_IsError()
        {
            Validate(new[] { CreateLinked("walker") }, null);

            Assert.AreEqual(1, _report.ErrorCount, Describe());
        }

        private void Validate(EnemyData[] enemies, EnemySpawnerConfig config)
        {
            EnemyLinksValidator.Validate(enemies, config, _usedBy, _report);
        }

        private EnemyData CreateLinked(string id)
        {
            EnemyData data = CreateData(id);
            SetReference(data, "_prefab", CreatePrefab(data));
            return data;
        }

        private EnemyData CreateData(string id)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.name = string.IsNullOrEmpty(id) ? "NoId" : id;
            _created.Add(data);
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private Enemy CreatePrefab(EnemyData data)
        {
            var root = new GameObject("Prefab");
            _created.Add(root);
            var enemy = root.AddComponent<Enemy>();
            SetReference(enemy, "_data", data);
            return enemy;
        }

        private EnemySpawnerConfig CreateConfig(params EnemyData[] rows)
        {
            var config = ScriptableObject.CreateInstance<EnemySpawnerConfig>();
            config.name = "EnemySpawnerConfig";
            _created.Add(config);
            var serialized = new SerializedObject(config);
            SerializedProperty list = serialized.FindProperty("_enemies");
            list.arraySize = rows.Length;
            for (int i = 0; i < rows.Length; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_data").objectReferenceValue = rows[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static void SetReference(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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
