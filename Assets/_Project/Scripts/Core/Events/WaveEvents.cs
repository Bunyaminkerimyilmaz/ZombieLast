namespace LastHaven.Core
{
    /// <summary>Bir gece dalgası başladığında gönderilir (<see cref="IWaveService.StartNight"/>).</summary>
    public readonly struct NightStartedEvent : IEvent
    {
        public NightStartedEvent(int nightIndex, int nightNumber, int totalEnemies)
        {
            NightIndex = nightIndex;
            NightNumber = nightNumber;
            TotalEnemies = totalEnemies;
        }

        /// <summary>0 tabanlı gece indeksi.</summary>
        public int NightIndex { get; }

        /// <summary>Arayüzde görünen gece numarası (1 tabanlı).</summary>
        public int NightNumber { get; }

        /// <summary>Bu gece doğacak toplam düşman (boss dahil).</summary>
        public int TotalEnemies { get; }
    }

    /// <summary>
    /// Gecenin kalan düşman sayısı değiştiğinde gönderilir: gece başında bir kez, sonra her dalga düşmanı ölünce
    /// (ve doğurulamayan düşman düşülünce). HUD "kalan düşman" için dinler.
    /// </summary>
    public readonly struct WaveProgressEvent : IEvent
    {
        public WaveProgressEvent(int nightIndex, int remainingEnemies, int totalEnemies)
        {
            NightIndex = nightIndex;
            RemainingEnemies = remainingEnemies;
            TotalEnemies = totalEnemies;
        }

        public int NightIndex { get; }

        /// <summary>Henüz doğmamış + kuyrukta bekleyen + canlı dalga düşmanları.</summary>
        public int RemainingEnemies { get; }

        public int TotalEnemies { get; }
    }

    /// <summary>
    /// Gece bittiğinde gönderilir: tüm gruplar doğmuş ve canlı dalga düşmanı kalmamış (GDD §8). Oyun akışı şafağa geçer.
    /// </summary>
    public readonly struct NightCompletedEvent : IEvent
    {
        public NightCompletedEvent(int nightIndex, int nightNumber)
        {
            NightIndex = nightIndex;
            NightNumber = nightNumber;
        }

        public int NightIndex { get; }

        public int NightNumber { get; }
    }
}
