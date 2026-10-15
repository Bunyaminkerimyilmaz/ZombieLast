namespace LastHaven.Enemies
{
    /// <summary>Düşman durum makinesinin durumları.</summary>
    public enum EnemyStateId
    {
        /// <summary>Yerden çıkıyor; hareket etmez, saldırmaz.</summary>
        Spawning,

        /// <summary>Hedefe yürüyor (hedef yoksa bekler).</summary>
        Chasing,

        /// <summary>Menzilde; durur, hedefe döner ve saldırı hızına göre vurur.</summary>
        Attacking,

        /// <summary>Öldü; ceset bekler, yere batar ve havuza döner.</summary>
        Dead,
    }
}
