using LastHaven.Core;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşman prefab'ının kök bileşeni: NavMeshAgent + <see cref="Health"/> + havuz. Mantık saf C#'tadır:
    /// hedef seçimi <see cref="TargetSelector"/>'de (<see cref="EnemyTargeting"/> üzerinden), davranış <see cref="EnemyStateMachine"/>'de,
    /// saldırı <see cref="EnemyData.Attack"/> stratejisinde (<see cref="IEnemyAttackRuntime"/>); Unity tarafı <see cref="EnemyBody"/>'de.
    /// Havuzdan her çıkışta değerler <see cref="EnemyData"/>'dan yeniden kurulur. Ölünce <see cref="EnemyKilledEvent"/> gönderir.
    /// Animasyonlar <see cref="AnimationDriver"/> üzerinden; vuruş tepkisi <see cref="EnemyHitReaction"/>'da.
    /// Gündüz gezgini (GDD §9) <see cref="BeginWander"/> ile olur, <see cref="EndWander"/> ile normal düşmana döner.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public sealed class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Bu düşmanın tipi (GDD §7). Üretici bağlar.")]
        private EnemyData _data;

        [SerializeField, Tooltip("Opsiyonel mermi çıkış noktası (Visual dışında; Tükürücü). Boşsa gövde merkezi. Üretici bağlar.")]
        private Transform _muzzle;

        private Health _health;
        private AnimationDriver _animation;
        private EnemyNavigator _navigator;
        private EnemyTargeting _targeting;
        private EnemyAttackContext _attackContext;
        private IEnemyAttackRuntime _attack;
        private EnemyBody _body;
        private EnemyStateMachine _machine;
        private Vector3 _baseScale;
        private bool _servicesResolved;
        private bool _spawned;

        public EnemyData Data => _data;

        /// <summary>Şu anki durum. Awake'ten önce (prefab asset'inde) <see cref="EnemyStateId.Spawning"/>.</summary>
        public EnemyStateId State => _machine != null ? _machine.CurrentId : EnemyStateId.Spawning;

        public bool IsAlive => _spawned && !_machine.IsDead;

        /// <summary>Gezgin modunda mı (GDD §9): üsse gitmez, alanda dolaşır, sadece aggro yarıçapındaki oyuncuya saldırır.</summary>
        public bool IsWanderer => _machine != null && _machine.IsWanderer;

        /// <summary>Seçili ve hedeflenebilir hedef; yoksa null.</summary>
        public ITargetable Target => _targeting.Target.Target;

        /// <summary>GDD §7 kural 4 etkinse yolu tıkalı olan asıl hedef (seçili hedef o zaman engeldir); değilse null.</summary>
        public ITargetable BlockedTarget => _targeting.BlockedTarget;

        public bool HasTarget => _targeting.Target.IsValid;

        public float DistanceToTarget => _targeting.Target.DistanceFrom(transform.position);

        /// <summary>Yürüme hızı sınırı (m/sn): data hızı × hız çarpanı (yavaşlatma).</summary>
        public float MaxSpeed => _navigator.MaxSpeed;

        /// <summary>Kaynağın hız çarpanı (yavaşlatma, boss öfkesi, gece olayı); çarpanlar çarpılır, her doğuşta 1'e döner.</summary>
        public void SetSpeedMultiplier(SpeedSource source, float multiplier) => _navigator?.SetSpeedMultiplier(source, multiplier); // Awake'ten önce (EditMode) yok.

        /// <summary>
        /// Gezgin moduna alır (GDD §9 gündüz tehlikesi): hedef sadece aggro yarıçapındaki oyuncu; oyuncu yokken
        /// <paramref name="settings"/> halkasında dolaşır. Havuzdan her çıkışta kapanır; canlı değilse bir şey yapmaz.
        /// </summary>
        public void BeginWander(in WanderSettings settings)
        {
            if (!IsAlive)
            {
                return;
            }

            _targeting.PlayerOnly = true;
            _machine.StartWandering(settings);
        }

        /// <summary>Gezgin modundan çıkarır: hedef seçimi normale (GDD §7 kural 1–4) döner, düşman üsse yürür.</summary>
        public void EndWander()
        {
            if (!_spawned)
            {
                return;
            }

            _targeting.PlayerOnly = false;
            _machine.StopWandering();
        }

        public void OnSpawned()
        {
            Spawn();
        }

        public void OnDespawned()
        {
            _spawned = false;
            _attack.OnDespawned();
            _navigator.Deactivate();
            _targeting.Clear();
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _animation = GetComponent<AnimationDriver>();
            var collider = GetComponent<Collider>();
            _navigator = new EnemyNavigator(GetComponent<NavMeshAgent>());
            _targeting = new EnemyTargeting(_navigator);
            _attackContext = new EnemyAttackContext(this, _health, collider, _muzzle, _navigator, _targeting);
            _attack = (_data != null ? _data.Attack : MeleeAttack.Default).CreateRuntime(_attackContext);
            _body = new EnemyBody(gameObject, _navigator, _targeting, _animation, _attack, _health, collider);
            _machine = new EnemyStateMachine(_body);
            _baseScale = transform.localScale;
            // Havuzlanan obje: abonelik ömür boyu bir kez (OnEnable/OnDisable her doğuşta delegate ayırırdı). Health aynı objede.
            _health.Died += OnDied;

            if (_data == null)
            {
                Debug.LogError($"Enemy: '{name}' için EnemyData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
            }
        }

        private void OnDestroy()
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
                _attack.Tick(deltaTime);
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
            _body.Activate(_data, _baseScale);

            // Seçici sıfırlanınca "sadece oyuncu" da kapanır; makine Begin'de gezgin modunu kapatır.
            TargetSelectorSettings targeting = _data.CreateTargetSelectorSettings();
            _targeting.Reset(targeting, Random.Range(0f, targeting.EvaluationInterval), _data.AttackRange);
            _machine.Begin(_data.CreateBehaviourSettings());
            _attack.OnSpawned();
        }

        private void ResolveServices()
        {
            if (_servicesResolved)
            {
                return;
            }

            _servicesResolved = true;
            ServiceLocator.TryGet(out IPoolService pool);
            _body.Pool = pool;
            _attackContext.Pool = pool;
            _targeting.Initialize(_data.CreateTargetSelectorSettings(), this);
        }

        private void OnDied(DiedEvent died)
        {
            if (!_spawned || _machine.IsDead)
            {
                return;
            }

            // Önce saldırı (Patlayan burada patlar): collider ve ajan henüz açık, konum yüzeyde ölçülür.
            _attack.OnDied();
            Vector3 deathPosition = _body.BeginDeath();
            _machine.Kill();
            EnemyDeathReport.Raise(_data, deathPosition, died.KillingBlow);
        }
    }
}
