namespace LastHaven.Building
{
    /// <summary>
    /// Periyodik güç kesintisi (saf, GDD §14 Fırtına: her 25 sn'de 5 sn): <see cref="Start"/>'tan sonra her periyodun sonunda
    /// <see cref="Duration"/> sn kesinti; yani periyot 25 / süre 5 → 25–30, 50–55, 75–80. sn kesik. Süre periyottan uzunsa
    /// kesinti sürekli olur. <see cref="Stop"/> kesintiyi hemen bitirir.
    /// </summary>
    public sealed class PowerOutageSchedule
    {
        private float _elapsed;

        public bool IsRunning { get; private set; }

        /// <summary>Şu an kesinti var.</summary>
        public bool IsOut { get; private set; }

        public float Interval { get; private set; }

        public float Duration { get; private set; }

        /// <summary>Takvimi baştan başlatır (ilk kesinti <paramref name="interval"/> sn sonra). Periyot 0 veya süre 0 ise kesinti olmaz.</summary>
        public void Start(float interval, float duration)
        {
            Interval = interval;
            Duration = duration;
            _elapsed = 0f;
            IsRunning = interval > 0f && duration > 0f;
            IsOut = false;
        }

        /// <summary>Takvimi durdurur; kesinti sürüyorsa biter. Durum değiştiyse true.</summary>
        public bool Stop()
        {
            bool wasOut = IsOut;
            IsRunning = false;
            IsOut = false;
            return wasOut;
        }

        /// <summary>Zamanı ilerletir; kesinti başladıysa veya bittiyse true.</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsRunning)
            {
                return false;
            }

            _elapsed += deltaTime;
            bool wasOut = IsOut;
            IsOut = IsOutAt(_elapsed, Interval, Duration);
            return IsOut != wasOut;
        }

        /// <summary>Başlangıçtan <paramref name="elapsed"/> sn sonra kesinti var mı (ilk periyotta yok).</summary>
        public static bool IsOutAt(float elapsed, float interval, float duration)
        {
            if (interval <= 0f || duration <= 0f || elapsed < interval)
            {
                return false;
            }

            float intoCycle = (elapsed - interval) % interval;
            return duration >= interval || intoCycle < duration;
        }
    }
}
