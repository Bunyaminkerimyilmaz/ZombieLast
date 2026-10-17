namespace LastHaven.Building
{
    /// <summary>
    /// Üssün güç kuralı (saf; GDD §12, §14): jeneratör ayakta, yakıt var ve gece olayının kesintisi (Fırtına) yokken güç var. Evreye
    /// bakmaz (yakıt sadece gece azalır).
    /// </summary>
    public static class PowerRule
    {
        public static bool IsPowered(bool generatorAlive, int fuel, bool outage = false)
        {
            return generatorAlive && fuel > 0 && !outage;
        }
    }
}
