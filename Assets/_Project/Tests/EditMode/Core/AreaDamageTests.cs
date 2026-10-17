using System.Collections.Generic;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// AreaDamage: mesafeyle azalma, yapı çarpanı; yarıçaptaki canlılara bir kez hasar (çok collider'lı hedef tek), layer süzgeci,
    /// ölüler ve kaynak atlanır, trigger'lı yapı (tuzak) etkilenir. Test objeleri haritanın çok dışında kurulur.
    /// </summary>
    public class AreaDamageTests
    {
        private const float Tolerance = 0.001f;
        private static readonly Vector3 Origin = new Vector3(5000f, 0f, -5000f);

        // Patlayan (GDD §7): 3 m, 60 hasar, yapılara ×2, azalma yok; oyuncu + yapı.
        private static readonly AreaDamageSpec Exploder = new AreaDamageSpec(
            3f, 60f, 2f, 1f, GameLayers.PlayerMask | GameLayers.StructureMask, DamageType.Explosion);

        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly Collider[] _buffer = new Collider[16];

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

        [TestCase(0f, 1f)]
        [TestCase(1.5f, 0.75f)]
        [TestCase(3f, 0.5f)]
        [TestCase(5f, 0.5f)]
        public void Falloff_IsLinearToEdgeFraction(float distance, float expected)
        {
            Assert.AreEqual(expected, AreaDamage.Falloff(distance, 3f, 0.5f), Tolerance);
        }

        [Test]
        public void Falloff_EdgeOne_NoReduction()
        {
            Assert.AreEqual(1f, AreaDamage.Falloff(2.9f, 3f, 1f), Tolerance);
        }

        [Test]
        public void Calculate_AppliesStructureMultiplier()
        {
            Assert.AreEqual(60f, AreaDamage.Calculate(Exploder, 2f, isStructure: false), Tolerance);
            Assert.AreEqual(120f, AreaDamage.Calculate(Exploder, 2f, isStructure: true), Tolerance, "GDD §7: yapılara ×2.");
        }

        [Test]
        public void Apply_DamagesPlayerAndStructureInRadius_SkipsOutsideAndOtherLayers()
        {
            Health player = Create("Player", GameLayers.Player, new Vector3(2f, 0f, 0f), 100f);
            Health barricade = Create("Barricade", GameLayers.Structure, new Vector3(-2.5f, 0f, 0f), 300f);
            Health far = Create("Far", GameLayers.Player, new Vector3(4f, 0f, 0f), 100f);
            Health enemy = Create("Enemy", GameLayers.Enemy, new Vector3(0f, 0f, 1f), 50f);
            Physics.SyncTransforms();

            int damaged = AreaDamage.Apply(Origin + Vector3.up * 0.5f, Exploder, null, _buffer);

            Assert.AreEqual(2, damaged);
            Assert.AreEqual(40f, player.Current, Tolerance);
            Assert.AreEqual(180f, barricade.Current, Tolerance, "Yapıya ×2.");
            Assert.AreEqual(100f, far.Current, Tolerance, "Yarıçap dışı.");
            Assert.AreEqual(50f, enemy.Current, Tolerance, "Maskede olmayan layer.");
        }

        [Test]
        public void Apply_TargetWithTwoColliders_DamagedOnce()
        {
            Health player = Create("Player", GameLayers.Player, new Vector3(1f, 0f, 0f), 100f);
            var child = new GameObject("Extra") { layer = GameLayers.Player };
            child.transform.SetParent(player.transform, false);
            child.AddComponent<SphereCollider>().radius = 0.3f;
            Physics.SyncTransforms();

            Assert.AreEqual(1, AreaDamage.Apply(Origin, Exploder, null, _buffer));
            Assert.AreEqual(40f, player.Current, Tolerance);
        }

        [Test]
        public void Apply_SkipsDeadAndSource_HitsTriggerStructure()
        {
            Health dead = Create("Dead", GameLayers.Player, new Vector3(1f, 0f, 0f), 100f);
            dead.TakeDamage(new DamageInfo(1000f, DamageType.Bullet, null, Vector3.zero, Vector3.forward));
            Health source = Create("Source", GameLayers.Structure, new Vector3(0f, 0f, 1f), 100f);
            Health trap = Create("Trap", GameLayers.Structure, new Vector3(0f, 0f, -1.5f), 40f);
            trap.GetComponent<Collider>().isTrigger = true;
            Physics.SyncTransforms();

            int damaged = AreaDamage.Apply(Origin, Exploder, source.gameObject, _buffer);

            Assert.AreEqual(1, damaged);
            Assert.AreEqual(100f, source.Current, Tolerance, "Kaynak kendine hasar vermez.");
            Assert.IsFalse(trap.IsAlive, "Sadece trigger collider'ı olan tuzak da etkilenir (120 > 40).");
        }

        [Test]
        public void Apply_Falloff_UsesSurfaceDistance()
        {
            var spec = new AreaDamageSpec(4f, 100f, 1f, 0f, GameLayers.PlayerMask, DamageType.Explosion);
            // 1×1×1 kutunun merkezi 2.5 m'de; yüzeyi 2 m'de → oran 1 − 2/4 = 0.5.
            Health player = Create("Player", GameLayers.Player, new Vector3(2.5f, 0f, 0f), 100f);
            Physics.SyncTransforms();

            AreaDamage.Apply(Origin + Vector3.up * 0.5f, spec, null, _buffer);

            Assert.AreEqual(50f, player.Current, 0.01f);
        }

        [Test]
        public void Apply_ReportsDamageInfo()
        {
            Health player = Create("Player", GameLayers.Player, new Vector3(2f, 0f, 0f), 100f);
            var source = new GameObject("Exploder");
            _objects.Add(source);
            DamagedEvent received = default;
            player.Damaged += damaged => received = damaged;
            Physics.SyncTransforms();

            AreaDamage.Apply(Origin + Vector3.up * 0.5f, Exploder, source, _buffer);

            Assert.AreEqual(DamageType.Explosion, received.Damage.DamageType);
            Assert.AreSame(source, received.Damage.Source);
            Assert.AreEqual(1f, received.Damage.HitDirection.x, Tolerance, "Yön merkezden hedefe, yatay.");
            Assert.AreEqual(0f, received.Damage.HitDirection.y, Tolerance);
        }

        [Test]
        public void Apply_NoBufferOrZeroRadius_DoesNothing()
        {
            Health player = Create("Player", GameLayers.Player, new Vector3(1f, 0f, 0f), 100f);
            Physics.SyncTransforms();

            Assert.AreEqual(0, AreaDamage.Apply(Origin, Exploder, null, new Collider[0]));
            var zero = new AreaDamageSpec(0f, 60f, 1f, 1f, GameLayers.PlayerMask, DamageType.Explosion);
            Assert.AreEqual(0, AreaDamage.Apply(Origin, zero, null, _buffer));
            Assert.AreEqual(100f, player.Current, Tolerance);
        }

        /// <summary>Tabanı yerde 1×1×1 kutu collider'lı, canlı hedef.</summary>
        private Health Create(string name, int layer, Vector3 offset, float maxHealth)
        {
            var gameObject = new GameObject(name) { layer = layer };
            gameObject.transform.position = Origin + offset;
            var box = gameObject.AddComponent<BoxCollider>();
            box.center = Vector3.up * 0.5f;
            var health = gameObject.AddComponent<Health>();
            health.SetMax(maxHealth, refill: true);
            _objects.Add(gameObject);
            return health;
        }
    }
}
