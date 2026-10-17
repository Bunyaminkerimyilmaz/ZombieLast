using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşman saldırı stratejisi (GDD §7): yakın dövüş (<see cref="MeleeAttack"/>), menzilli mermi (<see cref="RangedAttack"/>),
    /// kendini patlatma (<see cref="SuicideExplodeAttack"/>). <see cref="EnemyData"/>'da <c>[SerializeReference]</c> ile seçilir ve
    /// o tipin tüm düşmanlarınca paylaşılır: sadece ayar tutar. Düşman başına durum <see cref="CreateRuntime"/>'ın nesnesindedir.
    /// Hasar, menzil ve saldırı hızı <see cref="EnemyData"/>'dadır.
    /// </summary>
    public interface IEnemyAttack
    {
        /// <summary>Saldırının havuzlanan yardımcılarını (mermi, efekt) ön ısıtır. Spawner oyun başında tip başına bir kez çağırır.</summary>
        void Prewarm(IPoolService pool);

        /// <summary>Düşman başına bir kez (Awake) çağrılır; dönen nesne havuz döngüleri boyunca yeniden kullanılır.</summary>
        IEnemyAttackRuntime CreateRuntime(EnemyAttackContext context);
    }
}
