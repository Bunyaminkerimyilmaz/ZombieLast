using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>StatusEffectLookup: collider → üst objedeki alıcı (düşmanın EnemyStatusEffects'i), alıcısız collider null, önbellek.</summary>
    public class StatusEffectLookupTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            StatusEffectLookup.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            StatusEffectLookup.Clear();
        }

        [Test]
        public void Find_ReturnsReceiverOnParent()
        {
            _root = new GameObject("Enemy");
            var receiver = _root.AddComponent<EnemyStatusEffects>();
            Assert.IsNotNull(receiver);
            var child = new GameObject("Hitbox");
            child.transform.SetParent(_root.transform, false);
            var collider = child.AddComponent<BoxCollider>();

            Assert.AreSame(receiver, StatusEffectLookup.Find(collider));
            Assert.AreSame(receiver, StatusEffectLookup.Find(collider), "İkinci çağrı önbellekten.");
            Assert.AreEqual(1, StatusEffectLookup.Count);
        }

        [Test]
        public void Find_WithoutReceiver_ReturnsNullAndCaches()
        {
            _root = new GameObject("Rock");
            var collider = _root.AddComponent<BoxCollider>();

            Assert.IsNull(StatusEffectLookup.Find(collider));
            Assert.AreEqual(1, StatusEffectLookup.Count, "Alıcısız collider da önbelleğe girer.");
            Assert.IsNull(StatusEffectLookup.Find(null));
        }
    }
}
