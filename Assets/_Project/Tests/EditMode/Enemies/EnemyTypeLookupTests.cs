using System.Collections.Generic;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>EnemyTypeLookup: id → EnemyData eşlemesi (EnemySpawner'ın string id ile tip bulması).</summary>
    public class EnemyTypeLookupTests
    {
        private readonly List<EnemyData> _created = new List<EnemyData>();
        private EnemyTypeLookup _lookup;

        [SetUp]
        public void SetUp()
        {
            _lookup = new EnemyTypeLookup();
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
        public void TryAdd_ValidData_IsFoundById()
        {
            EnemyData walker = CreateData("walker");

            Assert.AreEqual(EnemyTypeAddResult.Added, _lookup.TryAdd(walker));
            Assert.IsTrue(_lookup.TryGet("walker", out EnemyData found));
            Assert.AreSame(walker, found);
            Assert.AreEqual(1, _lookup.Count);
        }

        [Test]
        public void TryAdd_Null_IsRejected()
        {
            Assert.AreEqual(EnemyTypeAddResult.MissingData, _lookup.TryAdd(null));
            Assert.AreEqual(0, _lookup.Count);
        }

        [Test]
        public void TryAdd_EmptyId_IsRejected()
        {
            Assert.AreEqual(EnemyTypeAddResult.EmptyId, _lookup.TryAdd(CreateData(string.Empty)));
            Assert.AreEqual(0, _lookup.Count);
        }

        [Test]
        public void TryAdd_DuplicateId_KeepsFirst()
        {
            EnemyData first = CreateData("tank");
            EnemyData second = CreateData("tank");

            _lookup.TryAdd(first);

            Assert.AreEqual(EnemyTypeAddResult.DuplicateId, _lookup.TryAdd(second));
            Assert.IsTrue(_lookup.TryGet("tank", out EnemyData found));
            Assert.AreSame(first, found);
        }

        [TestCase("TANK")]
        [TestCase("Tank")]
        [TestCase("tank ")]
        public void TryGet_IsOrdinal(string query)
        {
            _lookup.TryAdd(CreateData("tank"));

            Assert.IsFalse(_lookup.TryGet(query, out EnemyData found));
            Assert.IsNull(found);
        }

        [Test]
        public void TryGet_DistinctIds_AreKeptApart_UnderTurkishCasing()
        {
            // tr-TR'de "I".ToLower() → "ı"; ordinal karşılaştırma kültürden etkilenmez.
            EnemyData upper = CreateData("RUNNER_I");
            EnemyData lower = CreateData("runner_i");
            _lookup.TryAdd(upper);
            _lookup.TryAdd(lower);

            Assert.IsTrue(_lookup.TryGet("RUNNER_I", out EnemyData foundUpper));
            Assert.IsTrue(_lookup.TryGet("runner_i", out EnemyData foundLower));
            Assert.AreSame(upper, foundUpper);
            Assert.AreSame(lower, foundLower);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("spitter")]
        public void TryGet_MissingId_ReturnsFalse(string query)
        {
            _lookup.TryAdd(CreateData("walker"));

            Assert.IsFalse(_lookup.TryGet(query, out EnemyData found));
            Assert.IsNull(found);
        }

        private EnemyData CreateData(string id)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(data);
            return data;
        }
    }
}
