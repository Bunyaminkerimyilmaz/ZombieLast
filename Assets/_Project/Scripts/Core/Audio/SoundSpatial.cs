namespace LastHaven.Core
{
    /// <summary>Bir sesin uzayda nasıl çalındığı (<see cref="SoundData"/>).</summary>
    /// <remarks>Değerler asset'lerde int saklanır: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum SoundSpatial
    {
        /// <summary>Konumsuz: her yerden aynı duyulur (arayüz, duyurular, müzik).</summary>
        TwoD = 0,

        /// <summary>Konumlu: dinleyiciye uzaklıkla (min/max mesafe) zayıflar ve sağ/sol kanala dağılır.</summary>
        ThreeD = 1,
    }
}
