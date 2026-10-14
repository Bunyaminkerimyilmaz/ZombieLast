using System;
using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>
    /// Silah yükseltmelerinin seviye başına etkisi ve en yüksek seviyesi (GDD §6). Tüm silahlar aynı değerleri kullanır;
    /// seviyeler silah başına <see cref="WeaponRuntime"/>'da tutulur. Asset: <c>Data/Weapons/WeaponUpgradeConfig.asset</c>.
    /// </summary>
    /// <remarks>Seviye artışları toplanır: hasar 5. seviyede +%50 (×1.5). Fiyatlar market verisindedir.</remarks>
    public sealed class WeaponUpgradeConfig : ScriptableObject
    {
        [Header("Hasar")]
        [SerializeField, Min(0f), Tooltip("Seviye başına hasar artışı (oran): 0.1 = +%10.")]
        private float _damagePerLevel = 0.1f;

        [SerializeField, Min(0), Tooltip("En yüksek hasar seviyesi.")]
        private int _damageMaxLevel = 5;

        [Header("Atış hızı")]
        [SerializeField, Min(0f), Tooltip("Seviye başına atış hızı artışı (oran): 0.08 = +%8.")]
        private float _fireRatePerLevel = 0.08f;

        [SerializeField, Min(0), Tooltip("En yüksek atış hızı seviyesi.")]
        private int _fireRateMaxLevel = 3;

        [Header("Şarjör")]
        [SerializeField, Min(0f), Tooltip("Seviye başına şarjör kapasitesi artışı (oran): 0.25 = +%25. Sonuç en yakın tam sayıya yuvarlanır.")]
        private float _magSizePerLevel = 0.25f;

        [SerializeField, Min(0), Tooltip("En yüksek şarjör seviyesi.")]
        private int _magSizeMaxLevel = 3;

        /// <summary>Seviye başına artış (oran).</summary>
        public float GetIncreasePerLevel(WeaponUpgradeType type)
        {
            return type switch
            {
                WeaponUpgradeType.Damage => _damagePerLevel,
                WeaponUpgradeType.FireRate => _fireRatePerLevel,
                WeaponUpgradeType.MagSize => _magSizePerLevel,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        public int GetMaxLevel(WeaponUpgradeType type)
        {
            return type switch
            {
                WeaponUpgradeType.Damage => _damageMaxLevel,
                WeaponUpgradeType.FireRate => _fireRateMaxLevel,
                WeaponUpgradeType.MagSize => _magSizeMaxLevel,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        /// <summary>Seviyenin çarpanı: 1 + seviye × artış. Seviye 0..max aralığına kırpılır.</summary>
        public float GetMultiplier(WeaponUpgradeType type, int level)
        {
            int clamped = Math.Max(0, Math.Min(level, GetMaxLevel(type)));
            return 1f + clamped * GetIncreasePerLevel(type);
        }
    }
}
