using System;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    public class HealthModelTests
    {
        [Test]
        public void Constructor_StartsFull()
        {
            var model = new HealthModel(100f);

            Assert.AreEqual(100f, model.Max);
            Assert.AreEqual(100f, model.Current);
            Assert.IsTrue(model.IsAlive);
        }

        [TestCase(0f)]
        [TestCase(-10f)]
        [TestCase(float.NaN)]
        public void Constructor_NonPositiveMax_Throws(float max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthModel(max));
        }

        [Test]
        public void TakeDamage_ReducesCurrentAndRaisesDamaged()
        {
            var model = new HealthModel(100f);
            float reported = 0f;
            model.Damaged += amount => reported = amount;

            float applied = model.TakeDamage(30f);

            Assert.AreEqual(30f, applied);
            Assert.AreEqual(30f, reported);
            Assert.AreEqual(70f, model.Current);
            Assert.IsTrue(model.IsAlive);
        }

        [TestCase(0f)]
        [TestCase(-5f)]
        [TestCase(float.NaN)]
        public void TakeDamage_NonPositiveAmount_Ignored(float amount)
        {
            var model = new HealthModel(100f);
            int damagedCalls = 0;
            model.Damaged += _ => damagedCalls++;

            float applied = model.TakeDamage(amount);

            Assert.AreEqual(0f, applied);
            Assert.AreEqual(100f, model.Current);
            Assert.AreEqual(0, damagedCalls);
        }

        [Test]
        public void TakeDamage_Lethal_ClampsToZeroAndRaisesDiedOnce()
        {
            var model = new HealthModel(50f);
            int diedCalls = 0;
            float reported = 0f;
            model.Damaged += amount => reported = amount;
            model.Died += () => diedCalls++;

            float applied = model.TakeDamage(80f);

            Assert.AreEqual(50f, applied, "Aşırı hasar kırpılmalı.");
            Assert.AreEqual(50f, reported);
            Assert.AreEqual(0f, model.Current);
            Assert.IsFalse(model.IsAlive);
            Assert.AreEqual(1, diedCalls);
        }

        [Test]
        public void TakeDamage_ExactlyRemainingHealth_Dies()
        {
            var model = new HealthModel(60f);
            model.TakeDamage(20f);

            model.TakeDamage(40f);

            Assert.AreEqual(0f, model.Current);
            Assert.IsFalse(model.IsAlive);
        }

        [Test]
        public void TakeDamage_WhenDead_IgnoredWithoutEvents()
        {
            var model = new HealthModel(10f);
            model.TakeDamage(10f);
            int damagedCalls = 0;
            int diedCalls = 0;
            model.Damaged += _ => damagedCalls++;
            model.Died += () => diedCalls++;

            float applied = model.TakeDamage(10f);

            Assert.AreEqual(0f, applied);
            Assert.AreEqual(0f, model.Current);
            Assert.AreEqual(0, damagedCalls);
            Assert.AreEqual(0, diedCalls);
        }

        [Test]
        public void Heal_DoesNotExceedMax()
        {
            var model = new HealthModel(100f);
            model.TakeDamage(30f);
            float reported = 0f;
            model.Healed += amount => reported = amount;

            float applied = model.Heal(50f);

            Assert.AreEqual(30f, applied);
            Assert.AreEqual(30f, reported);
            Assert.AreEqual(100f, model.Current);
        }

        [Test]
        public void Heal_AtFullHealth_NoEffectNoEvent()
        {
            var model = new HealthModel(100f);
            int healedCalls = 0;
            model.Healed += _ => healedCalls++;

            float applied = model.Heal(10f);

            Assert.AreEqual(0f, applied);
            Assert.AreEqual(0, healedCalls);
        }

        [Test]
        public void Heal_WhenDead_Ignored()
        {
            var model = new HealthModel(10f);
            model.TakeDamage(10f);

            float applied = model.Heal(5f);

            Assert.AreEqual(0f, applied);
            Assert.IsFalse(model.IsAlive);
        }

        [Test]
        public void SetMax_Lower_ClampsCurrent()
        {
            var model = new HealthModel(100f);

            model.SetMax(40f);

            Assert.AreEqual(40f, model.Max);
            Assert.AreEqual(40f, model.Current);
        }

        [Test]
        public void SetMax_Higher_KeepsCurrent()
        {
            var model = new HealthModel(100f);
            model.TakeDamage(20f);

            model.SetMax(150f);

            Assert.AreEqual(150f, model.Max);
            Assert.AreEqual(80f, model.Current);
        }

        [Test]
        public void SetMax_Refill_FillsAndRevives()
        {
            var model = new HealthModel(100f);
            model.TakeDamage(100f);

            model.SetMax(120f, refill: true);

            Assert.AreEqual(120f, model.Current);
            Assert.IsTrue(model.IsAlive);
        }

        [Test]
        public void SetMax_NonPositive_Throws()
        {
            var model = new HealthModel(100f);

            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetMax(0f));
        }

        [Test]
        public void Normalized_ReflectsRatio()
        {
            var model = new HealthModel(200f);
            model.TakeDamage(50f);

            Assert.AreEqual(0.75f, model.Normalized, 1e-5f);
        }
    }
}
