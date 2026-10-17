namespace LastHaven.Core
{
    /// <summary>
    /// Üssün güç durumu ve jeneratörün yakıt tüketimi (GDD §12). Taretler ve üs ışıkları güç yokken çalışmaz.
    /// Yakıt cüzdandadır (<see cref="IWallet.Fuel"/>); jeneratör gece tüketir, yakıt 0 olunca güç kesilir.
    /// Durum değişince <see cref="PowerChangedEvent"/>, kesilince ayrıca <see cref="PowerLostEvent"/> gönderilir.
    /// </summary>
    public interface IPowerService
    {
        /// <summary>Jeneratör güç veriyor mu (ayakta ve yakıt &gt; 0).</summary>
        bool IsPowered { get; }

        /// <summary>Jeneratör şu an yakıt yakıyor mu (gece ve ayakta).</summary>
        bool IsConsuming { get; }

        /// <summary>Yakıt kapasitesi (birim). HUD yakıt barının maksimumu.</summary>
        int FuelCapacity { get; }

        /// <summary>Gece tüketimi (birim/sn): taban + taret başına. Gündüz de hesaplanır (tahmin için); yakılmaz.</summary>
        float FuelConsumptionRate { get; }

        /// <summary>Mevcut yakıtın <see cref="FuelConsumptionRate"/> ile yeteceği tahmini süre (sn). Tüketim 0 ise sonsuz.</summary>
        float EstimatedFuelSeconds { get; }
    }
}
