using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Knockback: direnç, mesafenin süreye yayılması, son kare kırpması ve yeniden başlatma (GDD §7: 0.3 m).</summary>
    public class KnockbackTests
    {
        private const float Tolerance = 0.0001f;

        private Knockback _knockback;

        [SetUp]
        public void SetUp()
        {
            _knockback = new Knockback();
        }

        [TestCase(0f, 0.3f)]
        [TestCase(0.5f, 0.15f)]
        [TestCase(1f, 0f)]
        [TestCase(2f, 0f)]
        [TestCase(-1f, 0.3f)]
        public void ResistedDistance_ScalesByResistance(float resistance, float expected)
        {
            Assert.AreEqual(expected, Knockback.ResistedDistance(0.3f, resistance), Tolerance);
        }

        [Test]
        public void Advance_SpreadsDistanceOverDuration_AndClipsLastStep()
        {
            Assert.IsTrue(_knockback.Start(Vector3.right, 0.3f, 0.1f));

            Vector3 total = Vector3.zero;
            total += _knockback.Advance(0.04f);
            Assert.AreEqual(0.12f, total.x, Tolerance);

            total += _knockback.Advance(0.04f);
            total += _knockback.Advance(0.04f);

            Assert.AreEqual(0.3f, total.x, Tolerance, "Toplam tam mesafe; fazlası kırpılır.");
            Assert.IsFalse(_knockback.IsActive);
            Assert.AreEqual(Vector3.zero, _knockback.Advance(0.04f));
        }

        [Test]
        public void Start_FlattensAndNormalizesDirection()
        {
            _knockback.Start(new Vector3(0f, 5f, 2f), 1f, 0f);

            Vector3 step = _knockback.Advance(0.01f);

            Assert.AreEqual(0f, step.y, Tolerance);
            Assert.AreEqual(1f, step.z, Tolerance);
        }

        [Test]
        public void Start_ZeroDuration_CompletesOnFirstAdvance()
        {
            _knockback.Start(Vector3.back, 0.3f, 0f);

            Assert.AreEqual(-0.3f, _knockback.Advance(0.001f).z, Tolerance);
            Assert.IsFalse(_knockback.IsActive);
        }

        [Test]
        public void Start_ZeroDistanceOrVerticalDirection_IsIgnored()
        {
            Assert.IsFalse(_knockback.Start(Vector3.forward, 0f, 0.1f), "Direnç 1 (Tank) → itilmez.");
            Assert.IsFalse(_knockback.Start(Vector3.up, 0.3f, 0.1f));
            Assert.IsFalse(_knockback.IsActive);
        }

        [Test]
        public void Start_WhileActive_RestartsInsteadOfStacking()
        {
            // Pompalının 8 saçması aynı karede gelir: toplam 0.3 m, 2.4 m değil.
            for (int i = 0; i < 8; i++)
            {
                _knockback.Start(Vector3.forward, 0.3f, 0.1f);
            }

            Assert.AreEqual(0.3f, _knockback.RemainingDistance, Tolerance);

            _knockback.Advance(0.05f);
            _knockback.Start(Vector3.left, 0.3f, 0.1f);

            Assert.AreEqual(0.3f, _knockback.RemainingDistance, Tolerance);
            Assert.AreEqual(Vector3.left, _knockback.Direction);
        }

        [Test]
        public void Stop_EndsPush()
        {
            _knockback.Start(Vector3.forward, 0.3f, 0.1f);

            _knockback.Stop();

            Assert.IsFalse(_knockback.IsActive);
            Assert.AreEqual(Vector3.zero, _knockback.Advance(0.05f));
        }
    }
}
