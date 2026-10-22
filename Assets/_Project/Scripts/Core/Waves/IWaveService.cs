using UnityEngine;

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
        /// Sürmekte olan gecenin henüz doğmamış düşmanlarını (zamanı gelmemiş + tavan yüzünden kuyrukta bekleyen) iptal eder; gece,
        /// kalan canlılar ölünce normal yoldan biter (<see cref="NightCompletedEvent"/>). İptal edilen sayıyı döndürür; gece yoksa 0.
        /// Debug "geceyi atla" için.
        /// </summary>
        int CancelPendingSpawns();

        /// <summary>
        /// Sürmekte olan geceyi bitmiş saymadan durdurur: <see cref="NightCompletedEvent"/> gönderilmez, gece olayı biter, sahnedeki
        /// düşmanlar artık sayılmaz (yerlerinde kalırlar; ölümleri geceyi etkilemez). Gece yoksa false. Debug "N. geceye git" için.
        /// </summary>
        bool AbortNight();

        /// <summary>
        /// Gecenin istihbarat bilgisi: saldırı yönleri, düşman tipleri ve adetleri, olay adı. Gündüz HUD'u gösterir.
        /// Geçersiz indekste <see cref="System.ArgumentOutOfRangeException"/> fırlatır.
        /// </summary>
        NightInfo GetTonightInfo(int nightIndex);

        /// <summary>
        /// Yönün spawn bölgesinin merkezi (dünya, yer seviyesi): istihbarat okları bu noktayı gösterir. Bölge yoksa false.
        /// </summary>
        bool TryGetSpawnZoneCenter(CompassDirection direction, out Vector3 center);
    }
}
