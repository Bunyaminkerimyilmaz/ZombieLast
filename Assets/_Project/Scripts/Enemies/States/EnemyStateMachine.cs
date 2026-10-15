using System;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın saf C# durum makinesi: Spawning → Chasing ⇄ Attacking → Dead. Durumlar <see cref="IEnemyBody"/> üzerinden
    /// Unity'ye dokunur. Havuzdan her çıkışta <see cref="Begin"/> ile baştan başlar; durum nesneleri yeniden kullanılır.
    /// </summary>
    public sealed class EnemyStateMachine
    {
        private readonly EnemyState[] _states;
        private EnemyState _current;

        public EnemyStateMachine(IEnemyBody body)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));

            // İndeks = EnemyStateId.
            _states = new EnemyState[]
            {
                new SpawningState(this),
                new ChasingState(this),
                new AttackingState(this),
                new DeadState(this),
            };
        }

        /// <summary>Durum değişince (yeni durumun Enter'ından sonra) çağrılır.</summary>
        public event Action<EnemyStateId> StateChanged;

        public IEnemyBody Body { get; }

        public EnemyBehaviourSettings Settings { get; private set; }

        /// <summary><see cref="Begin"/> çağrıldı mı.</summary>
        public bool HasBegun => _current != null;

        /// <summary>Şu anki durum. <see cref="Begin"/>'den önce <see cref="EnemyStateId.Spawning"/> döner.</summary>
        public EnemyStateId CurrentId => _current?.Id ?? EnemyStateId.Spawning;

        public bool IsDead => _current != null && _current.Id == EnemyStateId.Dead;

        /// <summary>Bir sonraki saldırıya kalan süre (sn). Ölü değilken her durumda azalır; menzile girip çıkmak sıfırlamaz.</summary>
        public float AttackCooldown { get; private set; }

        /// <summary>Ayarları yeniler, saldırı bekleme süresini sıfırlar ve Spawning'den başlar.</summary>
        public void Begin(in EnemyBehaviourSettings settings)
        {
            Settings = settings;
            AttackCooldown = 0f;
            ChangeState(EnemyStateId.Spawning);
        }

        /// <summary>Dead durumuna geçer (zaten ölüyse bir şey yapmaz).</summary>
        public void Kill()
        {
            if (!IsDead)
            {
                ChangeState(EnemyStateId.Dead);
            }
        }

        public void Tick(float deltaTime)
        {
            if (_current == null)
            {
                return;
            }

            if (!IsDead && AttackCooldown > 0f)
            {
                AttackCooldown = Math.Max(0f, AttackCooldown - deltaTime);
            }

            _current.Tick(deltaTime);
        }

        internal void ChangeState(EnemyStateId id)
        {
            _current?.Exit();
            _current = _states[(int)id];
            _current.Enter();
            StateChanged?.Invoke(id);
        }

        /// <summary>Saldırı hazırsa bekleme süresini yeniden kurar ve true döner.</summary>
        internal bool TryConsumeAttack()
        {
            if (AttackCooldown > 0f)
            {
                return false;
            }

            AttackCooldown = Settings.AttackInterval;
            return true;
        }
    }
}
