using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Bir <see cref="Health"/> hasar aldığında gönderilir.</summary>
    public readonly struct DamagedEvent : IEvent
    {
        public GameObject Target { get; }

        public DamageInfo Damage { get; }

        /// <summary>Gerçekte düşen can (aşırı hasar kırpılmış).</summary>
        public float AppliedAmount { get; }

        public float CurrentHealth { get; }

        public float MaxHealth { get; }

        public DamagedEvent(GameObject target, DamageInfo damage, float appliedAmount, float currentHealth, float maxHealth)
        {
            Target = target;
            Damage = damage;
            AppliedAmount = appliedAmount;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    /// <summary>Bir <see cref="Health"/> öldüğünde gönderilir. <see cref="DamagedEvent"/>'ten sonra gelir.</summary>
    public readonly struct DiedEvent : IEvent
    {
        public GameObject Target { get; }

        /// <summary>Öldüren hasar.</summary>
        public DamageInfo KillingBlow { get; }

        public DiedEvent(GameObject target, DamageInfo killingBlow)
        {
            Target = target;
            KillingBlow = killingBlow;
        }
    }
}
