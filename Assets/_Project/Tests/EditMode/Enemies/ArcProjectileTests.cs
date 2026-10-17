using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// ArcProjectile: yay boyunca uçar, çarptığı canlıya hasar verir (yapıya çarpanla), zemine çarpınca hasarsız söner, ömrü dolunca
    /// havuza döner; maske dışındaki layer'ları ve trigger'ları geçer. Test objeleri haritanın çok dışında kurulur.
    /// </summary>
    public class ArcProjectileTests
    {
        private const float Tolerance = 0.001f;
        private const float Gravity = 20f;
        private const int HitLayers = GameLayers.PlayerMask | GameLayers.StructureMask | GameLayers.GroundMask;
        private static readonly Vector3 Origin = new Vector3(-6000f, 0f, -6000f);

        private sealed class FakePool : IPoolService
        {
            public readonly List<GameObject> Released = new List<GameObject>();

            public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation) => null;

            public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component => null;

            public void Release(GameObject instance) => Released.Add(instance);

            public void Prewarm(GameObject prefab, int count)
            {
            }

            public PoolStats GetStats(GameObject prefab) => default;
        }

        private readonly List<GameObject> _objects = new List<GameObject>();
        private FakePool _pool;
        private GameObject _source;

        [SetUp]
        public void SetUp()
        {
            _pool = new FakePool();
            _source = Track(new GameObject("Spitter"));
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
        public void Hit_Player_DealsDamage_AndReturnsToPool()
        {
            Health player = CreateTarget("Player", GameLayers.Player, new Vector3(8f, 0f, 0f), 100f, out Vector3 aim);
            ArcProjectile projectile = Launch(aim, structureMultiplier: 2f);

            Fly(projectile, 2f);

            Assert.AreEqual(88f, player.Current, Tolerance, "GDD §7: Tükürücü 12 hasar.");
            Assert.IsFalse(projectile.IsFlying);
            CollectionAssert.AreEqual(new[] { projectile.gameObject }, _pool.Released);
        }

        [Test]
        public void Hit_Structure_AppliesMultiplier_WithProjectileDamageType()
        {
            Health turret = CreateTarget("Turret", GameLayers.Structure, new Vector3(0f, 0f, 9f), 250f, out Vector3 aim);
            DamagedEvent received = default;
            turret.Damaged += damaged => received = damaged;
            ArcProjectile projectile = Launch(aim, structureMultiplier: 2f);

            Fly(projectile, 2f);

            Assert.AreEqual(226f, turret.Current, Tolerance);
            Assert.AreEqual(DamageType.Projectile, received.Damage.DamageType);
            Assert.AreSame(_source, received.Damage.Source);
        }

        [Test]
        public void Arc_PassesOverLowWall_HitsTargetBehind()
        {
            // 1.5 m'lik duvar (barikat yüksekliği) yolun ortasında; yay 2.5 m'den geçer. Duvar maskede değil ama yüksekliği için Ground.
            GameObject wall = Track(new GameObject("Wall") { layer = GameLayers.Ground });
            wall.transform.position = Origin + new Vector3(5f, 0.75f, 0f);
            wall.AddComponent<BoxCollider>().size = new Vector3(0.5f, 1.5f, 3f);
            Health player = CreateTarget("Player", GameLayers.Player, new Vector3(10f, 0f, 0f), 100f, out Vector3 aim);
            ArcProjectile projectile = Launch(aim, structureMultiplier: 1f);

            Fly(projectile, 2f);

            Assert.AreEqual(88f, player.Current, Tolerance);
        }

        [Test]
        public void Hit_Ground_NoDamage_AndReturnsToPool()
        {
            GameObject ground = Track(new GameObject("Ground") { layer = GameLayers.Ground });
            ground.transform.position = Origin + Vector3.down * 0.5f;
            ground.AddComponent<BoxCollider>().size = new Vector3(40f, 1f, 40f);
            ArcProjectile projectile = Launch(Origin + new Vector3(6f, 0f, 0f), structureMultiplier: 1f);

            Fly(projectile, 2f);

            Assert.IsFalse(projectile.IsFlying);
            Assert.AreEqual(1, _pool.Released.Count);
            Assert.Less(projectile.transform.position.y, Origin.y + 0.5f, "Zemine indi.");
        }

        [Test]
        public void IgnoresOtherLayersAndTriggers_ExpiresAfterLifetime()
        {
            Health enemy = CreateTarget("Enemy", GameLayers.Enemy, new Vector3(8f, 0f, 0f), 50f, out Vector3 aim);
            Health trap = CreateTarget("Trap", GameLayers.Structure, new Vector3(8f, 0f, 0f), 40f, out _);
            trap.GetComponent<Collider>().isTrigger = true;
            ArcProjectile projectile = Launch(aim, structureMultiplier: 2f, maxLifetime: 1.5f);

            Fly(projectile, 1.4f);
            Assert.IsTrue(projectile.IsFlying);
            Fly(projectile, 0.2f);

            Assert.IsFalse(projectile.IsFlying);
            Assert.AreEqual(50f, enemy.Current, Tolerance, "Düşmanlar mermiyi durdurmaz.");
            Assert.AreEqual(40f, trap.Current, Tolerance, "Üstünden geçilen tuzak (trigger) vurulmaz.");
            Assert.AreEqual(1, _pool.Released.Count);
        }

        private ArcProjectile Launch(Vector3 target, float structureMultiplier, float maxLifetime = 4f)
        {
            Vector3 start = Origin + new Vector3(0f, 1.6f, 0f);
            GameObject gameObject = Track(new GameObject("AcidProjectile") { layer = GameLayers.EnemyProjectile });
            var projectile = gameObject.AddComponent<ArcProjectile>();
            float time = BallisticArc.FlightTime(start, target, 10f, 0.35f);
            var shot = new ProjectileShot(start, BallisticArc.LaunchVelocity(start, target, time, Gravity), Gravity, 0.25f, HitLayers,
                maxLifetime, 12f, structureMultiplier, _source, null);
            projectile.Launch(shot, _pool);
            Physics.SyncTransforms();
            return projectile;
        }

        private static void Fly(ArcProjectile projectile, float seconds)
        {
            const float Step = 0.02f;
            for (float t = 0f; t < seconds - 0.0001f && projectile.IsFlying; t += Step)
            {
                projectile.Advance(Step);
            }
        }

        /// <summary>Tabanı yerde 1×2×1 kutu collider'lı hedef; nişan noktası kutunun merkezi.</summary>
        private Health CreateTarget(string name, int layer, Vector3 offset, float maxHealth, out Vector3 aim)
        {
            GameObject gameObject = Track(new GameObject(name) { layer = layer });
            gameObject.transform.position = Origin + offset;
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1f, 2f, 1f);
            box.center = Vector3.up;
            var health = gameObject.AddComponent<Health>();
            health.SetMax(maxHealth, refill: true);
            Physics.SyncTransforms();
            aim = gameObject.transform.position + Vector3.up;
            return health;
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
