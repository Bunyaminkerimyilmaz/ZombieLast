using System;

namespace LastHaven.Economy
{
    /// <summary>Bir öldürmenin toplam jeton değeri (GDD §10, saf).</summary>
    public static class CoinValue
    {
        /// <summary>
        /// Toplam = taban ödül × kombo çarpanı × jeton değeri çarpanı (<c>IRunModifiers</c> CoinValue), en yakın tam sayıya
        /// yuvarlı (.5 yukarı: 10 × 1.25 = 13). Çarpanlar decimal'e çevrilerek çarpılır; 1.15f gibi değerlerde float hatası
        /// yuvarlamayı kaydırmaz. Ödül 0/negatif ya da çarpan 0/negatif/NaN ise 0; int sınırında doyar.
        /// </summary>
        public static int Calculate(int moneyReward, float comboMultiplier, float valueMultiplier)
        {
            if (moneyReward <= 0 || !(comboMultiplier > 0f) || !(valueMultiplier > 0f))
            {
                return 0;
            }

            // Önce double ile taşma kontrolü: decimal dönüşümü çok büyük değerde hata fırlatır.
            if ((double)moneyReward * comboMultiplier * valueMultiplier >= int.MaxValue)
            {
                return int.MaxValue;
            }

            decimal total = Math.Round(
                moneyReward * (decimal)comboMultiplier * (decimal)valueMultiplier, MidpointRounding.AwayFromZero);
            return total >= int.MaxValue ? int.MaxValue : (int)total;
        }
    }
}
