namespace LastHaven.Core
{
    /// <summary>
    /// Gece dalgaları servisi (Waves'teki <c>WaveSpawner</c>, [Systems]/WaveSpawner). Gece indeksleri 0 tabanlıdır:
    /// 0 = 1. gece. Oyun akışı geceyi başlatır, HUD ve istihbarat bilgileri buradan okur (GDD §8, §9).
    /// </summary>
    public interface IWaveService
    {
        /// <summary>Config'teki toplam gece sayısı (GDD: 10).</summary>
        int TotalNights { get; }

        /// <summary>Bir gece sürüyorsa true: spawn'lar bitmemiş veya canlı düşman var.</summary>
        bool IsNightRunning { get; }

        /// <summary>Bu gecenin dalgasından doğmuş ve hâlâ canlı düşman sayısı.</summary>
        int AliveCount { get; }

        /// <summary>
        /// <paramref name="nightIndex"/>. geceyi başlatır: <see cref="NightStartedEvent"/> gönderir, gruplar zamanla doğar.
        /// Gece zaten sürüyorsa, indeks geçersizse veya gece verisi yoksa loglar ve false döner.
        /// </summary>
        bool StartNight(int nightIndex);

        /// <summary>
        /// Gecenin istihbarat bilgisi: saldırı yönleri, düşman tipleri ve adetleri, olay adı. Gündüz HUD'u gösterir.
        /// Geçersiz indekste <see cref="System.ArgumentOutOfRangeException"/> fırlatır.
        /// </summary>
        NightInfo GetTonightInfo(int nightIndex);
    }
}
