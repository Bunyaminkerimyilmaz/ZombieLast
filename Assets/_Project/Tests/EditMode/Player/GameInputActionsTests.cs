using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Player;
using NUnit.Framework;
using UnityEngine.InputSystem;
using Names = LastHaven.Player.InputActionNames;

namespace LastHaven.Tests.EditMode
{
    public class GameInputActionsTests
    {
        private GameInputActions _actions;

        [SetUp]
        public void SetUp()
        {
            _actions = new GameInputActions();
        }

        [TearDown]
        public void TearDown()
        {
            _actions.Dispose();
        }

        // GDD §4 kontrol tablosu.
        [TestCase(InputMode.Gameplay, Names.Move, "<Keyboard>/w")]
        [TestCase(InputMode.Gameplay, Names.Move, "<Keyboard>/a")]
        [TestCase(InputMode.Gameplay, Names.Move, "<Keyboard>/s")]
        [TestCase(InputMode.Gameplay, Names.Move, "<Keyboard>/d")]
        [TestCase(InputMode.Gameplay, Names.Move, "<Gamepad>/leftStick")]
        [TestCase(InputMode.Gameplay, Names.AimPointer, "<Mouse>/position")]
        [TestCase(InputMode.Gameplay, Names.AimStick, "<Gamepad>/rightStick")]
        [TestCase(InputMode.Gameplay, Names.Fire, "<Mouse>/leftButton")]
        [TestCase(InputMode.Gameplay, Names.Fire, "<Gamepad>/rightTrigger")]
        [TestCase(InputMode.Gameplay, Names.Reload, "<Keyboard>/r")]
        [TestCase(InputMode.Gameplay, Names.Reload, "<Gamepad>/buttonWest")]
        [TestCase(InputMode.Gameplay, Names.Dash, "<Keyboard>/space")]
        [TestCase(InputMode.Gameplay, Names.Dash, "<Gamepad>/buttonSouth")]
        [TestCase(InputMode.Gameplay, "WeaponSlot1", "<Keyboard>/1")]
        [TestCase(InputMode.Gameplay, "WeaponSlot3", "<Keyboard>/3")]
        [TestCase(InputMode.Gameplay, Names.WeaponCycle, "<Mouse>/scroll/y")]
        [TestCase(InputMode.Gameplay, Names.WeaponCycle, "<Gamepad>/buttonNorth")]
        [TestCase(InputMode.Gameplay, Names.Interact, "<Keyboard>/e")]
        [TestCase(InputMode.Gameplay, Names.Interact, "<Gamepad>/buttonEast")]
        [TestCase(InputMode.Gameplay, Names.ToggleBuildMode, "<Keyboard>/b")]
        [TestCase(InputMode.Gameplay, Names.ToggleBuildMode, "<Gamepad>/leftShoulder")]
        [TestCase(InputMode.Gameplay, Names.Shop, "<Keyboard>/tab")]
        [TestCase(InputMode.Gameplay, Names.Shop, "<Gamepad>/select")]
        [TestCase(InputMode.Gameplay, Names.Ready, "<Keyboard>/enter")]
        [TestCase(InputMode.Gameplay, Names.Pause, "<Keyboard>/escape")]
        [TestCase(InputMode.Gameplay, Names.GamepadStart, "<Gamepad>/start")]
        [TestCase(InputMode.BuildMode, Names.Move, "<Keyboard>/w")]
        [TestCase(InputMode.BuildMode, Names.Move, "<Gamepad>/leftStick")]
        [TestCase(InputMode.BuildMode, Names.AimPointer, "<Mouse>/position")]
        [TestCase(InputMode.BuildMode, Names.AimStick, "<Gamepad>/rightStick")]
        [TestCase(InputMode.BuildMode, Names.ToggleBuildMode, "<Keyboard>/b")]
        [TestCase(InputMode.BuildMode, Names.ToggleBuildMode, "<Gamepad>/leftShoulder")]
        [TestCase(InputMode.BuildMode, "BuildSlot1", "<Keyboard>/1")]
        [TestCase(InputMode.BuildMode, "BuildSlot5", "<Keyboard>/5")]
        [TestCase(InputMode.BuildMode, Names.BuildCycle, "<Gamepad>/dpad/left")]
        [TestCase(InputMode.BuildMode, Names.BuildCycle, "<Gamepad>/dpad/right")]
        [TestCase(InputMode.BuildMode, Names.Rotate, "<Keyboard>/q")]
        [TestCase(InputMode.BuildMode, Names.Rotate, "<Gamepad>/rightShoulder")]
        [TestCase(InputMode.BuildMode, Names.Upgrade, "<Keyboard>/u")]
        [TestCase(InputMode.BuildMode, Names.Upgrade, "<Gamepad>/dpad/up")]
        [TestCase(InputMode.BuildMode, Names.Sell, "<Keyboard>/x")]
        [TestCase(InputMode.BuildMode, Names.Sell, "<Gamepad>/dpad/down")]
        [TestCase(InputMode.BuildMode, Names.Confirm, "<Mouse>/leftButton")]
        [TestCase(InputMode.BuildMode, Names.Confirm, "<Gamepad>/rightTrigger")]
        [TestCase(InputMode.BuildMode, Names.Cancel, "<Mouse>/rightButton")]
        [TestCase(InputMode.BuildMode, Names.Cancel, "<Gamepad>/buttonEast")]
        [TestCase(InputMode.BuildMode, Names.Ready, "<Keyboard>/enter")]
        [TestCase(InputMode.BuildMode, Names.Pause, "<Keyboard>/escape")]
        [TestCase(InputMode.BuildMode, Names.GamepadStart, "<Gamepad>/start")]
        [TestCase(InputMode.UI, Names.Confirm, "<Keyboard>/enter")]
        [TestCase(InputMode.UI, Names.Confirm, "<Gamepad>/buttonSouth")]
        [TestCase(InputMode.UI, Names.Cancel, "<Keyboard>/escape")]
        [TestCase(InputMode.UI, Names.Cancel, "<Gamepad>/buttonEast")]
        [TestCase(InputMode.UI, Names.Shop, "<Keyboard>/tab")]
        [TestCase(InputMode.UI, Names.Shop, "<Gamepad>/select")]
        [TestCase(InputMode.UI, Names.Pause, "<Gamepad>/start")]
        [TestCase(InputMode.UI, "OptionSlot1", "<Keyboard>/1")]
        [TestCase(InputMode.UI, "OptionSlot3", "<Keyboard>/3")]
        public void Binding_MatchesGddTable(InputMode mode, string actionName, string path)
        {
            InputAction action = _actions.GetMap(mode).FindAction(actionName);

            Assert.IsNotNull(action, $"{mode} haritasında '{actionName}' yok.");
            CollectionAssert.Contains(BindingPaths(action), path);
        }

        [TestCase(InputMode.Gameplay)]
        [TestCase(InputMode.BuildMode)]
        [TestCase(InputMode.UI)]
        public void EachControl_IsBoundToSingleActionWithinMap(InputMode mode)
        {
            var owners = new Dictionary<string, string>();
            foreach (InputAction action in _actions.GetMap(mode).actions)
            {
                foreach (string path in BindingPaths(action))
                {
                    if (owners.TryGetValue(path, out string owner) && owner != action.name)
                    {
                        Assert.Fail($"{mode}: '{path}' hem '{owner}' hem '{action.name}' action'ına bağlı.");
                    }

                    owners[path] = action.name;
                }
            }
        }

        [Test]
        public void ModeSpecificActions_ExistOnlyInTheirMap()
        {
            Assert.IsNull(_actions.Gameplay.FindAction("BuildSlot1"));
            Assert.IsNull(_actions.Gameplay.FindAction(Names.Confirm));
            Assert.IsNull(_actions.Gameplay.FindAction(Names.Cancel));
            Assert.IsNull(_actions.BuildMode.FindAction(Names.Fire));
            Assert.IsNull(_actions.BuildMode.FindAction("WeaponSlot1"));
            Assert.IsNull(_actions.BuildMode.FindAction(Names.Interact));
            Assert.IsNull(_actions.UI.FindAction(Names.Move));
            Assert.IsNull(_actions.UI.FindAction(Names.Ready));
        }

        [Test]
        public void SlotActions_CoverKeysOneToCount()
        {
            Assert.IsNotNull(_actions.Gameplay.FindAction("WeaponSlot" + GameInputActions.WeaponSlotCount));
            Assert.IsNull(_actions.Gameplay.FindAction("WeaponSlot" + (GameInputActions.WeaponSlotCount + 1)));
            Assert.IsNotNull(_actions.BuildMode.FindAction("BuildSlot" + GameInputActions.BuildSlotCount));
            Assert.IsNull(_actions.BuildMode.FindAction("BuildSlot" + (GameInputActions.BuildSlotCount + 1)));
            Assert.IsNotNull(_actions.UI.FindAction("OptionSlot" + GameInputActions.OptionSlotCount), "13.3: seviye kartları 1–3.");
            Assert.IsNull(_actions.UI.FindAction("OptionSlot" + (GameInputActions.OptionSlotCount + 1)));
            Assert.IsNull(_actions.Gameplay.FindAction("OptionSlot1"), "Oyunda 1–3 silah yuvası.");
            Assert.AreEqual("WeaponSlot1", GameInputActions.SlotActionName(Names.WeaponSlot, 0));
        }

        [TestCase(InputMode.Gameplay)]
        [TestCase(InputMode.BuildMode)]
        public void GamepadStart_UsesHoldInteraction(InputMode mode)
        {
            InputAction action = _actions.GetMap(mode).FindAction(Names.GamepadStart);

            Assert.AreEqual(1, action.bindings.Count);
            Assert.AreEqual("hold", action.bindings[0].interactions);
        }

        [Test]
        public void UiPause_FiresOnPressWithoutHold()
        {
            InputAction action = _actions.UI.FindAction(Names.Pause);

            Assert.AreEqual(InputActionType.Button, action.type);
            Assert.IsTrue(string.IsNullOrEmpty(action.bindings[0].interactions));
        }

        [TestCase(InputMode.Gameplay, Names.WeaponCycle, "<Mouse>/scroll/y")]
        [TestCase(InputMode.BuildMode, Names.BuildCycle, "<Gamepad>/dpad/left")]
        public void CycleActions_InvertPreviousDirectionBinding(InputMode mode, string actionName, string invertedPath)
        {
            InputAction action = _actions.GetMap(mode).FindAction(actionName);

            Assert.AreEqual(InputActionType.PassThrough, action.type);
            foreach (InputBinding binding in action.bindings)
            {
                bool shouldInvert = binding.path == invertedPath;
                Assert.AreEqual(shouldInvert, binding.processors == "invert", binding.path);
            }
        }

        [TestCase(1f, 1)]
        [TestCase(120f, 1)]
        [TestCase(-0.01f, -1)]
        [TestCase(-120f, -1)]
        [TestCase(0f, 0)]
        public void ToCycleDirection_UsesSignAndIgnoresZero(float value, int expected)
        {
            Assert.AreEqual(expected, GameInputActions.ToCycleDirection(value));
        }

        [TestCase(InputMode.Gameplay)]
        [TestCase(InputMode.BuildMode)]
        [TestCase(InputMode.UI)]
        public void Enable_OpensOnlySelectedMap(InputMode mode)
        {
            _actions.Enable(InputMode.UI);
            _actions.Enable(mode);

            foreach (InputActionMap map in _actions.Maps)
            {
                Assert.AreEqual(map == _actions.GetMap(mode), map.enabled, map.name);
            }
        }

        [Test]
        public void Disable_ClosesAllMaps()
        {
            _actions.Enable(InputMode.Gameplay);
            _actions.Disable();

            foreach (InputActionMap map in _actions.Maps)
            {
                Assert.IsFalse(map.enabled, map.name);
            }
        }

        [Test]
        public void GetLocomotion_ReturnsOwnMapActions_AndNoneForUI()
        {
            Assert.AreSame(_actions.Gameplay, _actions.GetLocomotion(InputMode.Gameplay).Move.actionMap);
            Assert.AreSame(_actions.BuildMode, _actions.GetLocomotion(InputMode.BuildMode).AimStick.actionMap);
            Assert.IsNull(_actions.GetLocomotion(InputMode.UI));
        }

        private static List<string> BindingPaths(InputAction action)
        {
            var paths = new List<string>();
            foreach (InputBinding binding in action.bindings)
            {
                if (!binding.isComposite)
                {
                    paths.Add(binding.path);
                }
            }

            return paths;
        }
    }
}
