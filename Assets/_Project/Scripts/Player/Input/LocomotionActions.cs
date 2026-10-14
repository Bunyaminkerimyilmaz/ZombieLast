using UnityEngine.InputSystem;

namespace LastHaven.Player
{
    /// <summary>Bir haritadaki hareket ve nişan action'ları. Gameplay ve BuildMode haritalarının ikisinde de vardır.</summary>
    public sealed class LocomotionActions
    {
        public LocomotionActions(InputActionMap map)
        {
            Move = map.FindAction(InputActionNames.Move, throwIfNotFound: true);
            AimPointer = map.FindAction(InputActionNames.AimPointer, throwIfNotFound: true);
            AimStick = map.FindAction(InputActionNames.AimStick, throwIfNotFound: true);
        }

        public InputAction Move { get; }
        public InputAction AimPointer { get; }
        public InputAction AimStick { get; }
    }
}
