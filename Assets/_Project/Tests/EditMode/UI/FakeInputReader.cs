using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Modu elle değişen girdi (sahnesiz). Olaylar kullanılmaz; sadece mod ve cihaz.</summary>
    internal sealed class FakeInputReader : IInputReader
    {
        public InputMode Mode { get; private set; } = InputMode.Gameplay;
        public int SetModeCount { get; private set; }
        public Vector2 MoveInput => Vector2.zero;
        public Vector2 AimScreenPosition => Vector2.zero;
        public Vector2 AimStick => Vector2.zero;
        public bool IsUsingGamepad { get; set; }
        public bool IsFireHeld => false;
        public bool IsInteractHeld => false;

        public event Action ReloadPressed { add { } remove { } }
        public event Action DashPressed { add { } remove { } }
        public event Action<int> WeaponSlotPressed { add { } remove { } }
        public event Action<int> WeaponCyclePressed { add { } remove { } }
        public event Action BuildModePressed { add { } remove { } }
        public event Action RotatePressed { add { } remove { } }
        public event Action UpgradePressed { add { } remove { } }
        public event Action SellPressed { add { } remove { } }
        public event Action<int> BuildSlotPressed { add { } remove { } }
        public event Action<int> BuildCyclePressed { add { } remove { } }
        public event Action CancelPressed { add { } remove { } }
        public event Action ShopPressed { add { } remove { } }
        public event Action ReadyPressed { add { } remove { } }
        public event Action PausePressed { add { } remove { } }
        public event Action ConfirmPressed { add { } remove { } }
        public event Action<int> OptionPressed { add { } remove { } }

        public void SetMode(InputMode mode)
        {
            Mode = mode;
            SetModeCount++;
        }
    }
}
