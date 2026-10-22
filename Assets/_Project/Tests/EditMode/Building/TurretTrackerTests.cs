using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Taret takibinin ilk tarama gecikmesi (15.2): aynı karede kurulan taretler rastgele gecikmeyle taramalarını ayrı karelere dağıtır;
    /// gecikme 0 iken ilk Tick hemen tarar. Sahnedeki objelerle karışmasın diye hedef haritanın çok dışında.
    /// </summary>
    public class TurretTrackerTests
    {
        private static readonly Vector3 Axis = new Vector3(6000f, 1.2f, 6000f);

        private TurretConfig _config;
        private GameObject _target;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<TurretConfig>();
            _target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _target.layer = GameLayers.Enemy;
            _target.AddComponent<Health>();
            _target.transform.position = Axis + new Vector3(3f, 0f, 0f);
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_target);
            Object.DestroyImmediate(_config);
            DamageableLookup.Clear();
        }

        [Test]
        public void Reset_WithoutDelay_ScansOnFirstTick()
        {
            var tracker = new TurretTracker(_config);
            tracker.Reset(0f);

            tracker.Tick(Axis, 10f, 0.01f, out _);

            Assert.IsTrue(tracker.Target.IsValid);
        }

        [Test]
        public void Reset_WithDelay_WaitsBeforeFirstScan()
        {
            var tracker = new TurretTracker(_config);
            tracker.Reset(0f, 0.15f);

            tracker.Tick(Axis, 10f, 0.1f, out _);
            Assert.IsFalse(tracker.Target.IsValid, "Gecikme dolmadan taranmaz.");

            tracker.Tick(Axis, 10f, 0.1f, out _);
            Assert.IsTrue(tracker.Target.IsValid);
        }
    }
}
