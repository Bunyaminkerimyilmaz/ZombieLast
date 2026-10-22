namespace LastHaven.UI
{
    /// <summary>
    /// Seviye kartları panelinin (<c>UI/LevelUp/LevelUpPanel.uxml</c>) ve kart şablonunun (<c>UI/LevelUp/PerkCard.uxml</c>) element
    /// adları. EditMode testi kontrol eder.
    /// </summary>
    public static class LevelUpElementNames
    {
        // LevelUpPanel.uxml
        public const string LevelValue = "levelup-level-value";
        public const string Cards = "levelup-cards";
        public const string Pending = "levelup-pending";
        public const string PendingValue = "levelup-pending-value";

        // PerkCard.uxml
        public const string CardKey = "perk-card-key";
        public const string CardIcon = "perk-card-icon";
        public const string CardInitials = "perk-card-initials";
        public const string CardName = "perk-card-name";
        public const string CardDescription = "perk-card-description";
        public const string CardStacks = "perk-card-stacks";
    }
}
