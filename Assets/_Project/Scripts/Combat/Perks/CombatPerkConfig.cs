using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>
    /// Perk bayraklarının (<see cref="LastHaven.Core.PerkFlag"/>) silah tarafındaki değerleri (GDD §13). Bayrağı perk sistemi
    /// (Gün 13) açar; ne kadar etki edeceği burada. Asset: <c>Data/Weapons/CombatPerkConfig.asset</c>; oyuncunun
    /// <c>WeaponLoadoutData</c>'sı bağlıdır.
    /// </summary>
    public sealed class CombatPerkConfig : ScriptableObject
    {
        [Header("Avcı")]
        [SerializeField, Min(0f), Tooltip("Avcı (GDD §13): yavaşlamış hedefe ek hasar oranı. 0.5 = +%50.")]
        private float _hunterDamageBonus = 0.5f;

        public float HunterDamageBonus => _hunterDamageBonus;
    }
}
