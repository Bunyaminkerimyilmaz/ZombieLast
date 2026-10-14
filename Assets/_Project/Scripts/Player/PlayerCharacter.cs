using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncu prefab'ının kök bileşeni: canı PlayerData'dan kurar, <see cref="IPlayer"/> servisi olarak kaydolur ve
    /// kendini <see cref="ITargetRegistry"/>'ye <see cref="TargetType.Player"/> olarak ekler. Atılma sırasında hasarı engeller.
    /// </summary>
    /// <remarks>Adı "Player" değil: <c>LastHaven.Player</c> namespace'iyle çakışırdı.</remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(PlayerMotor), typeof(PlayerAim))]
    public sealed class PlayerCharacter : MonoBehaviour, IPlayer, ITargetable, IDamageFilter
    {
        [SerializeField, Tooltip("Oyuncu ayarları (GDD §5).")]
        private PlayerData _data;

        private Health _health;
        private PlayerMotor _motor;
        private PlayerAim _aim;
        private ITargetRegistry _registry;
        private bool _started;

        public Transform Transform => transform;

        public Vector3 Position => transform.position;

        public Vector3 AimPoint => _aim.AimPoint;

        public Vector3 AimDirection => _aim.AimDirection;

        public bool IsAlive => _health.IsAlive;

        public float PickupRadius => _data != null ? _data.PickupRadius : 0f;

        public TargetType TargetType => TargetType.Player;

        public bool IsTargetable => isActiveAndEnabled && _health.IsAlive;

        public bool CanTakeDamage(in DamageInfo damage)
        {
            return !_motor.IsInvulnerable;
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _motor = GetComponent<PlayerMotor>();
            _aim = GetComponent<PlayerAim>();

            if (_data != null)
            {
                _health.SetMax(_data.MaxHealth, refill: true);
            }
            else
            {
                Debug.LogError("PlayerCharacter: PlayerData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
            }

            _health.DamageFilter = this;
            ServiceLocator.Register<IPlayer>(this);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("PlayerCharacter: ITargetRegistry bulunamadı; düşmanlar oyuncuyu hedefleyemeyecek. Sahnede [Systems]/TargetRegistry var mı?", this);
            }

            _started = true;
            RegisterTarget();
        }

        private void OnEnable()
        {
            // Servis Start'ta alınır; ilk kaydı Start yapar.
            if (_started)
            {
                RegisterTarget();
            }
        }

        private void OnDisable()
        {
            _registry?.Unregister(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPlayer>(this);
        }

        private void RegisterTarget()
        {
            _registry?.Register(this);
        }
    }
}
