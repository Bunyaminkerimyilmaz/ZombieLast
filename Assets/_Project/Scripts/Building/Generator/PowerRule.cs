namespace LastHaven.Building
{
    /// <summary>Üssün güç kuralı (saf; GDD §12): jeneratör ayakta ve yakıt varken güç var. Evreye bakmaz (yakıt sadece gece azalır).</summary>
    public static class PowerRule
    {
        public static bool IsPowered(bool generatorAlive, int fuel)
        {
            return generatorAlive && fuel > 0;
        }
    }
}
