namespace LastHaven.Core
{
    /// <summary>
    /// Oyun akışının evresi (GDD §3, §15): Boot → Day → Dusk → Night → Dawn → Day ... Son gece biterse Victory,
    /// jeneratör yıkılırsa GameOver; ikisi de son durumdur.
    /// </summary>
    /// <remarks>Değerler açıkça numaralı (asset'lerde int saklanabilir): sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum GamePhase
    {
        /// <summary>Sahne yüklendi, akış henüz başlamadı.</summary>
        Boot = 0,

        /// <summary>Gündüz: market, inşa, sandık, istihbarat. Süreli; "Hazırım" ile erken biter.</summary>
        Day = 1,

        /// <summary>Alacakaranlık: gündüzden geceye kısa geçiş. Süreli.</summary>
        Dusk = 2,

        /// <summary>Gece: dalgalar. Gece verisindeki tüm düşmanlar doğup ölünce biter.</summary>
        Night = 3,

        /// <summary>Şafak: gecenin sonucu. Devam isteğiyle (<see cref="DawnContinueRequestedEvent"/>) gündüze geçer.</summary>
        Dawn = 4,

        /// <summary>Yenilgi: jeneratör yıkıldı.</summary>
        GameOver = 5,

        /// <summary>Zafer: son gece atlatıldı (tahliye).</summary>
        Victory = 6,
    }
}
