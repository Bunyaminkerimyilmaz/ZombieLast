using System;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın saf C# durum makinesi: Spawning → Chasing ⇄ Attacking → Dead; gezgin modunda (<see cref="StartWandering"/>)
    /// hedef yokken Chasing → Wandering. Durumlar <see cref="IEnemyBody"/> üzerinden Unity'ye dokunur. Havuzdan her çıkışta
    /// <see cref="Begin"/> ile baştan başlar; durum nesneleri yeniden kullanılır.
    /// </summary>
    public sealed class EnemyStateMachine
    {
        private readonly EnemyState[] _states;
        private readonly Func<float> _random01;
        private EnemyState _current;

        /// <param name="random01">Gezinme beklemesi için [0, 1) üreten kaynak; null ise <c>UnityEngine.Random.value</c>.</param>
        public EnemyStateMachine(IEnemyBody body, Func<float> random01 = null)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
            _random01 = random01 ?? DefaultRandom;

            // İndeks = EnemyStateId.
            _states = new EnemyState[]
            {
                new SpawningState(this),
                new ChasingState(this),
                new AttackingState(this),
                new DeadState(this),
                new WanderingState(this),
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

        /// <summary>Gezgin modunda mı (GDD §9): hedef yokken beklemek yerine dolaşır. <see cref="Begin"/> kapatır.</summary>
        public bool IsWanderer { get; private set; }

        /// <summary>Son <see cref="StartWandering"/>'in ayarları.</summary>
        public WanderSettings Wander { get; private set; }

        /// <summary>Ayarları yeniler, saldırı bekleme süresini sıfırlar, gezgin modunu kapatır ve Spawning'den başlar.</summary>
        public void Begin(in EnemyBehaviourSettings settings)
        {
            Settings = settings;
            AttackCooldown = 0f;
            IsWanderer = false;
            ChangeState(EnemyStateId.Spawning);
        }

        /// <summary>Gezgin moduna geçer: hedefi yokken (Chasing'de) <see cref="EnemyStateId.Wandering"/>'e geçer.</summary>
        public void StartWandering(in WanderSettings settings)
        {
            Wander = settings;
            IsWanderer = true;
        }

        /// <summary>Gezgin modundan çıkar: dolaşıyorsa bir sonraki Tick'te kovalamaya döner.</summary>
        public void StopWandering()
        {
            IsWanderer = false;
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

        /// <summary>[0, 1) rastgele sayı (gezinme beklemesi).</summary>
        internal float NextRandom()
        {
            return _random01();
        }

        private static float DefaultRandom()
        {
            return UnityEngine.Random.value;
        }
    }
}
