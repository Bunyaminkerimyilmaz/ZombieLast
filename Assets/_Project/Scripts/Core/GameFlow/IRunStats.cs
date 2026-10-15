namespace LastHaven.Core
{
    /// <summary>
    /// Run istatistikleri (GameFlow'daki <c>StatsTracker</c>; kaydı [Systems]/GameFlow'daki <c>StatsTrackerHost</c> yapar).
    /// Şafak, Oyun Bitti ve Zafer ekranları buradan okur (GDD §15, §16). "Gece" değerleri gece evresine girişte sıfırlanır;
    /// şafakta biten gecenin değeri okunur. Oyun bitince (yenilgi/zafer) tüm sayaçlar donar.
    /// </summary>
    public interface IRunStats
    {
        /// <summary>Son (veya süren) gecede öldürülen düşman sayısı.</summary>
        int NightKills { get; }

        /// <summary>Run boyunca öldürülen tüm düşmanlar (gündüz dahil).</summary>
        int TotalKills { get; }

        /// <summary>Son (veya süren) gecede toplanan jetonların değeri (para). Faiz ve bonus dahil değil.</summary>
        int NightMoneyEarned { get; }

        /// <summary>Run boyunca toplanan jetonların değeri (para).</summary>
        int TotalMoneyEarned { get; }

        /// <summary>Son (veya süren) gecenin en yüksek kombosu (ardışık öldürme).</summary>
        int NightBestCombo { get; }

        /// <summary>Run boyunca en yüksek kombo.</summary>
        int BestCombo { get; }

        /// <summary>Son şafakta ödenen faiz; gece başında 0'lanır.</summary>
        int LastInterest { get; }

        /// <summary>Run boyunca ödenen toplam faiz.</summary>
        int TotalInterest { get; }
    }
}
