using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Economy;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ganimet sandığının beklenen ödülü (saf): seçeneklerin ağırlıklı ortalaması, miktar aralığın ortası. Sandıkların uzak sayılan
    /// payı halkanın alanından hesaplanır (yerleşim alana göre eşit dağılır, <c>LootRing</c>).
    /// </summary>
    public static class LootExpectation
    {
        /// <summary>Bir sandığın beklenen miktarı (<paramref name="type"/> türünden); ağırlık toplamı 0 ise 0.</summary>
        public static double Expected(IReadOnlyList<LootRewardOption> options, LootRewardType type)
        {
            if (options == null)
            {
                return 0d;
            }

            double totalWeight = 0d;
            double weighted = 0d;
            for (int i = 0; i < options.Count; i++)
            {
                LootRewardOption option = options[i];
                if (option == null)
                {
                    continue;
                }

                totalWeight += option.Weight;
                if (option.Type == type)
                {
                    weighted += option.Weight * (option.MinAmount + option.MaxAmount) * 0.5d;
                }
            }

            return totalWeight > 0d ? weighted / totalWeight : 0d;
        }

        /// <summary>
        /// [<paramref name="minRadius"/>, <paramref name="maxRadius"/>] halkasında <paramref name="farDistance"/> ve ötesine düşen alan payı
        /// (0..1). Eşik halkanın içindeyse 1, dışındaysa 0.
        /// </summary>
        public static double FarFraction(float minRadius, float maxRadius, float farDistance)
        {
            double inner = Math.Max(0d, minRadius);
            double outer = Math.Max(inner, maxRadius);
            double ringArea = outer * outer - inner * inner;
            if (ringArea <= 0d)
            {
                return farDistance <= inner ? 1d : 0d;
            }

            double threshold = Math.Min(outer, Math.Max(inner, farDistance));
            return (outer * outer - threshold * threshold) / ringArea;
        }
    }
}
