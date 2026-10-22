namespace LastHaven.Core
{
    /// <summary>
    /// Oyunun duraklatılması (<c>PauseService</c>, [Systems]/Pause, Awake'te kaydolur). Birden fazla kaynak aynı anda duraklatabilir
    /// (seviye kartları, ileride duraklatma menüsü): oyun ilk kaynak duraklatınca durur, son kaynak devam ettirince sürer. Duraklatma
    /// <c>Time.timeScale</c>'i 0 yapar; Update'ler çalışmaya devam eder, ölçekli zaman (fizik, ajanlar, sayaçlar, animasyonlar) durur.
    /// Duraklatmayı yapan kod gerçek zamanla (<c>Time.unscaledDeltaTime</c>) ilerlemeli.
    /// </summary>
    public interface IPauseService
    {
        /// <summary>En az bir kaynak duraklatmış mı.</summary>
        bool IsPaused { get; }

        /// <summary>
        /// <paramref name="source"/> adına duraklatır. Aynı kaynağın ikinci çağrısı etkisizdir (kaynaklar sayılmaz, kümedir).
        /// Kaynak bir Unity nesnesiyse ve yok edilirse duraklatması kendiliğinden kalkar.
        /// </summary>
        void Pause(object source);

        /// <summary><paramref name="source"/>'un duraklatmasını kaldırır; başka kaynak yoksa oyun devam eder. Duraklatmamış kaynak etkisiz.</summary>
        void Resume(object source);

        /// <summary><paramref name="source"/> şu an duraklatıyor mu.</summary>
        bool IsPausedBy(object source);
    }
}
