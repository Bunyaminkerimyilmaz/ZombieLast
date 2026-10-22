using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>
    /// Perk bayraklarının (<see cref="PerkFlag"/>) silah tarafındaki değerleri (GDD §13). Bayrağı perk sistemi açar; ne kadar etki
    /// edeceği burada. Asset: <c>Data/Weapons/CombatPerkConfig.asset</c>; oyuncunun <c>WeaponLoadoutData</c>'sı bağlıdır (sadece
    /// oyuncu silahları; taretlerde perk kancası yok).
    /// </summary>
    public sealed class CombatPerkConfig : ScriptableObject
    {
        [Header("Avcı")]
        [SerializeField, Min(0f), Tooltip("Avcı (GDD §13): yavaşlamış hedefe ek hasar oranı. 0.5 = +%50.")]
        private float _hunterDamageBonus = 0.5f;

        [Header("Seken Mermi")]
        [SerializeField, Min(0f), Tooltip("Seken Mermi (GDD §13): sekmenin hasarı, saçmanın hasarına oranla. 0.5 = %50.")]
        private float _ricochetDamageFraction = 0.5f;

        [SerializeField, Min(0f), Tooltip("Sekilecek düşmanın arandığı yarıçap (m), ilk isabet noktasından. GDD'de yok.")]
        private float _ricochetRadius = 8f;

        [SerializeField, Tooltip("Sekilebilecek hedeflerin layer'ları (düşmanlar).")]
        private LayerMask _ricochetTargetLayers = GameLayers.EnemyMask;

        [SerializeField, Min(1), Tooltip("Sekme aramasında okunan en fazla collider. Kalabalıkta fazlası görülmez (en yakın atlanabilir).")]
        private int _ricochetBufferSize = 32;

        public float HunterDamageBonus => _hunterDamageBonus;

        public float RicochetDamageFraction => _ricochetDamageFraction;

        public float RicochetRadius => _ricochetRadius;

        public LayerMask RicochetTargetLayers => _ricochetTargetLayers;

        public int RicochetBufferSize => Mathf.Max(1, _ricochetBufferSize);
    }
}
