namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncunun XP'si ve seviyesi (GDD §13; Progression'daki <c>ExperienceService</c>, [Systems]/Progression, Awake'te kaydolur).
    /// Seviye 1 tabanlıdır. Atlanan her seviye bekleyen kuyruğuna girer; perk seçimi kuyruktan sırayla alır. Değişimler
    /// <see cref="XpChangedEvent"/> ve <see cref="LevelUpEvent"/> ile duyurulur; başlangıç değeri için olay yoktur.
    /// </summary>
    public interface IExperienceService
    {
        /// <summary>Şu anki seviye (1 tabanlı).</summary>
        int Level { get; }

        /// <summary>Bu seviyede biriken XP.</summary>
        int Xp { get; }

        /// <summary>Sonraki seviye için bu seviyede gereken toplam XP.</summary>
        int XpToNextLevel { get; }

        /// <summary>Run boyunca kazanılan toplam XP.</summary>
        int TotalXp { get; }

        /// <summary>Kuyrukta bekleyen (perk'i henüz seçilmemiş) seviye sayısı.</summary>
        int PendingLevelUps { get; }

        /// <summary>Bekleyen en eski seviyeyi kuyruktan çıkarır. Kuyruk boşsa false.</summary>
        bool TryDequeueLevelUp(out int level);
    }
}
