namespace LastHaven.UI
{
    /// <summary>
    /// Şafak, Oyun Bitti ve Zafer panellerinin UXML element adları (<c>UI/Panels/</c>). Oyun Bitti ve Zafer aynı adları kullanır;
    /// ikisini de <see cref="RunEndPanelController"/> sürer. EditMode testi kontrol eder.
    /// </summary>
    public static class PanelElementNames
    {
        // DawnPanel.uxml
        public const string DawnNightValue = "dawn-night-value";
        public const string NightKillsValue = "night-kills-value";
        public const string NightMoneyValue = "night-money-value";
        public const string NightBestComboValue = "night-best-combo-value";
        public const string InterestValue = "interest-value";
        public const string ContinueButton = "continue-button";

        // GameOverPanel.uxml / VictoryPanel.uxml
        public const string RunEndNightValue = "run-end-night-value";
        public const string TotalMoneyValue = "total-money-value";
        public const string BestComboValue = "best-combo-value";

        /// <summary>Alınan perk'lerin satırı (kod her perk için bir <see cref="RunEndPerkChip"/> ekler).</summary>
        public const string RunEndPerks = "run-end-perks";

        /// <summary>Hiç perk alınmadıysa görünen yazı.</summary>
        public const string RunEndNoPerks = "run-end-no-perks";

        /// <summary>"Tekrar Oyna".</summary>
        public const string RestartButton = "restart-button";

        public const string RunEndMainMenuButton = "main-menu-button";

        // Hepsinde
        public const string TotalKillsValue = "total-kills-value";
    }
}
