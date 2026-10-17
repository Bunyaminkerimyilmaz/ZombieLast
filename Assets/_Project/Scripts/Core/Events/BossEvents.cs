using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Boss (GDD §7: Brute) doğduğunda gönderilir (yerden çıkmaya başladığı an). HUD boss barını açar.</summary>
    public readonly struct BossSpawnedEvent : IEvent
    {
        public BossSpawnedEvent(GameObject boss, string enemyId, string displayName, float maxHealth)
        {
            Boss = boss;
            EnemyId = enemyId;
            DisplayName = displayName;
            MaxHealth = maxHealth;
        }

        /// <summary>Boss'un kök objesi. Can ve yenilgi olayları bununla eşlenir.</summary>
        public GameObject Boss { get; }

        public string EnemyId { get; }

        /// <summary>Arayüzde görünen ad (EnemyData.DisplayName, Türkçe).</summary>
        public string DisplayName { get; }

        public float MaxHealth { get; }
    }

    /// <summary>Boss'un canı değiştiğinde (hasar, iyileşme) gönderilir.</summary>
    public readonly struct BossHealthChangedEvent : IEvent
    {
        public BossHealthChangedEvent(GameObject boss, float currentHealth, float maxHealth)
        {
            Boss = boss;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }

        public GameObject Boss { get; }

        public float CurrentHealth { get; }

        public float MaxHealth { get; }
    }

    /// <summary>Boss öldüğünde gönderilir (ölüm anında; <see cref="EnemyKilledEvent"/> da ayrıca gelir). HUD boss barını kapatır.</summary>
    public readonly struct BossDefeatedEvent : IEvent
    {
        public BossDefeatedEvent(GameObject boss, string enemyId, bool killedByPlayer)
        {
            Boss = boss;
            EnemyId = enemyId;
            KilledByPlayer = killedByPlayer;
        }

        public GameObject Boss { get; }

        public string EnemyId { get; }

        /// <summary>Öldüren hasarın kaynağı oyuncuysa true.</summary>
        public bool KilledByPlayer { get; }
    }
}
