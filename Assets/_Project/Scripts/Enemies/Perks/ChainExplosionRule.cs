namespace LastHaven.Enemies
{
    /// <summary>Zincirleme Son (saf, GDD §13): hangi öldürme patlar.</summary>
    public static class ChainExplosionRule
    {
        /// <summary>
        /// Patlar mı: bayrak açık, öldüren oyuncu ve zar (<paramref name="roll"/>, 0 ≤ x &lt; 1) olasılığın altında.
        /// Olasılık 0 hiç, 1 her zaman patlatır.
        /// </summary>
        public static bool ShouldExplode(bool perkActive, bool killedByPlayer, float roll, float chance)
        {
            return perkActive && killedByPlayer && chance > 0f && roll < chance;
        }
    }
}
