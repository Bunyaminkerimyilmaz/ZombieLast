using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// HitscanShooter gerçek fizik sorgusuyla: hasar, ShotHitEvent, ıskalama ve maske.
    /// Objeler açık sahnenin çok yukarısında kurulur; sahnedeki geometriye çarpmasın diye.
    /// </summary>
    public class HitscanShooterTests
    {
        private static readonly Vector3 Origin = new Vector3(0f, 500f, 0f);

        private readonly List<ShotHitEvent> _hits = new List<ShotHitEvent>();
        private readonly List<ShotTracedEvent> _traces = new List<ShotTracedEvent>();
        private readonly List<GameObject> _objects = new List<GameObject>();
        private HitscanShooter _shooter;

        [SetUp]
        public void SetUp()
        {
            _shooter = new HitscanShooter();
            _hits.Clear();
            _traces.Clear();
            DamageableLookup.Clear();
            EventBus<ShotHitEvent>.Subscribe(OnShotHit);
            EventBus<ShotTracedEvent>.Subscribe(OnShotTraced);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<ShotHitEvent>.Unsubscribe(OnShotHit);
            EventBus<ShotTracedEvent>.Unsubscribe(OnShotTraced);
            foreach (GameObject gameObject in _objects)
            {
                Object.DestroyImmediate(gameObject);
            }

            _objects.Clear();
            DamageableLookup.Clear();
        }

        [Test]
        public void Hit_DamagesTarget_AndRaisesShotHitEvent()
        {
            Health health = CreateTarget(Origin + Vector3.forward * 5f, GameLayers.Enemy, 100f);

            IReadOnlyList<ShotResult> results = _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].IsHit);
            Assert.AreSame(health, results[0].Damageable);
            Assert.AreEqual(80f, health.Current);

            Assert.AreEqual(1, _hits.Count);
            Assert.AreEqual(HitTargetType.Enemy, _hits[0].TargetType);
            Assert.AreSame(health.gameObject, _hits[0].Target);
            Assert.AreEqual(20f, _hits[0].Damage);
            Assert.AreEqual(Origin.z + 4.5f, _hits[0].Point.z, 0.01f, "Kutunun ön yüzü.");
        }

        [Test]
        public void Pellets_EachApplyDamage()
        {
            Health health = CreateTarget(Origin + Vector3.forward * 3f, GameLayers.Enemy, 1000f, size: 4f);

            _shooter.Fire(Request(damage: 12f, pellets: 8, range: 10f, spread: 18f));

            Assert.AreEqual(1000f - 8 * 12f, health.Current, 0.01f);
            Assert.AreEqual(8, _hits.Count);
        }

        [Test]
        public void OutOfRange_Misses_WithoutEvent()
        {
            CreateTarget(Origin + Vector3.forward * 20f, GameLayers.Enemy, 100f);

            IReadOnlyList<ShotResult> results = _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

            Assert.IsFalse(results[0].IsHit);
            Assert.AreEqual(Origin + Vector3.forward * 10f, results[0].EndPoint);
            Assert.AreEqual(0, _hits.Count);
        }

        [Test]
        public void LayerOutsideMask_IsIgnored()
        {
            Health structure = CreateTarget(Origin + Vector3.forward * 5f, GameLayers.Structure, 100f);

            _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

            Assert.AreEqual(100f, structure.Current);
            Assert.AreEqual(0, _hits.Count);
        }

        [Test]
        public void SurfaceWithoutHealth_RaisesEventWithZeroDamage()
        {
            CreateBox(Origin + Vector3.forward * 5f, GameLayers.Obstacle, 1f);

            _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

            Assert.AreEqual(1, _hits.Count);
            Assert.AreEqual(HitTargetType.Surface, _hits[0].TargetType);
            Assert.AreEqual(0f, _hits[0].Damage);
        }

        [Test]
        public void Miss_RaisesTraceToRangeEnd()
        {
            _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

            Assert.AreEqual(1, _traces.Count);
            Assert.IsFalse(_traces[0].IsHit);
            Assert.AreEqual(Origin, _traces[0].Start, "İz başlangıcı verilmezse ışının başlangıcı.");
            Assert.AreEqual(Origin + Vector3.forward * 10f, _traces[0].End);
            Assert.AreEqual(Vector3.forward, _traces[0].Direction);
            Assert.IsNull(_traces[0].WeaponId);
        }

        [Test]
        public void Hit_RaisesTraceFromTracerOriginToHitPoint()
        {
            CreateTarget(Origin + Vector3.forward * 5f, GameLayers.Enemy, 100f);
            Vector3 muzzle = Origin + Vector3.forward * 0.7f;
            var request = new HitscanRequest(Origin, Vector3.forward, 0f, 1, 10f, GameLayers.EnemyMask, 20f, null, "pistol", muzzle);

            _shooter.Fire(request);

            Assert.AreEqual(1, _traces.Count);
            Assert.IsTrue(_traces[0].IsHit);
            Assert.AreEqual("pistol", _traces[0].WeaponId);
            Assert.AreEqual(muzzle, _traces[0].Start);
            Assert.AreEqual(Origin.z + 4.5f, _traces[0].End.z, 0.01f, "Kutunun ön yüzü.");
        }

        [Test]
        public void Pellets_EachRaiseTrace_HitOrMiss()
        {
            CreateTarget(Origin + Vector3.forward * 3f, GameLayers.Enemy, 1000f, size: 0.2f);

            _shooter.Fire(Request(damage: 12f, pellets: 8, range: 10f, spread: 18f));

            Assert.AreEqual(8, _traces.Count);
            int hits = 0;
            foreach (ShotTracedEvent trace in _traces)
            {
                hits += trace.IsHit ? 1 : 0;
            }

            Assert.AreEqual(_hits.Count, hits, "İsabet eden her saçmanın izi isabet olarak işaretli.");
        }

        [Test]
        public void Hunter_SlowedTarget_TakesBonusDamage()
        {
            CombatPerkConfig perks = ScriptableObject.CreateInstance<CombatPerkConfig>();
            try
            {
                Health health = CreateTarget(Origin + Vector3.forward * 5f, GameLayers.Enemy, 100f);
                var receiver = health.gameObject.AddComponent<EnemyStatusEffects>();
                var modifiers = new RunModifiers();
                _shooter.SetPerks(modifiers, perks);

                receiver.ApplySlow(0.4f, 10f);
                Assert.IsTrue(receiver.IsSlowed);
                _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));
                Assert.AreEqual(80f, health.Current, 0.01f, "Bayrak yokken bonus yok.");

                modifiers.AddFlag(PerkFlag.Hunter);
                _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));
                Assert.AreEqual(50f, health.Current, 0.01f, "Avcı + yavaşlamış: 20 × 1.5 = 30.");
                Assert.AreEqual(30f, _hits[1].Damage, 0.01f, "Olaydaki hasar bonuslu.");

                ((IPoolable)receiver).OnDespawned(); // yavaşlatma temizlenir
                Assert.IsFalse(receiver.IsSlowed);
                _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));
                Assert.AreEqual(30f, health.Current, 0.01f, "Yavaşlamamış hedefe bonus yok.");
            }
            finally
            {
                Object.DestroyImmediate(perks);
                StatusEffectLookup.Clear();
            }
        }

        [Test]
        public void Hunter_WithoutPerkConfig_IsOff()
        {
            Health health = CreateTarget(Origin + Vector3.forward * 5f, GameLayers.Enemy, 100f);
            health.gameObject.AddComponent<EnemyStatusEffects>().ApplySlow(0.4f, 10f);
            var modifiers = new RunModifiers();
            modifiers.AddFlag(PerkFlag.Hunter);
            _shooter.SetPerks(modifiers, null);

            _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

            Assert.AreEqual(80f, health.Current, 0.01f, "Taretler gibi perk bağlanmamış atıcı.");
            StatusEffectLookup.Clear();
        }

        [Test]
        public void Ricochet_BouncesOnceToNearestOtherEnemy_AtHalfDamage()
        {
            CombatPerkConfig perks = ScriptableObject.CreateInstance<CombatPerkConfig>();
            try
            {
                Health primary = CreateTarget(Origin + Vector3.forward * 5f, GameLayers.Enemy, 100f);
                Health nearest = CreateTarget(Origin + new Vector3(3f, 0f, 5f), GameLayers.Enemy, 100f);
                Health farther = CreateTarget(Origin + new Vector3(6f, 0f, 5f), GameLayers.Enemy, 100f);
                var modifiers = new RunModifiers();
                modifiers.AddFlag(PerkFlag.Ricochet);
                _shooter.SetPerks(modifiers, perks);

                IReadOnlyList<ShotResult> results = _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

                Assert.AreEqual(1, results.Count, "Sekme sonuç listesine girmez.");
                Assert.AreEqual(80f, primary.Current, 0.01f);
                Assert.AreEqual(90f, nearest.Current, 0.01f, "En yakın ikinci düşmana %50.");
                Assert.AreEqual(100f, farther.Current, 0.01f, "Tek sekme: sekme tekrar sekmez.");
                Assert.AreEqual(2, _hits.Count);
                Assert.AreSame(nearest.gameObject, _hits[1].Target);
                Assert.AreEqual(10f, _hits[1].Damage, 0.01f);
                Assert.AreEqual(2, _traces.Count);
                Assert.AreEqual(results[0].EndPoint, _traces[1].Start, "Sekme izi ilk isabet noktasından başlar.");
                Assert.AreEqual(nearest.transform.position, _traces[1].End);
            }
            finally
            {
                Object.DestroyImmediate(perks);
            }
        }

        [Test]
        public void Ricochet_WithoutFlagOrTarget_DoesNotBounce()
        {
            CombatPerkConfig perks = ScriptableObject.CreateInstance<CombatPerkConfig>();
            try
            {
                Health primary = CreateTarget(Origin + Vector3.forward * 5f, GameLayers.Enemy, 100f);
                Health other = CreateTarget(Origin + new Vector3(3f, 0f, 5f), GameLayers.Enemy, 100f);
                var modifiers = new RunModifiers();
                _shooter.SetPerks(modifiers, perks);

                _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));
                Assert.AreEqual(100f, other.Current, 0.01f, "Bayrak kapalı.");

                modifiers.AddFlag(PerkFlag.Ricochet);
                other.transform.position = Origin + new Vector3(30f, 0f, 5f);
                Physics.SyncTransforms();
                _shooter.Fire(Request(damage: 20f, pellets: 1, range: 10f));

                Assert.AreEqual(60f, primary.Current, 0.01f);
                Assert.AreEqual(100f, other.Current, 0.01f, "Yarıçap dışında hedef yok.");
                Assert.AreEqual(2, _hits.Count);
            }
            finally
            {
                Object.DestroyImmediate(perks);
            }
        }

        [Test]
        public void Ricochet_Damage_IsFractionOfPellet()
        {
            Assert.AreEqual(6f, Ricochet.GetDamage(12f, 0.5f), 0.0001f);
            Assert.AreEqual(0f, Ricochet.GetDamage(12f, 0f), 0.0001f);
            Assert.AreEqual(0f, Ricochet.GetDamage(-5f, 0.5f), 0.0001f);
        }

        private static HitscanRequest Request(float damage, int pellets, float range, float spread = 0f)
        {
            LayerMask mask = GameLayers.EnemyMask | GameLayers.ObstacleMask;
            return new HitscanRequest(Origin, Vector3.forward, spread, pellets, range, mask, damage, null);
        }

        private Health CreateTarget(Vector3 position, int layer, float maxHealth, float size = 1f)
        {
            GameObject box = CreateBox(position, layer, size);
            var health = box.AddComponent<Health>();
            health.SetMax(maxHealth, refill: true);
            return health;
        }

        private GameObject CreateBox(Vector3 position, int layer, float size)
        {
            var box = new GameObject("HitscanTarget") { layer = layer };
            box.transform.position = position;
            box.AddComponent<BoxCollider>().size = Vector3.one * size;
            _objects.Add(box);
            Physics.SyncTransforms();
            return box;
        }

        private void OnShotHit(ShotHitEvent hit)
        {
            _hits.Add(hit);
        }

        private void OnShotTraced(ShotTracedEvent trace)
        {
            _traces.Add(trace);
        }
    }
}
