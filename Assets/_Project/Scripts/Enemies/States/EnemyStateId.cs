namespace LastHaven.Enemies
{
    /// <summary>Düşman durum makinesinin durumları. Değer = <see cref="EnemyStateMachine"/>'deki durum dizisinin indeksi.</summary>
    public enum EnemyStateId
    {
        /// <summary>Yerden çıkıyor; hareket etmez, saldırmaz.</summary>
        Spawning,

        /// <summary>Hedefe yürüyor (hedef yoksa bekler; gezginse dolaşmaya geçer).</summary>
        Chasing,

        /// <summary>Menzilde; durur, hedefe döner ve saldırı hızına göre vurur.</summary>
        Attacking,

        /// <summary>Öldü; ceset bekler, yere batar ve havuza döner.</summary>
        Dead,

        /// <summary>Gezgin modu (GDD §9): hedef yokken alanda kısa ayaklarla dolaşır, ayak aralarında bekler.</summary>
        Wandering,
    }
}
