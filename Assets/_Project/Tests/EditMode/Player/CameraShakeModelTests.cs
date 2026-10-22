using LastHaven.Player;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Kamera sarsıntısı (14.1): istekler kırpılır, uzaklıkla zayıflar, aynı karede toplanmaz (en güçlüsü), kapalıyken yok sayılır;
    /// kamera ayarlarının varsayılanı açık.
    /// </summary>
    public class CameraShakeModelTests
    {
        private const float Tolerance = 0.0001f;
        private const float FullRadius = 10f;
        private const float ZeroRadius = 30f;

        [Test]
        public void SameFrame_KeepsStrongest_NotSum()
        {
            var shake = new CameraShakeModel(1f, FullRadius, ZeroRadius, true);

            shake.Request(0.2f);
            shake.Request(0.5f);
            shake.Request(0.3f);

            Assert.IsTrue(shake.TryConsume(out float strength));
            Assert.AreEqual(0.5f, strength, Tolerance);
            Assert.IsFalse(shake.TryConsume(out _), "Alınan istek sıfırlanır.");
        }

        [Test]
        public void Request_ClampedToMax_NonPositiveIgnored()
        {
            var shake = new CameraShakeModel(0.8f, FullRadius, ZeroRadius, true);

            shake.Request(-1f);
            shake.Request(0f);
            Assert.IsFalse(shake.TryConsume(out _));

            shake.Request(3f);
            shake.TryConsume(out float strength);
            Assert.AreEqual(0.8f, strength, Tolerance);
        }

        [TestCase(0f, 1f)]
        [TestCase(10f, 1f)]
        [TestCase(20f, 0.5f)]
        [TestCase(30f, 0f)]
        [TestCase(50f, 0f)]
        public void Falloff_FullInside_LinearBetween_ZeroOutside(float distance, float expected)
        {
            Assert.AreEqual(expected, CameraShakeModel.Falloff(distance, FullRadius, ZeroRadius), Tolerance);
        }

        [Test]
        public void Falloff_ZeroRadiusNotLarger_StepAtFullRadius()
        {
            Assert.AreEqual(1f, CameraShakeModel.Falloff(5f, 10f, 10f));
            Assert.AreEqual(0f, CameraShakeModel.Falloff(10.5f, 10f, 5f));
        }

        [Test]
        public void PositionedRequest_Attenuated()
        {
            var shake = new CameraShakeModel(1f, FullRadius, ZeroRadius, true);

            shake.Request(0.6f, 20f);

            shake.TryConsume(out float strength);
            Assert.AreEqual(0.3f, strength, Tolerance);
        }

        [Test]
        public void Disabled_IgnoresRequests_DisablingClearsPending()
        {
            var shake = new CameraShakeModel(1f, FullRadius, ZeroRadius, true);
            shake.Request(0.5f);

            shake.Enabled = false;
            Assert.AreEqual(0f, shake.Pending, "Kapatınca bekleyen istek silinir.");
            shake.Request(0.5f);
            Assert.IsFalse(shake.TryConsume(out _));

            shake.Enabled = true;
            shake.Request(0.4f);
            Assert.IsTrue(shake.TryConsume(out _));
        }

        [Test]
        public void CameraConfig_ShakeOnByDefault_ModelFollowsFlag()
        {
            var config = ScriptableObject.CreateInstance<CameraConfig>();
            try
            {
                Assert.IsTrue(config.ShakeEnabled, "GDD §16: sarsıntı varsayılan açık (ayarlardan kapatılır).");
                Assert.IsTrue(config.CreateShakeModel().Enabled);
                Assert.Greater(config.ShakeAmplitude, 0f);
                Assert.Greater(config.ShakeDuration, 0f);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
