namespace LastHaven.Enemies
{
    /// <summary>
    /// Bir düşmanın saldırısının çalışma zamanı (<see cref="IEnemyAttack.CreateRuntime"/>). <see cref="Enemy"/> yaşam döngüsünü,
    /// durum makinesi (<see cref="EnemyStateId.Attacking"/>, bekleme dolunca) <see cref="Perform"/>'u çağırır.
    /// </summary>
    public interface IEnemyAttackRuntime
    {
        /// <summary>
        /// Saldırı sürüyor (ör. yere vurmanın 1 sn'lik hazırlığı): düşman bu sırada saldırıdan çıkmaz, yerinde kalır. Anlık saldırılarda
        /// hep false.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>Havuzdan çıkış (değerler data'dan yeniden kurulduktan sonra).</summary>
        void OnSpawned();

        /// <summary>Havuza dönüş.</summary>
        void OnDespawned();

        /// <summary>Düşman canlıyken her kare.</summary>
        void Tick(float deltaTime);

        /// <summary>Hedef menzildeyken saldırı anı (saldırı hızına göre).</summary>
        void Perform();

        /// <summary>Düşman öldü (ölüm duyurusundan önce).</summary>
        void OnDied();
    }
}
