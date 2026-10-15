using LastHaven.Core;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşman prefab'ının kök bileşeni: NavMeshAgent + <see cref="Health"/> + havuz. Mantık saf C#'tadır:
    /// hedef seçimi <see cref="TargetSelector"/>'de (<see cref="EnemyTargeting"/> üzerinden), davranış <see cref="EnemyStateMachine"/>'de;
    /// bu sınıf onlara gövde olur. Havuzdan her çıkışta değerler <see cref="EnemyData"/>'dan yeniden kurulur. Ölünce
    /// <see cref="EnemyKilledEvent"/> gönderir. Animasyonlar <see cref="AnimationDriver"/> üzerinden; vuruş tepkisi <see cref="EnemyHitReaction"/>'da.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public sealed class Enemy : MonoBehaviour, IPoolable, IEnemyBody
    {
        [SerializeField, Tooltip("Bu düşmanın tipi (GDD §7). Üretici bağlar.")]
        private EnemyData _data;

        private Health _health;
        private AnimationDriver _animation;
        private Collider _collider;
        private EnemyNavigator _navigator;
        private EnemyTargeting _targeting;
        private EnemyStateMachine _machine;
        private IPoolService _pool;
        private Vector3 _baseScale;
        private bool _servicesResolved;
        private bool _spawned;

        public EnemyData Data => _data;

        /// <summary>Şu anki durum. Awake'ten önce (prefab asset'inde) <see cref="EnemyStateId.Spawning"/>.</summary>
        public EnemyStateId State => _machine != null ? _machine.CurrentId : EnemyStateId.Spawning;

        public bool IsAlive => _spawned && !_machine.IsDead;

        /// <summary>Seçili ve hedeflenebilir hedef; yoksa null.</summary>
        public ITargetable Target => _targeting.Target.Target;

        /// <summary>GDD §7 kural 4 etkinse yolu tıkalı olan asıl hedef (seçili hedef o zaman engeldir); değilse null.</summary>
        public ITargetable BlockedTarget => _targeting.BlockedTarget;

        public bool HasTarget => _targeting.Target.IsValid;

        public float DistanceToTarget => _targeting.Target.DistanceFrom(transform.position);

        public void OnSpawned()
        {
            Spawn();
        }

        public void OnDespawned()
        {
            _spawned = false;
            _navigator.Deactivate();
            _targeting.Clear();
        }

        void IEnemyBody.SetEmergence(float progress) => _navigator.SetEmergence(progress);

        void IEnemyBody.MoveToTarget() => _navigator.MoveTo(_targeting.Target.Position);

        void IEnemyBody.Halt() => _navigator.Halt();

        void IEnemyBody.FaceTarget(float deltaTime) => _navigator.FaceTowards(_targeting.Target.Position, deltaTime);

        void IEnemyBody.SetSink(float progress) => _navigator.SetSink(progress);

        void IEnemyBody.Attack()
        {
            if (_animation != null)
            {
                _animation.TriggerAttack();
            }

            _targeting.TryStrike(transform.position, transform.forward, _data, gameObject);
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
            _targeting = new EnemyTargeting(_navigator);
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
                _targeting.Tick(deltaTime, transform.position);
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

            _navigator.Activate(_data.MoveSpeed, _data.BurrowDepth * _data.Scale, _data.RepathDistance, _data.PickAvoidancePriority());

            TargetSelectorSettings targeting = _data.CreateTargetSelectorSettings();
            _targeting.Reset(targeting, Random.Range(0f, targeting.EvaluationInterval), _data.AttackRange);
            _machine.Begin(_data.CreateBehaviourSettings());
        }

        private void ResolveServices()
        {
            if (_servicesResolved)
            {
                return;
            }

            _servicesResolved = true;
            ServiceLocator.TryGet(out _pool);
            _targeting.Initialize(_data.CreateTargetSelectorSettings(), this);
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

            // Yerden çıkarken öldüyse transform gömülü; jetonlar yüzeye düşsün. BeginDeath ajanı kapatmadan önce alınır.
            Vector3 deathPosition = _navigator.SurfacePosition;
            _navigator.BeginDeath();
            _targeting.Clear();
            _machine.Kill();
            if (_animation != null)
            {
                _animation.TriggerDie();
            }

            EnemyDeathReport.Raise(_data, deathPosition, died.KillingBlow);
        }
    }
}
