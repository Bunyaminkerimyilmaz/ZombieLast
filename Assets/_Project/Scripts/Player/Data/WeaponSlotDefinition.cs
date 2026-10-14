using System;
using LastHaven.Combat;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>Oyuncunun bir silah yuvası: hangi silah ve oyun başında açık mı.</summary>
    [Serializable]
    public struct WeaponSlotDefinition
    {
        [SerializeField, Tooltip("Bu yuvadaki silah. Boşsa yuva kullanılmaz.")]
        private WeaponData _weapon;

        [SerializeField, Tooltip("Oyun başında açık mı. Kapalı yuvalar marketten açılır.")]
        private bool _unlockedAtStart;

        public WeaponSlotDefinition(WeaponData weapon, bool unlockedAtStart)
        {
            _weapon = weapon;
            _unlockedAtStart = unlockedAtStart;
        }

        /// <summary>Null olabilir.</summary>
        public WeaponData Weapon => _weapon;

        public bool UnlockedAtStart => _unlockedAtStart;
    }
}
