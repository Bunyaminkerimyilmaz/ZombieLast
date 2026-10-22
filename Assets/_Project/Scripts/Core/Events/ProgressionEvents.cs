namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncunun XP'si değişince gönderilir (GDD §13), seviye atlamaları işlendikten sonra (değerler son durum). Aynı kazançla
    /// atlanan her seviye için ardından bir <see cref="LevelUpEvent"/> gelir. HUD XP barı dinler.
    /// </summary>
    public readonly struct XpChangedEvent : IEvent
    {
        public XpChangedEvent(int level, int xp, int xpToNextLevel, int totalXp, int gained)
        {
            Level = level;
            Xp = xp;
            XpToNextLevel = xpToNextLevel;
            TotalXp = totalXp;
            Gained = gained;
        }

        /// <summary>Şu anki seviye (1 tabanlı).</summary>
        public int Level { get; }

        /// <summary>Bu seviyede biriken XP (0 ≤ Xp &lt; <see cref="XpToNextLevel"/>).</summary>
        public int Xp { get; }

        /// <summary>Sonraki seviye için bu seviyede gereken toplam XP (GDD §13: 10 + 6 × seviye).</summary>
        public int XpToNextLevel { get; }

        /// <summary>Run boyunca kazanılan toplam XP.</summary>
        public int TotalXp { get; }

        /// <summary>Bu değişimde kazanılan XP.</summary>
        public int Gained { get; }
    }

    /// <summary>
    /// Oyuncu bir seviye atlayınca gönderilir (GDD §13), atlanan her seviye için bir kez ve sırayla, aynı kazancın
    /// <see cref="XpChangedEvent"/>'inden sonra. Seviye bu olaydan önce bekleyen kuyruğa girer
    /// (<see cref="IExperienceService.TryDequeueLevelUp"/>); seviye kartları (13.3, UI) kuyruktan sırayla alır ve oyunu duraklatır.
    /// </summary>
    public readonly struct LevelUpEvent : IEvent
    {
        public LevelUpEvent(int level, int pendingLevelUps)
        {
            Level = level;
            PendingLevelUps = pendingLevelUps;
        }

        /// <summary>Ulaşılan seviye (1 tabanlı).</summary>
        public int Level { get; }

        /// <summary>
        /// Olay anında kuyrukta bekleyen (perk'i seçilmemiş) seviye sayısı. Bu seviye ve aynı kazançla atlanan sonraki seviyeler
        /// de dahildir (hepsi ilk olaydan önce kuyruğa girer): kuyruk boşken tek kazançla iki seviye atlanırsa iki olay da 2 der.
        /// </summary>
        public int PendingLevelUps { get; }
    }

    /// <summary>
    /// Oyuncu bir perk aldı (GDD §13), etkileri uygulandıktan sonra (statlar, bayraklar, iyileşme). Yığın başına bir kez.
    /// Gönderen: <see cref="IPerkService.ApplyPerk"/>. HUD perk listesi dinler (13.3); istatistik ekranı (14.3) dinleyecek.
    /// </summary>
    public readonly struct PerkAcquiredEvent : IEvent
    {
        public PerkAcquiredEvent(string perkId, string displayName, int stacks, int maxStacks)
        {
            PerkId = perkId;
            DisplayName = displayName;
            Stacks = stacks;
            MaxStacks = maxStacks;
        }

        /// <summary>Perk id'si (ordinal).</summary>
        public string PerkId { get; }

        /// <summary>Kartta görünen ad.</summary>
        public string DisplayName { get; }

        /// <summary>Bu alımdan sonraki yığın sayısı (1 tabanlı).</summary>
        public int Stacks { get; }

        /// <summary>Perk'in en fazla yığını; <see cref="Stacks"/> buna ulaşınca perk tekrar sunulmaz.</summary>
        public int MaxStacks { get; }
    }
}
