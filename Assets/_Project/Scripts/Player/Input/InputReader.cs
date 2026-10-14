using System;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using Names = LastHaven.Player.InputActionNames;

namespace LastHaven.Player
{
    /// <summary>
    /// <see cref="IInputReader"/>'ın Input System köprüsü. Bağlamalar <see cref="GameInputActions"/>'ta kodda kurulur;
    /// bu bileşen action'ları olaylara bağlar, modu yönetir ve son kullanılan cihazı izler.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputReader : MonoBehaviour, IInputReader
    {
        private GameInputActions _actions;
        private LocomotionActions _locomotion;
        private InputAction _fire;
        private InputAction _interact;
        private InputMode _mode = InputMode.Gameplay;
        private Vector2 _aimScreenPosition;
        private bool _isUsingGamepad;
        private bool _startHoldPerformed;

        public event Action ReloadPressed;
        public event Action DashPressed;
        public event Action<int> WeaponSlotPressed;
        public event Action<int> WeaponCyclePressed;
        public event Action BuildModePressed;
        public event Action RotatePressed;
        public event Action UpgradePressed;
        public event Action SellPressed;
        public event Action<int> BuildSlotPressed;
        public event Action<int> BuildCyclePressed;
        public event Action CancelPressed;
        public event Action ShopPressed;
        public event Action ReadyPressed;
        public event Action PausePressed;
        public event Action ConfirmPressed;

        public InputMode Mode => _mode;
        public Vector2 MoveInput => _locomotion != null ? _locomotion.Move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 AimScreenPosition => _aimScreenPosition;
        public Vector2 AimStick => _locomotion != null ? _locomotion.AimStick.ReadValue<Vector2>() : Vector2.zero;
        public bool IsUsingGamepad => _isUsingGamepad;
        public bool IsFireHeld => IsHeld(_fire);
        public bool IsInteractHeld => IsHeld(_interact);

        public void SetMode(InputMode mode)
        {
            _mode = mode;
            _locomotion = _actions.GetLocomotion(mode);
            if (isActiveAndEnabled)
            {
                _actions.Enable(mode);
            }
        }

        private void Awake()
        {
            _actions = new GameInputActions();
            _fire = _actions.Gameplay.FindAction(Names.Fire, throwIfNotFound: true);
            _interact = _actions.Gameplay.FindAction(Names.Interact, throwIfNotFound: true);
            _locomotion = _actions.GetLocomotion(_mode);
            BindEvents();
            ServiceLocator.Register<IInputReader>(this);
        }

        private void OnEnable()
        {
            // Nişan action'ı PassThrough; ilk fare hareketine kadar tetiklenmez, başlangıç değeri buradan alınır.
            if (Mouse.current != null)
            {
                _aimScreenPosition = Mouse.current.position.ReadValue();
            }

            _actions.Enable(_mode);
        }

        private void OnDisable()
        {
            _actions.Disable();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IInputReader>(this);
            _actions.Dispose();
        }

        private void BindEvents()
        {
            BindPressed(Names.Reload, () => ReloadPressed?.Invoke());
            BindPressed(Names.Dash, () => DashPressed?.Invoke());
            BindPressed(Names.ToggleBuildMode, () => BuildModePressed?.Invoke());
            BindPressed(Names.Rotate, () => RotatePressed?.Invoke());
            BindPressed(Names.Upgrade, () => UpgradePressed?.Invoke());
            BindPressed(Names.Sell, () => SellPressed?.Invoke());
            BindPressed(Names.Cancel, () => CancelPressed?.Invoke());
            BindPressed(Names.Shop, () => ShopPressed?.Invoke());
            BindPressed(Names.Ready, () => ReadyPressed?.Invoke());
            BindPressed(Names.Pause, () => PausePressed?.Invoke());
            BindPressed(Names.Confirm, () => ConfirmPressed?.Invoke());
            BindSlots(Names.WeaponSlot, GameInputActions.WeaponSlotCount, slot => WeaponSlotPressed?.Invoke(slot));
            BindSlots(Names.BuildSlot, GameInputActions.BuildSlotCount, slot => BuildSlotPressed?.Invoke(slot));
            BindCycle(Names.WeaponCycle, direction => WeaponCyclePressed?.Invoke(direction));
            BindCycle(Names.BuildCycle, direction => BuildCyclePressed?.Invoke(direction));

            ForEachAction(Names.GamepadStart, action =>
            {
                action.started += _ => _startHoldPerformed = false;
                action.performed += OnGamepadStartHeld;
                action.canceled += OnGamepadStartCanceled;
            });

            ForEachAction(Names.AimPointer, action => action.performed += OnAimPointer);

            for (int i = 0; i < _actions.Maps.Count; i++)
            {
                _actions.Maps[i].actionTriggered += TrackDevice;
            }
        }

        /// <summary>Adı geçen action'ı içeren her haritada <paramref name="bind"/>'ı çalıştırır (kurulumda bir kez).</summary>
        private void ForEachAction(string actionName, Action<InputAction> bind)
        {
            for (int i = 0; i < _actions.Maps.Count; i++)
            {
                InputAction action = _actions.Maps[i].FindAction(actionName);
                if (action != null)
                {
                    bind(action);
                }
            }
        }

        private void BindPressed(string actionName, Action raise)
        {
            ForEachAction(actionName, action => action.performed += _ => raise());
        }

        private void BindSlots(string prefix, int count, Action<int> raise)
        {
            for (int i = 0; i < count; i++)
            {
                int slot = i;
                BindPressed(GameInputActions.SlotActionName(prefix, i), () => raise(slot));
            }
        }

        private void BindCycle(string actionName, Action<int> raise)
        {
            ForEachAction(actionName, action => action.performed += context =>
            {
                // PassThrough bırakmada ve tekerlek sıfırlanınca da 0 ile tetiklenir; onlar yok sayılır.
                int direction = GameInputActions.ToCycleDirection(context.ReadValue<float>());
                if (direction != 0)
                {
                    raise(direction);
                }
            });
        }

        private void OnGamepadStartHeld(InputAction.CallbackContext context)
        {
            _startHoldPerformed = true;
            ReadyPressed?.Invoke();
        }

        /// <summary>
        /// Hold süresi dolmadan bırakılan Start kısa basıştır → Pause. Mod değişince harita kapanırken de canceled gelir;
        /// tuş hâlâ basılıysa bu bırakma değildir ve Pause gönderilmez.
        /// </summary>
        private void OnGamepadStartCanceled(InputAction.CallbackContext context)
        {
            bool released = context.control != null && !context.control.IsPressed();
            if (!_startHoldPerformed && released)
            {
                PausePressed?.Invoke();
            }

            _startHoldPerformed = false;
        }

        private void OnAimPointer(InputAction.CallbackContext context)
        {
            _aimScreenPosition = context.ReadValue<Vector2>();
        }

        private void TrackDevice(InputAction.CallbackContext context)
        {
            InputDevice device = context.control?.device;
            if (device is Gamepad)
            {
                _isUsingGamepad = true;
            }
            else if (device is Keyboard || device is Mouse)
            {
                _isUsingGamepad = false;
            }
        }

        private static bool IsHeld(InputAction action)
        {
            // Awake'ten önce (ör. editörde araçlar property okurken) action'lar henüz yok.
            return action != null && action.enabled && action.IsPressed();
        }
    }
}
