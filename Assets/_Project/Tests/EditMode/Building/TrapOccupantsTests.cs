using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Tuzak alanı taraması: kutudaki canlı düşmanlar (alıcılarıyla), ölüler/kapalılar/başka layer'lar ve alan dışı atlanır,
    /// birden fazla collider'lı düşman bir kez sayılır, kutu trigger collider'ının dünya boyutundan alınır. Test objeleri
    /// haritanın çok dışında kurulur.
    /// </summary>
    public class TrapOccupantsTests
    {
        private static readonly Vector3 Origin = new Vector3(-5000f, 0f, 5000f);
        private static readonly Vector3 HalfExtents = new Vector3(1f, 0.5f, 1f);

        private readonly List<GameObject> _objects = new List<GameObject>();
        private TrapOccupants _occupants;

        [SetUp]
        public void SetUp()
        {
            _occupants = new TrapOccupants(16, GameLayers.EnemyMask);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in _objects)
            {
                Object.DestroyImmediate(gameObject);
            }

            _objects.Clear();
            DamageableLookup.Clear();
            StatusEffectLookup.Clear();
        }

        [Test]
        public void Scan_FindsAliveEnemiesInBox_WithReceivers()
        {
            Health inside = CreateEnemy(new Vector3(0.5f, 0f, -0.5f));
            var receiver = inside.gameObject.AddComponent<EnemyStatusEffects>();
            Assert.IsNotNull(receiver);
            CreateEnemy(new Vector3(3f, 0f, 0f));
            Physics.SyncTransforms();

            int count = _occupants.Scan(Origin + Vector3.up * 0.5f, HalfExtents, Quaternion.identity);

            Assert.AreEqual(1, count);
            Assert.AreSame(inside, _occupants.Current[0].Damageable);
            Assert.AreSame(receiver, _occupants.Current[0].Receiver);
        }

        [Test]
        public void Scan_SkipsDeadDisabledAndOtherLayers()
        {
            Health dead = CreateEnemy(new Vector3(0.2f, 0f, 0f));
            dead.TakeDamage(new DamageInfo(1000f, DamageType.Bullet, null, Vector3.zero, Vector3.forward));
            Health structure = CreateEnemy(new Vector3(-0.2f, 0f, 0f));
            structure.gameObject.layer = GameLayers.Structure;
            Health alive = CreateEnemy(new Vector3(0f, 0f, 0.5f));
            Physics.SyncTransforms();

            Assert.AreEqual(1, _occupants.Scan(Origin + Vector3.up * 0.5f, HalfExtents, Quaternion.identity));
            Assert.AreSame(alive, _occupants.Current[0].Damageable);
            Assert.IsNull(_occupants.Current[0].Receiver, "Alıcısı olmayan da hasar alır.");
        }

        [Test]
        public void Scan_EnemyWithTwoColliders_CountedOnce()
        {
            Health enemy = CreateEnemy(Vector3.zero);
            var child = new GameObject("Hitbox") { layer = GameLayers.Enemy };
            child.transform.SetParent(enemy.transform, false);
            child.AddComponent<SphereCollider>().radius = 0.3f;
            Physics.SyncTransforms();

            Assert.AreEqual(1, _occupants.Scan(Origin + Vector3.up * 0.5f, HalfExtents, Quaternion.identity));
        }

        [Test]
        public void Scan_UsesBoxColliderWorldArea()
        {
            var trap = new GameObject("Trap") { layer = GameLayers.Structure };
            trap.transform.position = Origin;
            var area = trap.AddComponent<BoxCollider>();
            area.isTrigger = true;
            area.size = new Vector3(2f, 1f, 2f);
            area.center = new Vector3(0f, 0.5f, 0f);
            _objects.Add(trap);
            CreateEnemy(new Vector3(0.8f, 0f, 0.8f));
            CreateEnemy(new Vector3(1.6f, 0f, 0f));
            Physics.SyncTransforms();

            TrapOccupants.GetWorldBox(area, out Vector3 center, out Vector3 halfExtents, out _);
            Assert.Less(Vector3.Distance(Origin + Vector3.up * 0.5f, center), 0.001f);
            Assert.Less(Vector3.Distance(new Vector3(1f, 0.5f, 1f), halfExtents), 0.001f);
            Assert.AreEqual(1, _occupants.Scan(area), "Kendi trigger'ı sayılmaz (Structure layer, trigger).");
        }

        // Kapsül yerine küçük kutu: ayak noktası (pivot) yerde, gövde yukarıda.
        private Health CreateEnemy(Vector3 offset)
        {
            var enemy = new GameObject("TrapTestEnemy") { layer = GameLayers.Enemy };
            enemy.transform.position = Origin + offset;
            var collider = enemy.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.4f, 1.8f, 0.4f);
            collider.center = new Vector3(0f, 0.9f, 0f);
            var health = enemy.AddComponent<Health>();
            health.SetMax(60f, refill: true);
            _objects.Add(enemy);
            return health;
        }
    }
}
