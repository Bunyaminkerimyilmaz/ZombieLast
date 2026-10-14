using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>DamageableLookup: üst objede arar, sonucu (null dahil) önbellekler, yok edilen hedefi tekrar arar.</summary>
    public class DamageableLookupTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            DamageableLookup.Clear();
            _root = new GameObject("LookupRoot");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            DamageableLookup.Clear();
        }

        [Test]
        public void Find_ReturnsHealthOnParent()
        {
            var health = _root.AddComponent<Health>();
            Collider child = CreateChildCollider();

            Assert.AreSame(health, DamageableLookup.Find(child));
            Assert.AreEqual(1, DamageableLookup.Count);
        }

        [Test]
        public void Find_WithoutDamageable_ReturnsNullAndCachesIt()
        {
            Collider child = CreateChildCollider();

            Assert.IsNull(DamageableLookup.Find(child));
            Assert.IsNull(DamageableLookup.Find(child));
            Assert.AreEqual(1, DamageableLookup.Count);
        }

        [Test]
        public void Find_NullCollider_ReturnsNull()
        {
            Assert.IsNull(DamageableLookup.Find(null));
            Assert.AreEqual(0, DamageableLookup.Count);
        }

        [Test]
        public void Find_DestroyedHealth_LooksUpAgain()
        {
            var health = _root.AddComponent<Health>();
            Collider child = CreateChildCollider();
            DamageableLookup.Find(child);

            Object.DestroyImmediate(health);

            Assert.IsNull(DamageableLookup.Find(child));
        }

        private Collider CreateChildCollider()
        {
            var child = new GameObject("Hitbox");
            child.transform.SetParent(_root.transform, false);
            return child.AddComponent<BoxCollider>();
        }
    }
}
