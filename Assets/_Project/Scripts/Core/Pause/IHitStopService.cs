namespace LastHaven.Core
{
    /// <summary>
    /// Vuruş donması (hit-stop; <c>PauseService</c>, [Systems]/Pause, Awake'te kaydolur): ağır bir öldürmede oyunu gerçek zamanla
    /// çok kısa (ör. 0.06 sn) dondurur. Zaman ölçeğinin tek sahibi duraklatma servisi olduğu için <see cref="IPauseService"/> ile
    /// çakışmaz: duraklıyken istek yok sayılır, donma sürerken duraklatma gelirse donma iptal olur ve duraklatma kalkınca eski ölçek
    /// geri gelir. Donma <see cref="IPauseService.IsPaused"/>'i değiştirmez (menüler donmayı duraklatma saymaz) ve log basmaz.
    /// </summary>
    public interface IHitStopService
    {
        /// <summary>Şu an donma sürüyor mu.</summary>
        bool IsHitStopped { get; }

        /// <summary>
        /// Oyunu <paramref name="duration"/> sn (gerçek zaman) dondurur. Donma sürerken gelen istek kalan süreyi sadece uzatır
        /// (kalan ile yeninin büyüğü). Süre ≤ 0 veya oyun duraklıysa etkisiz.
        /// </summary>
        void HitStop(float duration);
    }
}
