using System;
using LastHaven.Core;

namespace LastHaven.Combat
{
    /// <summary>
    /// Bir silahın yükseltme seviyeleri (saf C#; GDD §6). Seviye başına etki ve en yüksek seviye <see cref="WeaponUpgradeConfig"/>'ten.
    /// Ödeme çağıranın işi (market).
    /// </summary>
    public sealed class WeaponUpgrades
    {
        private static readonly int TypeCount = Enum.GetValues(typeof(WeaponUpgradeType)).Length;

        private readonly WeaponUpgradeConfig _config;
        private readonly int[] _levels = new int[TypeCount];

        /// <param name="config">Null ise yükseltme yapılamaz (çarpanlar 1).</param>
        public WeaponUpgrades(WeaponUpgradeConfig config)
        {
            _config = config;
        }

        public int GetLevel(WeaponUpgradeType type)
        {
            return _levels[(int)type];
        }

        /// <summary>Bu yükseltmenin en yüksek seviyesi; yükseltme verisi yoksa 0.</summary>
        public int GetMaxLevel(WeaponUpgradeType type)
        {
            return _config != null ? _config.GetMaxLevel(type) : 0;
        }

        /// <summary>Mevcut seviyenin çarpanı (1 + seviye × artış); yükseltme verisi yoksa 1.</summary>
        public float GetMultiplier(WeaponUpgradeType type)
        {
            return _config != null ? _config.GetMultiplier(type, GetLevel(type)) : 1f;
        }

        /// <summary>Bir seviye artırır; en yüksek seviyedeyse false.</summary>
        public bool TryUpgrade(WeaponUpgradeType type)
        {
            if (GetLevel(type) >= GetMaxLevel(type))
            {
                return false;
            }

            _levels[(int)type]++;
            return true;
        }
    }
}
