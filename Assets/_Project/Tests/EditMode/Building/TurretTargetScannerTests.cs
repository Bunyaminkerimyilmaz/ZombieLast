using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Taret taraması (GDD §11): menzildeki en yakın canlı düşman (yatay mesafe), ölüler ve menzil dışı atlanır, sadece hedef
    /// layer'ı. Sahnedeki objelerle karışmasın diye test objeleri haritanın çok dışında kurulur.
    /// </summary>
    public class TurretTargetScannerTests
    {
        private static readonly Vector3 Origin = new Vector3(5000f, 1.2f, 5000f);

        private readonly List<GameObject> _objects = new List<GameObject>();
        private TurretTargetScanner _scanner;

        [SetUp]
        public void SetUp()
        {
            _scanner = new TurretTargetScanner(16, GameLayers.EnemyMask);
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
        }

        [Test]
        public void TryFindNearest_PicksNearestAliveInRange()
        {
            CreateEnemy(new Vector3(6f, 0f, 0f));
            Health near = CreateEnemy(new Vector3(0f, 0f, -3f));
            Health dead = CreateEnemy(new Vector3(1f, 0f, 0f));
            dead.TakeDamage(new DamageInfo(1000f, DamageType.Bullet, null, Vector3.zero, Vector3.forward));
            Physics.SyncTransforms();

            Assert.IsTrue(_scanner.TryFindNearest(Origin, 12f, out TurretTarget target));
            Assert.AreSame(near.transform, target.Transform, "Ölü düşman en yakın olsa da atlanır.");
            Assert.IsTrue(target.IsValid);
        }

        [Test]
        public void TryFindNearest_IgnoresOutOfRangeAndOtherLayers()
        {
            CreateEnemy(new Vector3(20f, 0f, 0f));
            Health structure = CreateEnemy(new Vector3(2f, 0f, 0f));
            structure.gameObject.layer = GameLayers.Structure;
            Physics.SyncTransforms();

            Assert.IsFalse(_scanner.TryFindNearest(Origin, 12f, out TurretTarget target));
            Assert.IsFalse(target.IsValid);
        }

        [Test]
        public void TryFindNearest_ZeroRangeFindsNothing()
        {
            CreateEnemy(new Vector3(0.5f, 0f, 0f));
            Physics.SyncTransforms();

            Assert.IsFalse(_scanner.TryFindNearest(Origin, 0f, out _));
        }

        [Test]
        public void Target_BecomesInvalidWhenEnemyDiesOrIsDisabled()
        {
            Health enemy = CreateEnemy(new Vector3(3f, 0f, 0f));
            Health pooled = CreateEnemy(new Vector3(0f, 0f, 4f));
            Physics.SyncTransforms();
            Assert.IsTrue(_scanner.TryFindNearest(Origin, 12f, out TurretTarget target));
            Assert.AreSame(enemy.transform, target.Transform);
            var pooledTarget = new TurretTarget(pooled, pooled.transform);
            Assert.IsTrue(pooledTarget.IsValid);

            enemy.TakeDamage(new DamageInfo(1000f, DamageType.Bullet, null, Vector3.zero, Vector3.forward));
            pooled.gameObject.SetActive(false);

            Assert.IsFalse(target.IsValid, "Öldü.");
            Assert.IsFalse(pooledTarget.IsValid, "Havuza döndü (kapalı).");
        }

        private Health CreateEnemy(Vector3 offset)
        {
            var enemy = new GameObject("ScannerEnemy") { layer = GameLayers.Enemy };
            enemy.transform.position = new Vector3(Origin.x, 0f, Origin.z) + offset;
            var capsule = enemy.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1f, 0f);
            capsule.height = 2f;
            capsule.radius = 0.5f;
            Health health = enemy.AddComponent<Health>();
            health.SetMax(50f, refill: true);
            _objects.Add(enemy);
            return health;
        }
    }
}
