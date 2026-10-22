namespace LastHaven.Core
{
    /// <summary>
    /// Ayarlar değişti (<see cref="ISettingsService.Apply"/>; ayarlar ekranında her değişiklikte, kaydırıcı sürüklenirken de).
    /// Ses düzeyleri, kamera preset'i, sarsıntı ve tam ekran bunu dinleyip hemen uygular.
    /// </summary>
    public readonly struct SettingsChangedEvent : IEvent
    {
        public SettingsChangedEvent(GameSettings settings, GameSettings previous)
        {
            Settings = settings;
            Previous = previous;
        }

        /// <summary>Yeni ayarlar.</summary>
        public GameSettings Settings { get; }

        /// <summary>Değişiklikten önceki ayarlar.</summary>
        public GameSettings Previous { get; }
    }
}
