namespace LastHaven.Core
{
    /// <summary>
    /// Üssün güç durumu (GDD §12). Taretler ve üs ışıkları güç yokken çalışmaz.
    /// Durum değişince <see cref="PowerChangedEvent"/> gönderilir.
    /// </summary>
    public interface IPowerService
    {
        /// <summary>Jeneratör güç veriyor mu.</summary>
        bool IsPowered { get; }
    }
}
