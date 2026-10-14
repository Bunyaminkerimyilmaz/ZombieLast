using LastHaven.Core;
using LastHaven.Player;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>CameraConfig: GDD §4 preset'leri ve kameraya göre hareketin her preset'te W = ekranın yukarısı olması.</summary>
    public class CameraConfigTests
    {
        private const float Tolerance = 0.0001f;

        private CameraConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<CameraConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Defaults_MatchGdd()
        {
            Assert.AreEqual(CameraPreset.Isometric, _config.DefaultPreset);
            Assert.AreEqual(3f, _config.LookAheadDistance);
            AssertPreset(_config.GetPreset(CameraPreset.Isometric), 50f, 45f, 22f, 35f);
            AssertPreset(_config.GetPreset(CameraPreset.TopDown), 75f, 0f, 24f, 40f);
        }

        [TestCase(CameraPreset.Isometric)]
        [TestCase(CameraPreset.TopDown)]
        public void FollowOffset_PointsBackAlongViewAtDistance(CameraPreset presetId)
        {
            CameraPresetSettings preset = _config.GetPreset(presetId);
            Vector3 offset = preset.FollowOffset;
            Vector3 forward = preset.Rotation * Vector3.forward;

            Assert.AreEqual(preset.Distance, offset.magnitude, Tolerance);
            Assert.AreEqual(-1f, Vector3.Dot(offset.normalized, forward), Tolerance);
            Assert.Greater(offset.y, 0f, "Kamera hedefin üstünde olmalı.");
        }

        [TestCase(CameraPreset.Isometric)]
        [TestCase(CameraPreset.TopDown)]
        public void MoveUp_IsScreenUpOnGround(CameraPreset presetId)
        {
            CameraPresetSettings preset = _config.GetPreset(presetId);
            Vector3 screenUp = preset.Rotation * Vector3.up;
            screenUp.y = 0f;

            Vector3 move = PlanarMath.CameraRelative(Vector2.up, preset.Yaw);

            AssertVector(screenUp.normalized, move);
        }

        [TestCase(CameraPreset.Isometric)]
        [TestCase(CameraPreset.TopDown)]
        public void MoveRight_IsScreenRight(CameraPreset presetId)
        {
            CameraPresetSettings preset = _config.GetPreset(presetId);

            Vector3 move = PlanarMath.CameraRelative(Vector2.right, preset.Yaw);

            AssertVector(preset.Rotation * Vector3.right, move);
        }

        private static void AssertPreset(CameraPresetSettings preset, float pitch, float yaw, float distance, float fov)
        {
            Assert.AreEqual(pitch, preset.Pitch);
            Assert.AreEqual(yaw, preset.Yaw);
            Assert.AreEqual(distance, preset.Distance);
            Assert.AreEqual(fov, preset.FieldOfView);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, $"x: beklenen {expected}, gelen {actual}");
            Assert.AreEqual(expected.y, actual.y, Tolerance, $"y: beklenen {expected}, gelen {actual}");
            Assert.AreEqual(expected.z, actual.z, Tolerance, $"z: beklenen {expected}, gelen {actual}");
        }
    }
}
