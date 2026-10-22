using System;

namespace LastHaven.Progression
{
    /// <summary>
    /// Seviye eşiği formülü (GDD §13, saf): sonraki seviye için gereken XP = <see cref="BaseXp"/> + <see cref="XpPerLevel"/> ×
    /// mevcut seviye (10 + 6 × seviye: 1 → 16, 2 → 22, 3 → 28). Eşik en az 1'dir (sonsuz seviye atlama olmasın).
    /// </summary>
    public readonly struct LevelCurve
    {
        public LevelCurve(int baseXp, int xpPerLevel)
        {
            BaseXp = Math.Max(0, baseXp);
            XpPerLevel = Math.Max(0, xpPerLevel);
        }

        public int BaseXp { get; }

        public int XpPerLevel { get; }

        /// <summary><paramref name="level"/> seviyesinden bir sonrakine geçmek için gereken XP (negatif seviye 0 sayılır).</summary>
        public int XpToNext(int level)
        {
            long required = BaseXp + (long)XpPerLevel * Math.Max(0, level);
            return (int)Math.Min(int.MaxValue, Math.Max(1L, required));
        }
    }
}
