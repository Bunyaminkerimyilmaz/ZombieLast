using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Üretilen düşman tipleri: GDD §7 değerleri, §18 görünüm (renk + gövde şekli/ölçeği), saldırı stratejisi ve havuz ön ısıtma sayısı.
    /// Değerler asset'lere sadece ilk oluşturmada yazılır.
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

        // Tükürücü: mor kapsül, 10 m'den yay çizen asit mermisi, önce taret (15 m). Ön ısıtma: en kalabalık gece (9) 15.
        public static readonly EnemyDefinition Spitter = new EnemyDefinition(
            "Spitter", NightDefinitions.SpitterId, "Tükürücü", PlaceholderMaterialId.Spitter, Vector3.one, prewarm: 15,
            maxHealth: 70f, moveSpeed: 2.5f, damage: 12f, attackRate: 0.5f, attackRange: 10f, moneyReward: 25, xpReward: 2,
            preferredTarget: TargetType.Turret, attack: EnemyAttackKind.Ranged);

        // Patlayan: turuncu küre, yürürken nabız atar; 1.2 m'de değince/ölünce 3 m'lik 60 hasar (yapılara ×2). Saldırı hızı GDD'de
        // yok (tek seferlik); 1. Patlayınca ceset kalmaz (efekt kapatır). Gövde çapı GDD'de yok: 1.2 m.
        public static readonly EnemyDefinition Exploder = new EnemyDefinition(
            "Exploder", NightDefinitions.ExploderId, "Patlayan", PlaceholderMaterialId.Exploder, Vector3.one * 1.2f, prewarm: 15,
            maxHealth: 50f, moveSpeed: 3.5f, damage: 60f, attackRate: 1f, attackRange: 1.2f, moneyReward: 20, xpReward: 2,
            structureDamageMultiplier: 2f, attack: EnemyAttackKind.SuicideExplode, bodyShape: PrimitiveType.Sphere,
            corpseDuration: 0f, sinkDuration: 0f, walkPulseScale: 0.15f);

        // Brute (boss, Gece 5): siyah-kırmızı dev kapsül (×2.5). 6 sn'de bir, 1 sn uyarılı 4 m'lik yere vurma (40), %50 can altında
        // öfke (BossConfig), geri itilmez. Final (Gece 10) aynı tip, 6000 can; ayrı prefab (her verinin kendi prefab'ı olmalı).
        public static readonly EnemyDefinition Brute = new EnemyDefinition(
            "Brute", NightDefinitions.BruteId, "Brute", PlaceholderMaterialId.Brute, Vector3.one * 2.5f, prewarm: 1,
            maxHealth: 3000f, moveSpeed: 1.8f, damage: 40f, attackRate: BruteAttackRate, attackRange: 4f, moneyReward: 500, xpReward: 30,
            knockbackResistance: 1f, attack: EnemyAttackKind.GroundSlam, isBoss: true);

        public static readonly EnemyDefinition BruteFinal = new EnemyDefinition(
            "BruteFinal", NightDefinitions.BruteFinalId, "Brute (Final)", PlaceholderMaterialId.Brute, Vector3.one * 2.5f, prewarm: 1,
            maxHealth: 6000f, moveSpeed: 1.8f, damage: 40f, attackRate: BruteAttackRate, attackRange: 4f, moneyReward: 500, xpReward: 30,
            knockbackResistance: 1f, attack: EnemyAttackKind.GroundSlam, isBoss: true);

        public static readonly IReadOnlyList<EnemyDefinition> All = new[] { Walker, Runner, Tank, Spitter, Exploder, Brute, BruteFinal };

        // GDD §7: 6 sn'de bir.
        private const float BruteAttackRate = 1f / 6f;
    }

    /// <summary>Düşman tipinin saldırı stratejisi (<c>LastHaven.Enemies.IEnemyAttack</c> uygulamaları).</summary>
    public enum EnemyAttackKind
    {
        Melee,
        Ranged,
        SuicideExplode,
        GroundSlam,
    }

    /// <summary>
    /// Bir düşman tipinin üretici tanımı. Gövde ölçeği primitive'e göredir: kapsül 2 m boy / 0.5 m yarıçap, küre 1 m çap.
    /// </summary>
    public readonly struct EnemyDefinition
    {
        /// <summary>Kapsül primitive'inin boyu ve yarıçapı (ölçek 1).</summary>
        public const float CapsuleHeight = 2f;
        public const float CapsuleRadius = 0.5f;

        /// <summary>Küre primitive'inin çapı (ölçek 1).</summary>
        public const float SphereDiameter = 1f;

        // EnemyData varsayılanı: 2 m boya 2.2 m gömülme. Büyük gövde yerden çıkarken toprağın üstünde görünmesin diye boyla orantılı.
        private const float BurrowDepthPerHeight = 1.1f;

        // GDD §7 kural 1: oyuncu 6 m içindeyse ona saldırılır (Tank 3 m).
        private const float DefaultAggroRadius = 6f;

        // GDD §7: ölünce devrilir, 2 sn sonra 1 sn'de yere batar.
        private const float DefaultCorpseDuration = 2f;
        private const float DefaultSinkDuration = 1f;

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
            float knockbackResistance = 0f,
            EnemyAttackKind attack = EnemyAttackKind.Melee,
            PrimitiveType bodyShape = PrimitiveType.Capsule,
            float corpseDuration = DefaultCorpseDuration,
            float sinkDuration = DefaultSinkDuration,
            float walkPulseScale = 0f,
            bool isBoss = false)
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
            Attack = attack;
            BodyShape = bodyShape;
            CorpseDuration = corpseDuration;
            SinkDuration = sinkDuration;
            WalkPulseScale = walkPulseScale;
            IsBoss = isBoss;
        }

        /// <summary>Asset ve prefab dosya adı (ör. "Walker").</summary>
        public string AssetName { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public PlaceholderMaterialId Material { get; }

        /// <summary>Visual altındaki gövdenin ölçeği (GDD §18).</summary>
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
        public EnemyAttackKind Attack { get; }

        /// <summary>Gövde primitive'i: kapsül veya küre (GDD §18 Patlayan).</summary>
        public PrimitiveType BodyShape { get; }

        /// <summary>Ölümden sonra cesedin bekleme ve batma süreleri (sn).</summary>
        public float CorpseDuration { get; }
        public float SinkDuration { get; }

        /// <summary>Yürürken Visual'ın nabız gibi büyüme oranı (0 = yok).</summary>
        public float WalkPulseScale { get; }

        /// <summary>Boss mu: prefab'a <c>BossBehaviour</c> (öfke, HUD olayları) eklenir.</summary>
        public bool IsBoss { get; }

        /// <summary>Gövde boyu (m): collider ve NavMeshAgent boyu.</summary>
        public float BodyHeight => (BodyShape == PrimitiveType.Sphere ? SphereDiameter : CapsuleHeight) * BodyScale.y;

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
