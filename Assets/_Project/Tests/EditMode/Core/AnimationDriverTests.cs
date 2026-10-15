using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// AnimationDriver: Animator yokken prosedürel yedeğin Visual'a uygulanması ve sıfırlama.
    /// EditMode'da Awake çalışmaz; sürücü tembel kurulur.
    /// </summary>
    public class AnimationDriverTests
    {
        private const float Tolerance = 0.001f;

        private GameObject _root;
        private Transform _visual;
        private AnimationDriver _driver;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Entity");
            _visual = new GameObject(VisualRoot.Name).transform;
            _visual.SetParent(_root.transform, false);
            _driver = _root.AddComponent<AnimationDriver>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void FindsVisualChild_AndUsesFallbackWithoutAnimator()
        {
            Assert.AreSame(_visual, _driver.Visual);
            Assert.IsFalse(_driver.UsesAnimator);
        }

        [Test]
        public void AnimatorWithoutController_UsesFallback()
        {
            _visual.gameObject.AddComponent<Animator>();

            Assert.IsFalse(_driver.UsesAnimator);
        }

        [Test]
        public void TriggerAttack_LungesVisualForward_ThenReturns()
        {
            _driver.TriggerAttack();

            _driver.Advance(_driver.Pose.Settings.LungeDuration * 0.5f);
            Assert.Greater(_visual.localPosition.z, 0f);

            _driver.Advance(_driver.Pose.Settings.LungeDuration);
            Assert.AreEqual(Vector3.zero, _visual.localPosition);
        }

        [Test]
        public void TriggerDie_TopplesVisual_UntilReset()
        {
            _driver.TriggerDie();
            _driver.Advance(_driver.Pose.Settings.ToppleDuration);

            float tilt = Vector3.Angle(Vector3.up, _visual.up);
            Assert.AreEqual(_driver.Pose.Settings.ToppleAngle, tilt, 0.1f);
            Assert.AreEqual(0f, _visual.up.y, Tolerance, "90° devrilince yatay yatar.");

            _driver.ResetPose();

            Assert.AreEqual(Vector3.zero, _visual.localPosition);
            Assert.AreEqual(Quaternion.identity, _visual.localRotation);
        }

        [Test]
        public void SpeedDashingFireReload_WithoutAnimator_DoNothing()
        {
            _driver.SetSpeed(5f);
            _driver.SetDashing(true);
            _driver.TriggerFire();
            _driver.TriggerReload();

            Assert.IsFalse(_driver.Pose.IsActive);
        }

        [Test]
        public void OnSpawned_ResetsToppledPose()
        {
            _driver.TriggerDie();
            _driver.Advance(1f);

            ((IPoolable)_driver).OnSpawned();

            Assert.IsFalse(_driver.Pose.IsActive);
            Assert.AreEqual(Quaternion.identity, _visual.localRotation);
        }
    }
}
