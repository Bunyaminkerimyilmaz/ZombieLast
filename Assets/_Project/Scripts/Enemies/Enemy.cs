using LastHaven.Core;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşman prefab'ının kök bileşeni: NavMeshAgent + <see cref="Health"/> + havuz. Mantık saf C#'tadır:
    /// hedef seçimi <see cref="TargetSelector"/>'de, davranış <see cref="EnemyStateMachine"/>'de; bu sınıf onlara gövde olur.
    /// Havuzdan her çıkışta değerler <see cref="EnemyData"/>'dan yeniden kurulur. Ölünce <see cref="EnemyKilledEvent"/> gönderir.
    /// Animasyonlar (yürüme hızı, saldırı, ölüm) <see cref="AnimationDriver"/> üzerinden; vuruş tepkisi <see cref="EnemyHitReaction"/>'da.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public sealed class Enemy : MonoBehaviour, IPoolable, IEnemyBody
    {
        [SerializeField, Tooltip("Bu düşmanın tipi (GDD §7). Üretici bağlar.")]
        private EnemyData _data;

        private readonly TrackedTarget _target = new TrackedTarget();
        private Health _health;
        private AnimationDriver _animation;
        private Collider _collider;
        private EnemyNavigator _navigator;
        private EnemyStateMachine _machine;
        private TargetSelector _selector;
        private IPoolService _pool;
        private Vector3 _baseScale;
        private bool _servicesResolved;
        private bool _spawned;

        public EnemyData Data => _data;

        /// <summary>Şu anki durum. Awake'ten önce (prefab asset'inde) <see cref="EnemyStateId.Spawning"/>.</summary>
        public EnemyStateId State => _machine != null ? _machine.CurrentId : EnemyStateId.Spawning;

        public bool IsAlive => _spawned && !_machine.IsDead;

        /// <summary>Seçili ve hedeflenebilir hedef; yoksa null.</summary>
        public ITargetable Target => _target.Target;

        public bool HasTarget => _target.IsValid;

        public float DistanceToTarget => _target.DistanceFrom(transform.position);

        public void OnSpawned()
        {
            Spawn();
        }

        public void OnDespawned()
        {
            _spawned = false;
            _navigator.Deactivate();
            SetTarget(null);
        }

        void IEnemyBody.SetEmergence(float progress) => _navigator.SetEmergence(progress);

        void IEnemyBody.MoveToTarget() => _navigator.MoveTo(_target.Position);

        void IEnemyBody.Halt() => _navigator.Halt();

        void IEnemyBody.FaceTarget(float deltaTime) => _navigator.FaceTowards(_target.Position, deltaTime);

        void IEnemyBody.SetSink(float progress) => _navigator.SetSink(progress);

        void IEnemyBody.Attack()
        {
            if (_animation != null)
            {
                _animation.TriggerAttack();
            }

            if (!_target.TryGetHit(transform.position, transform.forward, out IDamageable damageable, out Vector3 hitPoint, out Vector3 direction))
            {
                return;
            }

            float amount = MeleeDamage.Calculate(_data.Damage, _target.Target.TargetType, _data.StructureDamageMultiplier);
            damageable.TakeDamage(new DamageInfo(amount, DamageType.Melee, gameObject, hitPoint, direction));
        }

        void IEnemyBody.Despawn()
        {
            if (_pool != null)
            {
                _pool.Release(gameObject);
                return;
            }

            Destroy(gameObject);
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _animation = GetComponent<AnimationDriver>();
            _collider = GetComponent<Collider>();
            _navigator = new EnemyNavigator(GetComponent<NavMeshAgent>());
            _machine = new EnemyStateMachine(this);
            _baseScale = transform.localScale;

            if (_data == null)
            {
                Debug.LogError($"Enemy: '{name}' için EnemyData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
        }

        private void Start()
        {
            // Sahneye elle konmuş (havuzdan gelmeyen) düşman da çalışsın.
            if (!_spawned)
            {
                Spawn();
            }
        }

        private void Update()
        {
            if (!_spawned)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (!_machine.IsDead)
            {
                _selector.Tick(deltaTime, transform.position);
                SetTarget(_selector.Current);
            }

            _machine.Tick(deltaTime);
            if (_animation != null)
            {
                _animation.SetSpeed(_navigator.Speed);
            }
        }

        private void Spawn()
        {
            if (_data == null)
            {
                return;
            }

            ResolveServices();
            _spawned = true;

            transform.localScale = _baseScale * _data.Scale;
            _health.SetMax(_data.MaxHealth, refill: true);
            if (_collider != null)
            {
                _collider.enabled = true;
            }

            _navigator.Activate(_data.MoveSpeed, _data.BurrowDepth * _data.Scale, _data.RepathDistance);

            TargetSelectorSettings targeting = _data.CreateTargetSelectorSettings();
            _selector.Reset(targeting, Random.Range(0f, targeting.EvaluationInterval));
            SetTarget(null);
            _machine.Begin(_data.CreateBehaviourSettings());
        }

        private void ResolveServices()
        {
            if (_servicesResolved)
            {
                return;
            }

            _servicesResolved = true;
            if (!ServiceLocator.TryGet(out ITargetRegistry registry))
            {
                Debug.LogWarning("Enemy: ITargetRegistry bulunamadı; düşman hedef seçemeyecek. Sahnede [Systems]/TargetRegistry var mı?", this);
            }

            ServiceLocator.TryGet(out _pool);
            _selector = new TargetSelector(registry, _data.CreateTargetSelectorSettings());
        }

        private void SetTarget(ITargetable target)
        {
            if (_target.Set(target))
            {
                _navigator.InvalidatePath();
            }
        }

        private void OnDied(DiedEvent died)
        {
            if (!_spawned || _machine.IsDead)
            {
                return;
            }

            if (_collider != null)
            {
                _collider.enabled = false;
            }

            _navigator.BeginDeath();
            SetTarget(null);
            _machine.Kill();
            if (_animation != null)
            {
                _animation.TriggerDie();
            }

            GameObject source = died.KillingBlow.Source;
            bool killedByPlayer = source != null && source.CompareTag(GameTags.Player);
            EventBus<EnemyKilledEvent>.Raise(new EnemyKilledEvent(
                transform.position, _data.Id, _data.MoneyReward, _data.XpReward, killedByPlayer));
        }
    }
}
