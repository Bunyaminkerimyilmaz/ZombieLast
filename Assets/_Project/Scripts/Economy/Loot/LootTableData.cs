using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Ganimet sandığı ödül tablosu (GDD §9). Asset'i üretici oluşturur: <c>Data/Loot/LootTable.asset</c>. Yakın (merkeze &lt; 32 m)
    /// ve uzak sandığın seçenekleri ağırlıklıdır; biri seçilir. GDD ağırlık vermediği için varsayılanlar eşit. Saf kural
    /// <see cref="LootTable"/>'da (<see cref="CreateTable"/>).
    /// </summary>
    public sealed class LootTableData : ScriptableObject
    {
        [Header("Uzaklık (GDD §9)")]
        [SerializeField, Min(0f), Tooltip("Sandığın üs merkezine yatay uzaklığı bundan küçükse yakın, değilse (dahil) uzak tablo (m).")]
        private float _farDistance = 32f;

        [Header("Yakın sandık (GDD §9: 30–60 para · 15 yakıt)")]
        [SerializeField, Tooltip("Biri ağırlığına göre seçilir.")]
        private List<LootRewardOption> _nearRewards = new List<LootRewardOption>
        {
            new LootRewardOption(LootRewardType.Money, 30, 60, 1f),
            new LootRewardOption(LootRewardType.Fuel, 15, 15, 1f),
        };

        [Header("Uzak sandık (GDD §9: 80–150 para · 40 yakıt · mermi paketi)")]
        [SerializeField, Tooltip("Biri ağırlığına göre seçilir.")]
        private List<LootRewardOption> _farRewards = new List<LootRewardOption>
        {
            new LootRewardOption(LootRewardType.Money, 80, 150, 1f),
            new LootRewardOption(LootRewardType.Fuel, 40, 40, 1f),
            new LootRewardOption(LootRewardType.AmmoPack, 1, 1, 1f),
        };

        [Header("Mermi paketi (GDD'de yok: marketteki mermi paketleri)")]
        [SerializeField, Tooltip("Bir mermi paketinin silah başına yedek mermisi. Kilitli silahın yedeği de artar (açılınca hazır).")]
        private List<LootAmmoGrant> _ammoPack = new List<LootAmmoGrant>
        {
            new LootAmmoGrant("shotgun", 12),
            new LootAmmoGrant("rifle", 60),
        };

        public float FarDistance => _farDistance;

        public IReadOnlyList<LootRewardOption> NearRewards => _nearRewards;

        public IReadOnlyList<LootRewardOption> FarRewards => _farRewards;

        public IReadOnlyList<LootAmmoGrant> AmmoPack => _ammoPack;

        /// <summary>Saf tablo (o anki değerlerle).</summary>
        public LootTable CreateTable()
        {
            return new LootTable(_farDistance, _nearRewards, _farRewards);
        }
    }
}
