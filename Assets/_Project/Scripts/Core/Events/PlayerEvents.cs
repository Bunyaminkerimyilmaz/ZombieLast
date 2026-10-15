using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncu hasar aldığında gönderilir (dokunulmazlıkla engellenen hasar gönderilmez). HUD, ekran flaşı ve kombo dinler.
    /// Aynı vuruşun genel <see cref="DamagedEvent"/>'i de gelir.
    /// </summary>
    public readonly struct PlayerDamagedEvent : IEvent
    {
        public PlayerDamagedEvent(GameObject player, DamageInfo damage, float appliedAmount, float currentHealth, float maxHealth)
        {
            Player = player;
            Damage = damage;
            AppliedAmount = appliedAmount;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }

        public GameObject Player { get; }

        public DamageInfo Damage { get; }

        /// <summary>Gerçekte düşen can (aşırı hasar kırpılmış).</summary>
        public float AppliedAmount { get; }

        public float CurrentHealth { get; }

        public float MaxHealth { get; }
    }

    /// <summary>Oyuncu öldüğünde gönderilir (<see cref="PlayerDamagedEvent"/>'ten sonra). Yeniden doğma ve kombo sıfırlama dinler.</summary>
    public readonly struct PlayerDiedEvent : IEvent
    {
        public PlayerDiedEvent(GameObject player, Vector3 position, DamageInfo killingBlow)
        {
            Player = player;
            Position = position;
            KillingBlow = killingBlow;
        }

        public GameObject Player { get; }

        /// <summary>Öldüğü yer (ayak, dünya).</summary>
        public Vector3 Position { get; }

        public DamageInfo KillingBlow { get; }
    }

    /// <summary>Oyuncu ölümden sonra üste tam canla yeniden doğduğunda gönderilir. HUD can barını yeniler.</summary>
    public readonly struct PlayerRespawnedEvent : IEvent
    {
        public PlayerRespawnedEvent(GameObject player, Vector3 position, float currentHealth, float maxHealth)
        {
            Player = player;
            Position = position;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }

        public GameObject Player { get; }

        /// <summary>Doğduğu yer (ayak, dünya).</summary>
        public Vector3 Position { get; }

        public float CurrentHealth { get; }

        public float MaxHealth { get; }
    }
}
