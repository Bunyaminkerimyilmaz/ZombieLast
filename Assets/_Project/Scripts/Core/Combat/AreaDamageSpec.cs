namespace LastHaven.Core
{
    /// <summary><see cref="AreaDamage"/> ayarları: yarıçap, hasar, yapı çarpanı, mesafeyle azalma, etkilenen layer'lar.</summary>
    public readonly struct AreaDamageSpec
    {
        public AreaDamageSpec(float radius, float damage, float structureMultiplier, float edgeDamageFraction, int layerMask, DamageType damageType)
        {
            Radius = radius;
            Damage = damage;
            StructureMultiplier = structureMultiplier;
            EdgeDamageFraction = edgeDamageFraction;
            LayerMask = layerMask;
            DamageType = damageType;
        }

        /// <summary>Alanın yarıçapı (m): merkezden collider yüzeyine.</summary>
        public float Radius { get; }

        /// <summary>Merkezdeki hasar.</summary>
        public float Damage { get; }

        /// <summary>Structure layer'ındaki hedeflere (jeneratör, barikat, taret, tuzak) uygulanan çarpan.</summary>
        public float StructureMultiplier { get; }

        /// <summary>Yarıçap sınırındaki hasarın merkez hasarına oranı: 1 = mesafeyle azalmaz, 0 = sınırda 0. Arası doğrusal.</summary>
        public float EdgeDamageFraction { get; }

        /// <summary>Hasar alabilecek layer'lar.</summary>
        public int LayerMask { get; }

        public DamageType DamageType { get; }
    }
}
