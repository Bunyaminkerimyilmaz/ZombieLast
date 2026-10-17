namespace LastHaven.Enemies
{
    /// <summary>
    /// Durumların Unity tarafıyla konuştuğu köprü (NavMeshAgent, transform, hasar). <see cref="EnemyBody"/> uygular;
    /// testler sahte bir gövdeyle durum makinesini Unity olmadan çalıştırır.
    /// </summary>
    public interface IEnemyBody
    {
        /// <summary>Seçili ve hedeflenebilir bir hedef var mı.</summary>
        bool HasTarget { get; }

        /// <summary>Hedefin yüzeyine yatay uzaklık (m). Hedef yoksa +∞.</summary>
        float DistanceToTarget { get; }

        /// <summary>Başlamış saldırı sürüyor (ör. yere vurma hazırlığı); bitene kadar saldırı durumundan çıkılmaz.</summary>
        bool IsAttackBusy { get; }

        /// <summary>Yerden çıkma: 0 = tamamen gömülü, 1 = yüzeyde.</summary>
        void SetEmergence(float progress);

        /// <summary>Hedefe doğru yürür (yol gerekirse yeniden hesaplanır).</summary>
        void MoveToTarget();

        /// <summary>Yürümeyi durdurur.</summary>
        void Halt();

        /// <summary>Yerinde hedefe doğru döner.</summary>
        void FaceTarget(float deltaTime);

        /// <summary>Hedefe bir yakın dövüş vuruşu yapar.</summary>
        void Attack();

        /// <summary>
        /// Gezinme ayağı: <paramref name="settings"/> halkasında rastgele bir noktaya (en yakın NavMesh noktası) yürümeye başlar.
        /// Nokta bulunamazsa veya yürünemiyorsa false.
        /// </summary>
        bool StartWanderLeg(in WanderSettings settings);

        /// <summary>Son yürüme hedefine vardı mı (kalan mesafe ≤ durma mesafesi + <paramref name="tolerance"/>; yol yoksa da true).</summary>
        bool HasArrived(float tolerance);

        /// <summary>Yere batma: 0 = yüzeyde, 1 = tamamen gömülü.</summary>
        void SetSink(float progress);

        /// <summary>Ceset işi bitti; havuza dön.</summary>
        void Despawn();
    }
}
