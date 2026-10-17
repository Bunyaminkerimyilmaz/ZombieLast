namespace LastHaven.Building
{
    /// <summary>
    /// Tuzağın bir seviyedeki değerleri (GDD §11): <see cref="BuildableStat.Damage"/> saniyedeki hasar,
    /// <see cref="BuildableStat.SlowFraction"/> yavaşlatma oranı, <see cref="BuildableStat.TickInterval"/> tik aralığı. Yazılmamış
    /// stat 0'dır; tik aralığı 0 olan tuzak çalışmaz (<see cref="IsValid"/>).
    /// </summary>
    public readonly struct SpikeTrapStats
    {
        public SpikeTrapStats(float damagePerSecond, float slowFraction, float tickInterval)
        {
            DamagePerSecond = damagePerSecond < 0f ? 0f : damagePerSecond;
            SlowFraction = slowFraction < 0f ? 0f : slowFraction > 1f ? 1f : slowFraction;
            TickInterval = tickInterval < 0f ? 0f : tickInterval;
        }

        public float DamagePerSecond { get; }

        /// <summary>0..1 (0.4 = %40).</summary>
        public float SlowFraction { get; }

        /// <summary>İki hasar tiki arası (sn).</summary>
        public float TickInterval { get; }

        /// <summary>Tik başına hasar = hasar/sn × tik aralığı (GDD: 15 × 0.5 = 7.5).</summary>
        public float DamagePerTick => DamagePerSecond * TickInterval;

        public bool IsValid => TickInterval > 0f;

        /// <summary>Seviyenin statları; seviye null ise geçersiz (sıfır) değerler.</summary>
        public static SpikeTrapStats From(BuildableLevel level)
        {
            if (level == null)
            {
                return default;
            }

            return new SpikeTrapStats(
                level.GetStat(BuildableStat.Damage, 0f),
                level.GetStat(BuildableStat.SlowFraction, 0f),
                level.GetStat(BuildableStat.TickInterval, 0f));
        }
    }
}
