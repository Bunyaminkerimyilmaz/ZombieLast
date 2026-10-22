using System;
using System.Collections.Generic;

namespace LastHaven.Progression
{
    /// <summary>
    /// XP ve seviye (GDD §13, saf). <see cref="AddXp"/> XP'yi biriktirir; eşik (<see cref="LevelCurve"/>) dolunca seviye artar,
    /// artan XP sonraki seviyeye devreder. Tek kazançla birden fazla seviye atlanabilir: atlanan her seviye sırayla bekleyen
    /// kuyruğuna girer ve <see cref="TryDequeueLevelUp"/> ile alınana kadar orada kalır (perk seçimi).
    /// </summary>
    public sealed class ExperienceTracker
    {
        private readonly LevelCurve _curve;
        private readonly Queue<int> _pending = new Queue<int>();

        public ExperienceTracker(LevelCurve curve, int startingLevel = 1)
        {
            _curve = curve;
            Level = Math.Max(1, startingLevel);
        }

        /// <summary>Şu anki seviye (1 tabanlı).</summary>
        public int Level { get; private set; }

        /// <summary>Bu seviyede biriken XP.</summary>
        public int Xp { get; private set; }

        /// <summary>Run boyunca kazanılan toplam XP (int.MaxValue'da doyar).</summary>
        public int TotalXp { get; private set; }

        /// <summary>Sonraki seviye için bu seviyede gereken toplam XP.</summary>
        public int XpToNextLevel => _curve.XpToNext(Level);

        /// <summary>Bu seviyenin doluluğu (0..1).</summary>
        public float Progress => Math.Min(1f, (float)Xp / XpToNextLevel);

        /// <summary>Kuyrukta bekleyen seviye sayısı.</summary>
        public int PendingLevelUps => _pending.Count;

        /// <summary>
        /// XP ekler ve atlanan seviye sayısını döndürür (0 = seviye atlanmadı). Atlanan seviyeler kuyruğa sırayla girer;
        /// ulaşılan seviyeler <c>Level - dönüş + 1 … Level</c>. Negatif miktar <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        public int AddXp(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "XP negatif olamaz.");
            }

            if (amount == 0)
            {
                return 0;
            }

            TotalXp = (int)Math.Min(int.MaxValue, (long)TotalXp + amount);
            long xp = (long)Xp + amount;
            int levelsGained = 0;
            int required = XpToNextLevel;
            while (xp >= required && Level < int.MaxValue)
            {
                xp -= required;
                Level++;
                _pending.Enqueue(Level);
                levelsGained++;
                required = XpToNextLevel;
            }

            Xp = (int)Math.Min(int.MaxValue, xp);
            return levelsGained;
        }

        /// <summary>Bekleyen en eski seviyeyi kuyruktan çıkarır. Kuyruk boşsa false.</summary>
        public bool TryDequeueLevelUp(out int level)
        {
            if (_pending.Count == 0)
            {
                level = 0;
                return false;
            }

            level = _pending.Dequeue();
            return true;
        }
    }
}
