namespace LastHaven.Player
{
    /// <summary>
    /// <see cref="GameInputActions"/> action adları. Yuva action'ları <see cref="GameInputActions.SlotActionName"/> ile
    /// adlanır ("WeaponSlot1").
    /// </summary>
    public static class InputActionNames
    {
        public const string Move = "Move";
        public const string AimPointer = "AimPointer";
        public const string AimStick = "AimStick";
        public const string Fire = "Fire";
        public const string Reload = "Reload";
        public const string Dash = "Dash";
        public const string WeaponSlot = "WeaponSlot";
        public const string WeaponCycle = "WeaponCycle";
        public const string Interact = "Interact";
        public const string ToggleBuildMode = "ToggleBuildMode";
        public const string Shop = "Shop";
        public const string Ready = "Ready";
        public const string Pause = "Pause";

        /// <summary>Gamepad Start: basılı tutmak Ready, kısa basmak Pause (bkz. <see cref="InputReader"/>).</summary>
        public const string GamepadStart = "GamepadStart";

        public const string BuildSlot = "BuildSlot";
        public const string BuildCycle = "BuildCycle";
        public const string Rotate = "Rotate";
        public const string Upgrade = "Upgrade";
        public const string Sell = "Sell";
        public const string Confirm = "Confirm";
        public const string Cancel = "Cancel";

        /// <summary>UI modunda 1–3: menüdeki seçeneği (ör. seviye kartı) doğrudan seçer. Yuva action'ı ("OptionSlot1").</summary>
        public const string OptionSlot = "OptionSlot";
    }
}
