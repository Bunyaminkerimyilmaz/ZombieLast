namespace LastHaven.Enemies
{
    /// <summary>Durum makinesinin süre ve menzil ayarları; <see cref="EnemyData.CreateBehaviourSettings"/>'ten gelir.</summary>
    public readonly struct EnemyBehaviourSettings
    {
        public EnemyBehaviourSettings(
            float spawnDuration,
            float attackRange,
            float disengageMargin,
            float attackInterval,
            float corpseDuration,
            float sinkDuration)
        {
            SpawnDuration = spawnDuration;
            AttackRange = attackRange;
            DisengageMargin = disengageMargin;
            AttackInterval = attackInterval;
            CorpseDuration = corpseDuration;
            SinkDuration = sinkDuration;
        }

        /// <summary>Yerden çıkma süresi (sn).</summary>
        public float SpawnDuration { get; }

        /// <summary>Hedef yüzeyine bu mesafede (dahil) saldırıya geçer (m).</summary>
        public float AttackRange { get; }

        /// <summary>Saldırırken mesafe menzil + bunu aşınca kovalamaya döner (m).</summary>
        public float DisengageMargin { get; }

        /// <summary>İki saldırı arası süre (sn).</summary>
        public float AttackInterval { get; }

        /// <summary>Ölümden sonra batmaya başlamadan önceki bekleme (sn).</summary>
        public float CorpseDuration { get; }

        /// <summary>Yere batma süresi (sn).</summary>
        public float SinkDuration { get; }
    }
}
