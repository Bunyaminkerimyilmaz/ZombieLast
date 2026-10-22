namespace LastHaven.DebugTools
{
    /// <summary><c>UI/Debug/DebugPanel.uxml</c> element adları ve sınıfları (<see cref="DebugPanelView"/>). UXML ile eşleşmeli.</summary>
    public static class DebugPanelElementNames
    {
        public const string Screen = "debug-panel";
        public const string FpsValue = "fps-value";
        public const string EnemiesValue = "enemies-value";
        public const string PhaseValue = "phase-value";
        public const string NightValue = "night-value";
        public const string NightTotal = "night-total";
        public const string RemainingValue = "remaining-value";
        public const string RemainingTotal = "remaining-total";
        public const string TimeScaleValue = "time-scale-value";
        public const string GcValue = "gc-value";
        public const string GcFramesValue = "gc-frames-value";
        public const string StressInfo = "stress-info";
        public const string StressStartButton = "stress-start-button";
        public const string StressStopButton = "stress-stop-button";
        public const string StressAlive = "stress-alive";
        public const string StressTarget = "stress-target";
        public const string AddMoneyButton = "add-money-button";
        public const string AddFuelButton = "add-fuel-button";
        public const string DrainFuelButton = "drain-fuel-button";
        public const string InvulnerableToggle = "invulnerable-toggle";
        public const string SkipNightButton = "skip-night-button";
        public const string KillAllButton = "kill-all-button";
        public const string SpawnHordeButton = "spawn-horde-button";
        public const string NightButtons = "night-buttons";
        public const string LevelUpButton = "level-up-button";
        public const string RandomPerkButton = "random-perk-button";
        public const string PerkDropdown = "perk-dropdown";
        public const string GivePerkButton = "give-perk-button";
        public const string TimeScaleButtons = "time-scale-buttons";

        /// <summary>Görünürlük sınıfı (Theme.uss: <c>.lh-screen.hidden</c>; UI modülündeki <c>UIClassNames.Hidden</c> ile aynı).</summary>
        public const string HiddenClass = "hidden";

        /// <summary>Kodun ürettiği düğmelerin sınıfları (DebugPanel.uss).</summary>
        public const string ButtonClass = "debug-button";
        public const string SmallButtonClass = "debug-button--small";
        public const string SelectedButtonClass = "debug-button--selected";
    }
}
