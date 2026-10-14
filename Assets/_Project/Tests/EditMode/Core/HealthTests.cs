using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Health köprüsünün EventBus'a doğru olayları gönderdiğini doğrular.</summary>
    public class HealthTests
    {
        private GameObject _gameObject;
        private Health _health;
        private int _damagedCount;
        private int _diedCount;
        private DamagedEvent _lastDamaged;
        private DiedEvent _lastDied;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("HealthTest");
            _health = _gameObject.AddComponent<Health>();
            _health.SetMax(50f, refill: true);
            _damagedCount = 0;
            _diedCount = 0;
            EventBus<DamagedEvent>.Subscribe(OnDamaged);
            EventBus<DiedEvent>.Subscribe(OnDied);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<DamagedEvent>.Unsubscribe(OnDamaged);
            EventBus<DiedEvent>.Unsubscribe(OnDied);
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void TakeDamage_RaisesDamagedEventWithPayload()
        {
            var damage = new DamageInfo(20f, DamageType.Bullet, null, Vector3.one, Vector3.forward);

            _health.TakeDamage(damage);

            Assert.AreEqual(1, _damagedCount);
            Assert.AreEqual(0, _diedCount);
            Assert.AreSame(_gameObject, _lastDamaged.Target);
            Assert.AreEqual(20f, _lastDamaged.AppliedAmount);
            Assert.AreEqual(30f, _lastDamaged.CurrentHealth);
            Assert.AreEqual(50f, _lastDamaged.MaxHealth);
            Assert.AreEqual(DamageType.Bullet, _lastDamaged.Damage.DamageType);
        }

        [Test]
        public void TakeDamage_Lethal_RaisesDiedOnceThenIgnoresFurtherDamage()
        {
            var damage = new DamageInfo(80f, DamageType.Explosion, null, Vector3.zero, Vector3.back);

            _health.TakeDamage(damage);
            _health.TakeDamage(damage);

            Assert.IsFalse(_health.IsAlive);
            Assert.AreEqual(1, _damagedCount);
            Assert.AreEqual(1, _diedCount);
            Assert.AreSame(_gameObject, _lastDied.Target);
            Assert.AreEqual(DamageType.Explosion, _lastDied.KillingBlow.DamageType);
        }

        private void OnDamaged(DamagedEvent evt)
        {
            _damagedCount++;
            _lastDamaged = evt;
        }

        private void OnDied(DiedEvent evt)
        {
            _diedCount++;
            _lastDied = evt;
        }
    }
}
