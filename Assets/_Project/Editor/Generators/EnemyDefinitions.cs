using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Üretilen düşman tipleri: GDD §7 değerleri, §18 görünüm (renk + gövde ölçeği) ve havuz ön ısıtma sayısı.
    /// Değerler asset'lere sadece ilk oluşturmada yazılır. Spitter, Exploder ve Brute Gün 12'de eklenecek.
    /// </summary>
    public static class EnemyDefinitions
    {
        // GDD §18: Yürüyen kapsül; Koşucu ince kapsül (×0.8, sadece genişlik); Tank büyük kapsül (×1.6).
        // Ön ısıtma GDD'de yok: Yürüyen F4'te 20'şer doğar, gecelerde en kalabalık tip.
        public static readonly EnemyDefinition Walker = new EnemyDefinition(
            "Walker", "walker", "Yürüyen", PlaceholderMaterialId.Walker, Vector3.one, prewarm: 40,
            maxHealth: 60f, moveSpeed: 2.2f, damage: 10f, attackRate: 1f, attackRange: 1.5f, moneyReward: 10, xpReward: 1);

        public static readonly EnemyDefinition Runner = new EnemyDefinition(
            "Runner", "runner", "Koşucu", PlaceholderMaterialId.Runner, new Vector3(0.8f, 1f, 0.8f), prewarm: 15,
            maxHealth: 35f, moveSpeed: 5f, damage: 6f, attackRate: 1.5f, attackRange: 1.3f, moneyReward: 12, xpReward: 1);

        // Tank: yapılara ×2, önce barikat (15 m), aggro 3 m, geri itilmez.
        public static readonly EnemyDefinition Tank = new EnemyDefinition(
            "Tank", "tank", "Tank", PlaceholderMaterialId.Tank, Vector3.one * 1.6f, prewarm: 5,
            maxHealth: 400f, moveSpeed: 1.6f, damage: 35f, attackRate: 0.6f, attackRange: 2f, moneyReward: 60, xpReward: 5,
            aggroRadius: 3f, preferredTarget: TargetType.Barricade, structureDamageMultiplier: 2f, knockbackResistance: 1f);

        public static readonly IReadOnlyList<EnemyDefinition> All = new[] { Walker, Runner, Tank };
    }

    /// <summary>Bir düşman tipinin üretici tanımı. Gövde ölçeği 2 m boy / 0.5 m yarıçaplı kapsül primitive'ine göredir.</summary>
    public readonly struct EnemyDefinition
    {
        /// <summary>Kapsül primitive'inin boyu ve yarıçapı (ölçek 1).</summary>
        public const float CapsuleHeight = 2f;
        public const float CapsuleRadius = 0.5f;

        // EnemyData varsayılanı: 2 m boya 2.2 m gömülme. Büyük gövde yerden çıkarken toprağın üstünde görünmesin diye boyla orantılı.
        private const float BurrowDepthPerHeight = 1.1f;

        // GDD §7 kural 1: oyuncu 6 m içindeyse ona saldırılır (Tank 3 m).
        private const float DefaultAggroRadius = 6f;

        public EnemyDefinition(
            string assetName,
            string id,
            string displayName,
            PlaceholderMaterialId material,
            Vector3 bodyScale,
            int prewarm,
            float maxHealth,
            float moveSpeed,
            float damage,
            float attackRate,
            float attackRange,
            int moneyReward,
            int xpReward,
            float aggroRadius = DefaultAggroRadius,
            TargetType? preferredTarget = null,
            float structureDamageMultiplier = 1f,
            float knockbackResistance = 0f)
        {
            AssetName = assetName;
            Id = id;
            DisplayName = displayName;
            Material = material;
            BodyScale = bodyScale;
            Prewarm = prewarm;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            Damage = damage;
            AttackRate = attackRate;
            AttackRange = attackRange;
            MoneyReward = moneyReward;
            XpReward = xpReward;
            AggroRadius = aggroRadius;
            PreferredTarget = preferredTarget;
            StructureDamageMultiplier = structureDamageMultiplier;
            KnockbackResistance = knockbackResistance;
        }

        /// <summary>Asset ve prefab dosya adı (ör. "Walker").</summary>
        public string AssetName { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public PlaceholderMaterialId Material { get; }

        /// <summary>Visual altındaki kapsülün ölçeği (GDD §18).</summary>
        public Vector3 BodyScale { get; }
        public int Prewarm { get; }
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float Damage { get; }
        public float AttackRate { get; }
        public float AttackRange { get; }
        public int MoneyReward { get; }
        public int XpReward { get; }
        public float AggroRadius { get; }
        public TargetType? PreferredTarget { get; }
        public float StructureDamageMultiplier { get; }
        public float KnockbackResistance { get; }

        /// <summary>Gövde boyu (m): collider ve NavMeshAgent boyu.</summary>
        public float BodyHeight => CapsuleHeight * BodyScale.y;

        /// <summary>Gövde yarıçapı (m): collider ve NavMeshAgent yarıçapı.</summary>
        public float BodyRadius => CapsuleRadius * Mathf.Max(BodyScale.x, BodyScale.z);

        /// <summary>Yerden çıkma / batma derinliği (m).</summary>
        public float BurrowDepth => BurrowDepthPerHeight * BodyHeight;

        /// <summary>Test adlarında ve loglarda tipi ayırt etmek için.</summary>
        public override string ToString()
        {
            return AssetName;
        }
    }
}
