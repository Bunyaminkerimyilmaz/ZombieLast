using System;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>Bir silahın atış sarsıntısı (<see cref="FeedbackConfig"/> listesi; silah id ile eşleşir).</summary>
    [Serializable]
    public struct WeaponShake
    {
        [SerializeField, Tooltip("Silah id'si (WeaponData.Id).")]
        private string _weaponId;

        [SerializeField, Min(0f), Tooltip("Sarsıntı şiddeti (0..1).")]
        private float _strength;

        public WeaponShake(string weaponId, float strength)
        {
            _weaponId = weaponId;
            _strength = strength;
        }

        public string WeaponId => _weaponId;

        public float Strength => _strength;
    }
}
