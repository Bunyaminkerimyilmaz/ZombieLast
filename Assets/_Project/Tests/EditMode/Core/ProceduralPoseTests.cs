using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>ProceduralPose: öne atılma, sarsılma ve devrilme yedek animasyonları.</summary>
    public class ProceduralPoseTests
    {
        private const float Tolerance = 0.001f;

        private static readonly ProceduralPoseSettings Settings = new ProceduralPoseSettings(
            lungeDistance: 0.4f, lungeDuration: 0.2f,
            shakeAmplitude: 0.1f, shakeDuration: 0.2f, shakeFrequency: 25f,
            toppleAngle: 90f, toppleDuration: 0.4f, toppleLift: 0.5f);

        private ProceduralPose _pose;

        [SetUp]
        public void SetUp()
        {
            _pose = new ProceduralPose(Settings);
        }

        [Test]
        public void New_IsAtRest()
        {
            Assert.IsFalse(_pose.IsActive);
            Assert.AreEqual(Vector3.zero, _pose.Offset);
            Assert.AreEqual(Quaternion.identity, _pose.Rotation);
        }

        [Test]
        public void Lunge_PeaksForwardAtHalfDuration_ThenReturnsToRest()
        {
            _pose.PlayLunge();

            _pose.Tick(0.1f);
            Assert.AreEqual(0.4f, _pose.Offset.z, Tolerance);
            Assert.AreEqual(0f, _pose.Offset.x, Tolerance);

            _pose.Tick(0.1f);
            Assert.IsFalse(_pose.IsLunging);
            Assert.IsFalse(_pose.IsActive);
            Assert.AreEqual(Vector3.zero, _pose.Offset);
        }

        [Test]
        public void Shake_StaysWithinAmplitude_ThenStops()
        {
            _pose.PlayShake();

            float maxSideways = 0f;
            for (int i = 0; i < 19; i++)
            {
                _pose.Tick(0.01f);
                maxSideways = Mathf.Max(maxSideways, Mathf.Abs(_pose.Offset.x));
                Assert.AreEqual(0f, _pose.Offset.z, Tolerance, "Sarsılma sadece yana.");
            }

            Assert.Greater(maxSideways, 0f);
            Assert.LessOrEqual(maxSideways, 0.1f + Tolerance);

            _pose.Tick(0.02f);
            Assert.IsFalse(_pose.IsShaking);
            Assert.AreEqual(Vector3.zero, _pose.Offset);
        }

        [Test]
        public void Topple_FallsTowardDirection_AndStaysDown()
        {
            Vector3 direction = new Vector3(1f, 0f, 1f).normalized;
            _pose.PlayTopple(direction);

            _pose.Tick(0.4f);
            AssertDirection(direction, _pose.Rotation * Vector3.up);
            Assert.AreEqual(0.5f, _pose.Offset.y, Tolerance, "Yatan gövde yarıçapı kadar kalkar.");

            _pose.Tick(5f);
            Assert.IsTrue(_pose.IsToppled);
            AssertDirection(direction, _pose.Rotation * Vector3.up);
        }

        [Test]
        public void Topple_AcceleratesLikeAFall()
        {
            _pose.PlayTopple(Vector3.forward);

            _pose.Tick(0.2f);
            float halfTimeAngle = Quaternion.Angle(Quaternion.identity, _pose.Rotation);

            Assert.AreEqual(90f * 0.25f, halfTimeAngle, 0.1f);
        }

        [Test]
        public void Topple_SecondCall_IsIgnored()
        {
            _pose.PlayTopple(Vector3.right);
            _pose.PlayTopple(Vector3.left);

            _pose.Tick(0.4f);

            AssertDirection(Vector3.right, _pose.Rotation * Vector3.up);
        }

        [Test]
        public void Topple_FlattensDirection_AndDefaultsToForward()
        {
            _pose.PlayTopple(Vector3.up * 3f);
            _pose.Tick(0.4f);

            AssertDirection(Vector3.forward, _pose.Rotation * Vector3.up);
        }

        [Test]
        public void Reset_ReturnsToRest()
        {
            _pose.PlayLunge();
            _pose.PlayShake();
            _pose.PlayTopple(Vector3.back);
            _pose.Tick(0.1f);

            _pose.Reset();

            Assert.IsFalse(_pose.IsActive);
            Assert.AreEqual(Vector3.zero, _pose.Offset);
            Assert.AreEqual(Quaternion.identity, _pose.Rotation);
        }

        private static void AssertDirection(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
            Assert.AreEqual(expected.z, actual.z, Tolerance);
        }
    }
}
