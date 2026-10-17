namespace LastHaven.UI
{
    /// <summary>
    /// <c>UI/HUD/HUD.uxml</c>'deki element adları: kod ile UXML arasındaki sözleşme. UXML'de ad değişirse burayı da değiştir
    /// (EditMode testi kontrol eder).
    /// </summary>
    public static class HudElementNames
    {
        // Evre adları: her evrenin kendi etiketi var, metni UXML'de; sadece biri görünür.
        public const string PhaseDay = "phase-day";
        public const string PhaseDusk = "phase-dusk";
        public const string PhaseNight = "phase-night";
        public const string PhaseDawn = "phase-dawn";
        public const string PhaseGameOver = "phase-game-over";
        public const string PhaseVictory = "phase-victory";

        public const string DayTimer = "day-timer";
        public const string NightValue = "night-value";
        public const string RemainingGroup = "remaining-group";
        public const string RemainingValue = "remaining-value";

        public const string PlayerHealthFill = "player-health-fill";
        public const string PlayerHealthValue = "player-health-value";
        public const string GeneratorHealthFill = "generator-health-fill";
        public const string GeneratorHealthValue = "generator-health-value";

        public const string WeaponName = "weapon-name";
        public const string AmmoValue = "ammo-value";

        // Cüzdan (HudWalletController).
        public const string MoneyValue = "money-value";
        public const string FuelValue = "fuel-value";

        // Yakıt ve güç (HudFuelController).
        public const string FuelFill = "fuel-fill";
        public const string FuelEstimate = "fuel-estimate";
        public const string PowerWarning = "power-warning";

        // Kombo (HudComboController).
        public const string ComboGroup = "combo-group";
        public const string ComboCount = "combo-count";
        public const string ComboMultiplier = "combo-multiplier";
        public const string ComboWindowFill = "combo-window-fill";

        // İnşa çubuğu (HudBuildBarController).
        public const string BuildBar = "build-bar";
        public const string BuildCards = "build-cards";
        public const string BuildDescription = "build-description";
        public const string BuildNoSelection = "build-no-selection";

        // İnşa çubuğu kartı (UI/HUD/BuildCard.uxml, BuildCardView).
        public const string CardKey = "build-card-key";
        public const string CardPrice = "build-card-price";
        public const string CardSwatch = "build-card-swatch";
        public const string CardName = "build-card-name";

        // Yapı ipucu (HudStructureTooltipController).
        public const string StructureTooltip = "structure-tooltip";
        public const string TooltipName = "tooltip-name";
        public const string TooltipLevelName = "tooltip-level-name";
        public const string TooltipLevel = "tooltip-level";
        public const string TooltipHealthValue = "tooltip-health-value";
        public const string TooltipHealthFill = "tooltip-health-fill";
        public const string TooltipUpgrade = "tooltip-upgrade";
        public const string TooltipUpgradeCost = "tooltip-upgrade-cost";
        public const string TooltipMaxLevel = "tooltip-max-level";
        public const string TooltipSellRefund = "tooltip-sell-refund";

        // Etkileşim ipucu (HudInteractionPromptController).
        public const string InteractionPrompt = "interaction-prompt";
        public const string InteractionRing = "interaction-ring";
        public const string InteractionText = "interaction-text";

        // Ganimet sandığı kenar göstergeleri (HudLootIndicatorsController); göstergeler EdgeIndicatorView sınıflarıyla bulunur.
        public const string LootIndicators = "loot-indicators";

        // Gündüz istihbaratı: saldırı yönü okları (HudAttackIndicatorsController). Her yönün kendi göstergesi var.
        public const string AttackIndicators = "attack-indicators";
        public const string AttackIndicatorNorth = "attack-indicator-north";
        public const string AttackIndicatorEast = "attack-indicator-east";
        public const string AttackIndicatorSouth = "attack-indicator-south";
        public const string AttackIndicatorWest = "attack-indicator-west";

        // Gündüz istihbaratı: bu gecenin düşmanları ve olayı (HudIntelController); satırlar IntelRowView sınıflarıyla bulunur.
        public const string IntelPanel = "intel-panel";
        public const string IntelEnemies = "intel-enemies";
        public const string IntelEvent = "intel-event";
        public const string IntelEventName = "intel-event-name";
        public const string IntelEventDescription = "intel-event-description";

        // Gündüzün son uyarısı (HudDayWarningController).
        public const string DayWarning = "day-warning";
        public const string DayWarningPulse = "day-warning-pulse";
        public const string DayCountdown = "day-countdown";

        // "Hazırım" ipucu (HudReadyHintController).
        public const string ReadyHint = "ready-hint";
        public const string ReadyBonus = "ready-bonus";
        public const string ReadyBonusValue = "ready-bonus-value";

        // Boss can barı (HudBossController).
        public const string BossPanel = "boss-panel";
        public const string BossName = "boss-name";
        public const string BossHealthFill = "boss-health-fill";
        public const string BossHealthValue = "boss-health-value";

        /// <summary>Saldırı yönü göstergeleri, <see cref="Core.CompassDirection"/> sırasıyla (indeks = yön).</summary>
        public static readonly string[] AttackIndicatorsByDirection =
        {
            AttackIndicatorNorth, AttackIndicatorEast, AttackIndicatorSouth, AttackIndicatorWest,
        };

        /// <summary>Evre adı etiketleri (sıra önemsiz).</summary>
        public static readonly string[] PhaseLabels =
        {
            PhaseDay, PhaseDusk, PhaseNight, PhaseDawn, PhaseGameOver, PhaseVictory,
        };
    }
}
