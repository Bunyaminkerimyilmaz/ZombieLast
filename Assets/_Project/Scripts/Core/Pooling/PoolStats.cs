namespace LastHaven.Core
{
    /// <summary>Bir prefab havuzunun anlık sayıları (debug ve testler için).</summary>
    public readonly struct PoolStats
    {
        public PoolStats(int created, int active, int inactive)
        {
            Created = created;
            Active = active;
            Inactive = inactive;
        }

        /// <summary>Şimdiye kadar oluşturulan örnek sayısı. Prewarm sonrası artıyorsa oynanışta Instantiate yapılıyordur.</summary>
        public int Created { get; }

        /// <summary>Havuzdan alınmış, sahnede kullanılan örnekler.</summary>
        public int Active { get; }

        /// <summary>Havuzda bekleyen kapalı örnekler.</summary>
        public int Inactive { get; }
    }
}
