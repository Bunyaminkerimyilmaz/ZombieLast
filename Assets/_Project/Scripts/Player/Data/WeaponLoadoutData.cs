using System;
using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncunun silah yuvaları ve mermi ayarları (GDD §6). Asset'i üretici oluşturur: <c>Data/Player/WeaponLoadout.asset</c>.
    /// </summary>
    public sealed class WeaponLoadoutData : ScriptableObject
    {
        [Header("Yuvalar")]
        [SerializeField, Tooltip("Sırayla 1, 2, 3 tuşlarının yuvaları. Varsayılan: Tabanca (açık), Pompalı, Tüfek.")]
        private WeaponSlotDefinition[] _slots = Array.Empty<WeaponSlotDefinition>();

        [SerializeField, Tooltip("Silah yükseltmelerinin seviye başına etkileri (GDD §6). Boşsa yükseltme yapılamaz.")]
        private WeaponUpgradeConfig _upgrades;

        [SerializeField, Tooltip("Perk'lerin silah etkileri (GDD §13: Avcı). Boşsa bu etkiler uygulanmaz.")]
        private CombatPerkConfig _perks;

        [Header("İsabet")]
        [SerializeField, Tooltip("Oyuncu mermilerinin çarptığı layer'lar. Yapılar ve oyuncu dahil değil: mermi kendi barikatının üstünden geçer.")]
        private LayerMask _hitMask = GameLayers.GroundMask | GameLayers.ObstacleMask | GameLayers.EnemyMask;

        public IReadOnlyList<WeaponSlotDefinition> Slots => _slots;

        /// <summary>Null olabilir.</summary>
        public WeaponUpgradeConfig Upgrades => _upgrades;

        /// <summary>Null olabilir.</summary>
        public CombatPerkConfig Perks => _perks;

        public LayerMask HitMask => _hitMask;
    }
}
