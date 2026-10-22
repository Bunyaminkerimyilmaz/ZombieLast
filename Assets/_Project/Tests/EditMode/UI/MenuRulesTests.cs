using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// 14.3 menülerinin saf kuralları: duraklatma menüsünün açılma kuralı, menü girdisinin önceki moda dönmesi, ses kaydırıcısı yüzde
    /// adımı ve ana menü kamerasının yörüngesi.
    /// </summary>
    public class MenuRulesTests
    {
        // --- PauseMenuRules ---

        [TestCase(InputMode.Gameplay, GamePhase.Day)]
        [TestCase(InputMode.Gameplay, GamePhase.Night)]
        [TestCase(InputMode.Gameplay, GamePhase.Dawn)]
        [TestCase(InputMode.BuildMode, GamePhase.Day)]
        public void PauseMenu_OpensInGameplayOrBuildModeDuringRun(InputMode mode, GamePhase phase)
        {
            Assert.IsTrue(PauseMenuRules.CanOpen(mode, false, true, phase));
        }

        [Test]
        public void PauseMenu_NotWhileAnotherMenuHoldsInput()
        {
            Assert.IsFalse(PauseMenuRules.CanOpen(InputMode.UI, false, true, GamePhase.Day), "Market / kartlar açıkken Esc onlarındır.");
        }

        [Test]
        public void PauseMenu_NotWhilePausedByOthers()
        {
            Assert.IsFalse(PauseMenuRules.CanOpen(InputMode.Gameplay, true, true, GamePhase.Night));
        }

        [TestCase(GamePhase.Boot)]
        [TestCase(GamePhase.GameOver)]
        [TestCase(GamePhase.Victory)]
        public void PauseMenu_NotOutsideRun(GamePhase phase)
        {
            Assert.IsFalse(PauseMenuRules.CanOpen(InputMode.Gameplay, false, true, phase));
        }

        [Test]
        public void PauseMenu_WithoutGameState_IgnoresPhase()
        {
            Assert.IsTrue(PauseMenuRules.CanOpen(InputMode.Gameplay, false, false, GamePhase.GameOver));
        }

        [Test]
        public void PauseMenu_ServiceOverload_ReadsPauseAndPhase()
        {
            var pause = new FakePauseService();
            var state = new FakeGameState { CurrentPhase = GamePhase.Night };

            Assert.IsTrue(PauseMenuRules.CanOpen(InputMode.Gameplay, pause, state));
            pause.Pause(this);
            Assert.IsFalse(PauseMenuRules.CanOpen(InputMode.Gameplay, pause, state));
            Assert.IsTrue(PauseMenuRules.CanOpen(InputMode.Gameplay, null, null), "Servis yoksa açılır.");
        }

        // --- MenuInputScope ---

        [Test]
        public void MenuInput_FromBuildMode_ReturnsToBuildMode()
        {
            var input = new FakeInputReader();
            input.SetMode(InputMode.BuildMode);
            var scope = new MenuInputScope(input);

            scope.Enter();
            Assert.AreEqual(InputMode.UI, input.Mode);
            scope.Exit();

            Assert.AreEqual(InputMode.BuildMode, input.Mode);
        }

        [Test]
        public void MenuInput_FromGameplay_ReturnsToGameplay()
        {
            var input = new FakeInputReader();
            var scope = new MenuInputScope(input);

            scope.Enter();
            scope.Exit();

            Assert.AreEqual(InputMode.Gameplay, input.Mode);
        }

        [Test]
        public void MenuInput_EnteredWhileUI_ReturnsToGameplay()
        {
            var input = new FakeInputReader();
            input.SetMode(InputMode.UI);
            var scope = new MenuInputScope(input);

            scope.Enter();
            scope.Exit();

            Assert.AreEqual(InputMode.Gameplay, input.Mode);
        }

        [Test]
        public void MenuInput_ModeChangedMeanwhile_IsLeftAlone()
        {
            var input = new FakeInputReader();
            var scope = new MenuInputScope(input);
            scope.Enter();
            input.SetMode(InputMode.BuildMode);
            int changes = input.SetModeCount;

            scope.Exit();

            Assert.AreEqual(InputMode.BuildMode, input.Mode);
            Assert.AreEqual(changes, input.SetModeCount);
        }

        [Test]
        public void MenuInput_WithoutService_DoesNothing()
        {
            var scope = new MenuInputScope(null);

            Assert.DoesNotThrow(() =>
            {
                scope.Enter();
                scope.Exit();
            });
            Assert.AreEqual(InputMode.Gameplay, scope.Mode);
        }

        // --- VolumePercent ---

        [TestCase(1f, 100)]
        [TestCase(0f, 0)]
        [TestCase(0.456f, 46)]
        [TestCase(2f, 100)]
        public void Percent_FromVolume(float volume, int expected)
        {
            Assert.AreEqual(expected, VolumePercent.FromVolume(volume));
        }

        [Test]
        public void Percent_ToVolume_Clamped()
        {
            Assert.AreEqual(0.35f, VolumePercent.ToVolume(35), 1e-6f);
            Assert.AreEqual(1f, VolumePercent.ToVolume(140));
            Assert.AreEqual(0f, VolumePercent.ToVolume(-5));
        }

        [TestCase(50, 1, 5, 55)]
        [TestCase(50, -1, 5, 45)]
        [TestCase(98, 1, 5, 100)]
        [TestCase(2, -1, 5, 0)]
        [TestCase(50, 1, 0, 51)]
        public void Percent_Step(int value, int direction, int step, int expected)
        {
            Assert.AreEqual(expected, VolumePercent.Step(value, direction, step));
        }

        // --- OrbitPose ---

        [Test]
        public void Orbit_ZeroAngle_IsSouthOfCenter_LooksAtCenter()
        {
            OrbitPose.Evaluate(Vector3.zero, 10f, 5f, 0f, 0f, 0f, out Vector3 position, out Quaternion rotation);

            Assert.AreEqual(0f, position.x, 1e-4f);
            Assert.AreEqual(5f, position.y, 1e-4f);
            Assert.AreEqual(-10f, position.z, 1e-4f);
            Vector3 forward = rotation * Vector3.forward;
            Assert.AreEqual(0f, Vector3.Angle(forward, (Vector3.zero - position).normalized), 0.01f);
        }

        [Test]
        public void Orbit_KeepsRadiusAndHeight()
        {
            OrbitPose.Evaluate(new Vector3(2f, 0f, 3f), 19f, 9f, 1.5f, 137f, -12f, out Vector3 position, out _);

            Vector2 planar = new Vector2(position.x - 2f, position.z - 3f);
            Assert.AreEqual(19f, planar.magnitude, 1e-3f);
            Assert.AreEqual(9f, position.y, 1e-4f);
        }

        [Test]
        public void Orbit_NegativeYawOffset_PutsCenterOnRight()
        {
            OrbitPose.Evaluate(Vector3.zero, 19f, 9f, 0f, 35f, -12f, out Vector3 position, out Quaternion rotation);

            Vector3 local = Quaternion.Inverse(rotation) * (Vector3.zero - position);
            Assert.Greater(local.x, 0f, "Menü şeridi solda; üs ekranın sağında görünmeli.");
            Assert.Greater(local.z, 0f);
        }

        [Test]
        public void Orbit_AdvanceWrapsAt360()
        {
            Assert.AreEqual(10f, OrbitPose.Advance(350f, 20f, 1f), 1e-4f);
            Assert.AreEqual(355f, OrbitPose.Advance(5f, -10f, 1f), 1e-4f);
        }
    }
}
