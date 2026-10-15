namespace LastHaven.Building
{
    /// <summary>
    /// Bir yerleşimin sonucu (GDD §11 önizlemesi: yeşil = <see cref="Valid"/>, gerisi kırmızı). Birden fazla sorun varsa
    /// sıradaki ilki raporlanır: alan → jeneratör → doluluk → çakışma → para.
    /// </summary>
    public enum PlacementStatus
    {
        Valid = 0,

        /// <summary>Ayak izinin bir kısmı üs alanının (14 m) dışında.</summary>
        OutsideArea = 1,

        /// <summary>Ayak izi jeneratörün yasak çevresine giriyor.</summary>
        NearGenerator = 2,

        /// <summary>Hücrelerden biri başka bir yapıyla dolu.</summary>
        Occupied = 3,

        /// <summary>Ayak izinde oyuncu, düşman veya engel var.</summary>
        Blocked = 4,

        /// <summary>Para yetmiyor.</summary>
        NotEnoughMoney = 5,
    }
}
