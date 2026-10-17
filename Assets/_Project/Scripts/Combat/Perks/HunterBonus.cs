namespace LastHaven.Combat
{
    /// <summary>Avcı perk'i (GDD §13): bayrak açıksa yavaşlamış hedefe ek hasar. Saf hesap; <see cref="HitscanShooter"/> kullanır.</summary>
    public static class HunterBonus
    {
        /// <summary>Hasar çarpanı: Avcı açık ve hedef yavaşlamışsa 1 + bonus (negatif bonus 0 sayılır), değilse 1.</summary>
        public static float GetMultiplier(bool hunterActive, bool targetSlowed, float bonus)
        {
            return hunterActive && targetSlowed && bonus > 0f ? 1f + bonus : 1f;
        }
    }
}
