using System;
using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Yakın dövüş (Yürüyen, Koşucu, Tank): saldırı anında hedefe bir vuruş, yapılara <see cref="EnemyData.StructureDamageMultiplier"/>
    /// (<see cref="MeleeDamage"/>). Vuruş noktası hedefin yüzeyinde. Ayarı yok; hasar ve menzil <see cref="EnemyData"/>'da.
    /// </summary>
    [Serializable]
    public sealed class MeleeAttack : IEnemyAttack
    {
        /// <summary>Saldırısı atanmamış veri için yedek (eski asset'ler; davranış 12.1 öncesiyle aynı).</summary>
        internal static readonly MeleeAttack Default = new MeleeAttack();

        public void Prewarm(IPoolService pool)
        {
        }

        public IEnemyAttackRuntime CreateRuntime(EnemyAttackContext context)
        {
            return new Runtime(context);
        }

        private sealed class Runtime : IEnemyAttackRuntime
        {
            private readonly EnemyAttackContext _context;

            public Runtime(EnemyAttackContext context)
            {
                _context = context;
            }

            public bool IsBusy => false;

            public void OnSpawned()
            {
            }

            public void OnDespawned()
            {
            }

            public void Tick(float deltaTime)
            {
            }

            public void Perform()
            {
                _context.Targeting.TryStrike(_context.Transform.position, _context.Transform.forward, _context.Data, _context.GameObject);
            }

            public void OnDied()
            {
            }
        }
    }
}
