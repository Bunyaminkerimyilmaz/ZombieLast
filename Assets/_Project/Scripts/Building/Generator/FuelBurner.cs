using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Jeneratörün gece yakıt tüketimi (saf C#; GDD §12: taban 0.4/sn + taret başına 0.05/sn). Tüketim kesirli, cüzdandaki yakıt
    /// tam sayı: yakılan kesir <see cref="Pending"/>'de birikir, her dolan tam birim cüzdandan düşülmek üzere döndürülür.
    /// Böylece <c>FuelChangedEvent</c> her kare değil, birim başına gelir.
    /// </summary>
    public sealed class FuelBurner
    {
        /// <summary>Yakılmış ama henüz cüzdandan düşülmemiş kesir (0..1).</summary>
        public float Pending { get; private set; }

        /// <summary>Gece tüketimi (birim/sn): taban + taret sayısı × taret başına. Negatif girdiler 0 sayılır.</summary>
        public static float Rate(float baseRate, float perTurret, int turretCount)
        {
            return Mathf.Max(0f, baseRate) + Mathf.Max(0f, perTurret) * Mathf.Max(0, turretCount);
        }

        /// <summary>
        /// <paramref name="deltaTime"/> kadar yakar ve cüzdandan düşülecek tam birim sayısını döndürür (en fazla
        /// <paramref name="availableFuel"/>). Yakıt yoksa birikim sıfırlanır: yakıt gelince yeni birim baştan sayılır.
        /// </summary>
        public int Burn(float deltaTime, float rate, int availableFuel)
        {
            if (availableFuel <= 0)
            {
                Pending = 0f;
                return 0;
            }

            if (deltaTime <= 0f || rate <= 0f)
            {
                return 0;
            }

            float burnt = Pending + rate * deltaTime;
            int units = Mathf.FloorToInt(burnt);
            if (units >= availableFuel)
            {
                Pending = 0f;
                return availableFuel;
            }

            Pending = burnt - units;
            return units;
        }

        /// <summary>
        /// Mevcut yakıtın bu tüketimle yeteceği süre (sn): (yakıt − birikmiş kesir) / tüketim. Tüketim 0 ise sonsuz, yakıt yoksa 0.
        /// </summary>
        public float EstimateSeconds(int fuel, float rate)
        {
            if (fuel <= 0)
            {
                return 0f;
            }

            if (rate <= 0f)
            {
                return float.PositiveInfinity;
            }

            return Mathf.Max(0f, fuel - Pending) / rate;
        }

        public void Reset()
        {
            Pending = 0f;
        }
    }
}
