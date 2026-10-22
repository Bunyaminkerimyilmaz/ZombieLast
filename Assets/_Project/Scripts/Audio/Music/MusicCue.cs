namespace LastHaven.Audio
{
    /// <summary>Çalan müzik parçası (GDD §18: Gündüz sakin, Gece gerilim, Boss, Zafer, Yenilgi).</summary>
    /// <remarks>Değerler asset'lerde int saklanır: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum MusicCue
    {
        /// <summary>Müzik yok (sessizlik).</summary>
        None = 0,

        Day = 1,

        Night = 2,

        Boss = 3,

        GameOver = 4,

        Victory = 5,
    }
}
