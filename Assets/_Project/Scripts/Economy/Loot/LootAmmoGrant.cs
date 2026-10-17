using System;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>Mermi paketinin bir silaha verdiği yedek mermi (GDD §9 "mermi paketi"; içerik GDD'de yok).</summary>
    [Serializable]
    public sealed class LootAmmoGrant
    {
        [SerializeField, Tooltip("Silahın id'si (WeaponData.Id), ör. shotgun. Sıra/büyük harf önemli (ordinal).")]
        private string _weaponId = string.Empty;

        [SerializeField, Min(0), Tooltip("Paket başına eklenen yedek mermi.")]
        private int _amount;

        public LootAmmoGrant()
        {
        }

        public LootAmmoGrant(string weaponId, int amount)
        {
            _weaponId = weaponId;
            _amount = amount;
        }

        public string WeaponId => _weaponId ?? string.Empty;

        public int Amount => Mathf.Max(0, _amount);
    }
}
