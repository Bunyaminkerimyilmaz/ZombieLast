using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Perk bayraklarının (<see cref="LastHaven.Core.PerkFlag"/>) yapı tarafındaki değerleri (GDD §13). Bayrağı perk sistemi açar;
    /// ne kadar etki edeceği burada. Asset: <c>Data/Building/BuildingPerkConfig.asset</c>; <see cref="TurretConfig"/> (Şok Taret) ve
    /// [Systems]/BuildingPerks'teki <see cref="BarricadeThorns"/> (Dikenli Duvar) bağlıdır. Sağlam Duvar ve Taret Ustası stat
    /// perk'leridir, değerleri perk verisinde.
    /// </summary>
    public sealed class BuildingPerkConfig : ScriptableObject
    {
        [Header("Dikenli Duvar")]
        [SerializeField, Min(0f), Tooltip("Dikenli Duvar (GDD §13): barikatın aldığı yakın dövüş hasarının saldırana yansıyan oranı. 0.3 = %30.")]
        private float _thornsReflectFraction = 0.3f;

        [Header("Şok Taret")]
        [SerializeField, Range(0f, 1f), Tooltip("Şok Taret (GDD §13): taret vuruşunun yavaşlatma oranı. 0.25 = %25.")]
        private float _shockSlowFraction = 0.25f;

        [SerializeField, Min(0f), Tooltip("Şok Taret yavaşlatmasının süresi (sn).")]
        private float _shockSlowDuration = 1f;

        public float ThornsReflectFraction => _thornsReflectFraction;

        public float ShockSlowFraction => _shockSlowFraction;

        public float ShockSlowDuration => _shockSlowDuration;
    }
}
