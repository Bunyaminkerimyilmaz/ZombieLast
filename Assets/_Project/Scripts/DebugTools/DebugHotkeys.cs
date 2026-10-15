using System;
using UnityEngine.InputSystem;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Geliştirici kısayolu (F-tuşları) action'ları. Oyun girdisi olmadıkları için <c>IInputReader</c>'a eklenmez;
    /// her araç kendi action'ını kurar, OnEnable/OnDisable'da açıp kapar, OnDestroy'da Dispose eder.
    /// </summary>
    public static class DebugHotkeys
    {
        /// <summary>Basılınca <paramref name="onPressed"/>'i çağıran (kapalı) buton action'ı, ör. "&lt;Keyboard&gt;/f5".</summary>
        public static InputAction Create(string binding, Action onPressed)
        {
            var action = new InputAction(binding, InputActionType.Button, binding);
            action.performed += _ => onPressed();
            return action;
        }
    }
}
