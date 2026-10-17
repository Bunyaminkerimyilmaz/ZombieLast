using System.Collections.Generic;
using LastHaven.Core;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>Bir sandığın ödülü: tür, miktar ve hangi tablodan çıktığı.</summary>
    public readonly struct LootRoll
    {
        public LootRoll(LootRewardType type, int amount, bool isFar)
        {
            Type = type;
            Amount = amount;
            IsFar = isFar;
        }

        public LootRewardType Type { get; }

        /// <summary>Para, yakıt birimi veya mermi paketi adedi; 0 = ödül yok (tablo boş ya da ağırlıklar 0).</summary>
        public int Amount { get; }

        /// <summary>Uzak tablodan (merkeze ≥ eşik).</summary>
        public bool IsFar { get; }

        public bool IsEmpty => Amount <= 0;
    }

    /// <summary>
    /// Ganimet tablosu (GDD §9, saf): sandığın üs merkezine uzaklığı eşiğin altındaysa yakın, değilse uzak tablodan ağırlığa göre bir
    /// seçenek ve aralığından bir miktar seçilir. Asset'ten <see cref="LootTableData.CreateTable"/> ile kurulur.
    /// </summary>
    public sealed class LootTable
    {
        private readonly IReadOnlyList<LootRewardOption> _near;
        private readonly IReadOnlyList<LootRewardOption> _far;

        public LootTable(float farDistance, IReadOnlyList<LootRewardOption> near, IReadOnlyList<LootRewardOption> far)
        {
            FarDistance = farDistance;
            _near = near ?? new List<LootRewardOption>();
            _far = far ?? new List<LootRewardOption>();
        }

        /// <summary>Bu uzaklık (m, dahil) ve ötesi uzak sandık.</summary>
        public float FarDistance { get; }

        public bool IsFar(float distance)
        {
            return distance >= FarDistance;
        }

        /// <summary>Uzaklığa göre tabloyu seçip bir ödül çeker. Seçilebilir seçenek yoksa miktarı 0 olan sonuç.</summary>
        public LootRoll Roll(float distance, Random random)
        {
            bool isFar = IsFar(distance);
            LootRewardOption option = Pick(isFar ? _far : _near, random);
            return option != null
                ? new LootRoll(option.Type, option.RollAmount(random), isFar)
                : new LootRoll(LootRewardType.Money, 0, isFar);
        }

        /// <summary>Ağırlığa göre bir seçenek; ağırlıkların toplamı 0 ise (veya liste boşsa) null.</summary>
        public static LootRewardOption Pick(IReadOnlyList<LootRewardOption> options, Random random)
        {
            float total = 0f;
            LootRewardOption last = null;
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] != null && options[i].Weight > 0f)
                {
                    total += options[i].Weight;
                    last = options[i];
                }
            }

            if (last == null)
            {
                return null;
            }

            float roll = (float)(random.NextDouble() * total);
            float cumulative = 0f;
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] == null || options[i].Weight <= 0f)
                {
                    continue;
                }

                cumulative += options[i].Weight;
                if (roll < cumulative)
                {
                    return options[i];
                }
            }

            // Kayan nokta yuvarlaması: zar toplama eşit çıktıysa son seçilebilir seçenek.
            return last;
        }
    }
}
