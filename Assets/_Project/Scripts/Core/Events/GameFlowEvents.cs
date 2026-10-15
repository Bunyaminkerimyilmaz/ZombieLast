namespace LastHaven.Core
{
    /// <summary>Oyun evresi değiştiğinde gönderilir (<see cref="IGameStateService"/>). Işık, HUD, müzik ve market dinler.</summary>
    public readonly struct PhaseChangedEvent : IEvent
    {
        public PhaseChangedEvent(GamePhase phase, int nightIndex, float duration)
        {
            Phase = phase;
            NightIndex = nightIndex;
            Duration = duration;
        }

        /// <summary>Yeni evre.</summary>
        public GamePhase Phase { get; }

        /// <summary>0 tabanlı gece indeksi (<see cref="IGameStateService.NightIndex"/> ile aynı anlam).</summary>
        public int NightIndex { get; }

        /// <summary>Evrenin süresi (sn). Süresiz evrelerde (Night, Dawn, GameOver, Victory) 0.</summary>
        public float Duration { get; }
    }

    /// <summary>
    /// Gündüz sayacı: kalan tam saniye (yukarı yuvarlı) değiştiğinde gönderilir. Gündüz başında bir kez (ör. 120), sonra
    /// saniyede bir; son değer 1'dir, süre bitince <see cref="PhaseChangedEvent"/> (Dusk) gelir. HUD zamanlayıcısı dinler.
    /// </summary>
    public readonly struct DayTimerTickEvent : IEvent
    {
        public DayTimerTickEvent(int remainingSeconds)
        {
            RemainingSeconds = remainingSeconds;
        }

        public int RemainingSeconds { get; }
    }

    /// <summary>
    /// Gündüzün kalan süresi son uyarı süresine (GDD §9: 15 sn) indiğinde, gündüz başına bir kez gönderilir.
    /// Gündüz "Hazırım" ile daha önce biterse gönderilmez. HUD büyük geri sayım ve kırmızı nabız gösterir.
    /// </summary>
    public readonly struct DayEndingSoonEvent : IEvent
    {
        public DayEndingSoonEvent(float remainingSeconds)
        {
            RemainingSeconds = remainingSeconds;
        }

        public float RemainingSeconds { get; }
    }

    /// <summary>
    /// Gündüz "Hazırım" ile erken bittiğinde gönderilir, alacakaranlığa geçişten önce. Bonus GameFlow'da hesaplanır
    /// (GDD §9: kalan her 10 sn için +5 para; HUD'daki tam saniyeyle); cüzdan <see cref="BonusMoney"/>'yi ekler.
    /// </summary>
    public readonly struct EarlyReadyEvent : IEvent
    {
        public EarlyReadyEvent(float remainingSeconds, int bonusMoney)
        {
            RemainingSeconds = remainingSeconds;
            BonusMoney = bonusMoney;
        }

        /// <summary>Basıldığı anda gündüzden kalan süre (sn).</summary>
        public float RemainingSeconds { get; }

        /// <summary>Kazanılan erken hazır bonusu (para); 10 sn'den az kaldıysa 0.</summary>
        public int BonusMoney { get; }
    }

    /// <summary>
    /// Gece atlatılıp şafağa geçildiğinde gönderilir (<see cref="PhaseChangedEvent"/>'ten sonra). Son gecede gönderilmez
    /// (<see cref="VictoryEvent"/> gelir). Faiz ve şafak ekranı dinler.
    /// </summary>
    public readonly struct DawnEvent : IEvent
    {
        public DawnEvent(int nightIndex)
        {
            NightIndex = nightIndex;
        }

        /// <summary>Biten gecenin 0 tabanlı indeksi.</summary>
        public int NightIndex { get; }
    }

    /// <summary>Şafak ekranından devam isteği: oyun akışı sıradaki gündüze geçer. Şafak panelinin "Devam" butonu ve Enter (GameFlow) gönderir.</summary>
    public readonly struct DawnContinueRequestedEvent : IEvent
    {
    }

    /// <summary>Jeneratör yıkılıp oyun bittiğinde bir kez gönderilir (GDD §15), <see cref="PhaseChangedEvent"/>'ten sonra.</summary>
    public readonly struct GameOverEvent : IEvent
    {
        public GameOverEvent(int nightIndex)
        {
            NightIndex = nightIndex;
        }

        /// <summary>Ulaşılan gecenin 0 tabanlı indeksi.</summary>
        public int NightIndex { get; }
    }

    /// <summary>Son gece atlatılınca bir kez gönderilir (GDD §15: tahliye), <see cref="PhaseChangedEvent"/>'ten sonra.</summary>
    public readonly struct VictoryEvent : IEvent
    {
        public VictoryEvent(int nightIndex)
        {
            NightIndex = nightIndex;
        }

        /// <summary>Son gecenin 0 tabanlı indeksi.</summary>
        public int NightIndex { get; }
    }
}
