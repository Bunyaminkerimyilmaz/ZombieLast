using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncu prefab'ının kök bileşeni: canı PlayerData'dan kurar, <see cref="IPlayer"/> servisi olarak kaydolur ve
    /// kendini <see cref="ITargetRegistry"/>'ye <see cref="TargetType.Player"/> olarak ekler. Atılma sırasında ve hasar aldıktan
    /// sonraki kısa pencerede (GDD §5: 0.3 sn) hasarı engeller. Hasarda kırmızı flaş (<see cref="HitFlash"/>) ve
    /// <see cref="PlayerDamagedEvent"/>, ölümde <see cref="PlayerDiedEvent"/> gönderir.
    /// </summary>
    /// <remarks>Adı "Player" değil: <c>LastHaven.Player</c> namespace'iyle çakışırdı.</remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(PlayerMotor), typeof(PlayerAim))]
    public sealed class PlayerCharacter : MonoBehaviour, IPlayer, ITargetable, IDamageFilter
    {
        [SerializeField, Tooltip("Oyuncu ayarları (GDD §5).")]
        private PlayerData _data;

        private readonly HitInvulnerability _hitInvulnerability = new HitInvulnerability();
        private Health _health;
        private PlayerMotor _motor;
        private PlayerAim _aim;
        private HitFlash _flash;
        private ITargetRegistry _registry;
        private IRunModifiers _modifiers;
        private float _pickupRadius;
        private bool _started;

        public Transform Transform => transform;

        public Vector3 Position => transform.position;

        public Vector3 AimPoint => _aim.AimPoint;

        public Vector3 AimDirection => _aim.AimDirection;

        public bool IsAlive => _health.IsAlive;

        /// <summary>PlayerData × <see cref="IRunModifiers"/> PickupRadius; stat değişince yeniden hesaplanır.</summary>
        public float PickupRadius => _pickupRadius;

        public TargetType TargetType => TargetType.Player;

        public bool IsTargetable => isActiveAndEnabled && _health.IsAlive;

        /// <summary>Atılıyor veya hasar sonrası dokunulmazlık penceresinde.</summary>
        public bool IsInvulnerable => _motor.IsInvulnerable || _hitInvulnerability.IsActive(Time.time);

        public bool CanTakeDamage(in DamageInfo damage)
        {
            return !IsInvulnerable;
        }

        /// <summary>
        /// Mıknatıs yarıçapı: taban (PlayerData) + düz eklemeler (market +1 m), sonra yüzdeler (Mıknatıs perk'i +%50).
        /// <paramref name="modifiers"/> null ise taban.
        /// </summary>
        public static float CalculatePickupRadius(float baseRadius, IRunModifiers modifiers)
        {
            return modifiers != null ? modifiers.Get(StatType.PickupRadius, baseRadius) : baseRadius;
        }

        /// <summary>
        /// Yeniden doğma: canı o anki maksimumla doldurur (ölüyse diriltir) ve hasar sonrası dokunulmazlık penceresini kapatır.
        /// Konum ve poz <see cref="PlayerRespawn"/>'da.
        /// </summary>
        public void Revive()
        {
            _hitInvulnerability.Reset();
            _health.SetMax(_health.Max, refill: true);
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _motor = GetComponent<PlayerMotor>();
            _aim = GetComponent<PlayerAim>();
            _flash = GetComponent<HitFlash>();

            if (_data != null)
            {
                _health.SetMax(_data.MaxHealth, refill: true);
            }
            else
            {
                Debug.LogError("PlayerCharacter: PlayerData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
            }

            _health.DamageFilter = this;
            RefreshPickupRadius();
            ServiceLocator.Register<IPlayer>(this);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("PlayerCharacter: ITargetRegistry bulunamadı; düşmanlar oyuncuyu hedefleyemeyecek. Sahnede [Systems]/TargetRegistry var mı?", this);
            }

            // Yoksa yarıçap sadece PlayerData'dan (WeaponController zaten uyarır).
            ServiceLocator.TryGet(out _modifiers);

            _started = true;
            Connect();
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;

            // Servisler Start'ta alınır; ilk bağlantıyı Start yapar.
            if (_started)
            {
                Connect();
            }
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
            _registry?.Unregister(this);
            if (_modifiers != null)
            {
                _modifiers.StatChanged -= OnStatChanged;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPlayer>(this);
        }

        private void Connect()
        {
            _registry?.Register(this);
            if (_modifiers != null)
            {
                _modifiers.StatChanged += OnStatChanged;
            }

            RefreshPickupRadius();
        }

        private void OnStatChanged(StatType stat)
        {
            if (stat == StatType.PickupRadius)
            {
                RefreshPickupRadius();
            }
        }

        private void RefreshPickupRadius()
        {
            _pickupRadius = CalculatePickupRadius(_data != null ? _data.PickupRadius : 0f, _modifiers);
        }

        private void OnDamaged(DamagedEvent damaged)
        {
            if (_data != null)
            {
                _hitInvulnerability.Trigger(Time.time, _data.HitInvulnerabilityDuration);
            }

            if (_flash != null)
            {
                _flash.Flash();
            }

            EventBus<PlayerDamagedEvent>.Raise(new PlayerDamagedEvent(
                gameObject, damaged.Damage, damaged.AppliedAmount, damaged.CurrentHealth, damaged.MaxHealth));
        }

        private void OnDied(DiedEvent died)
        {
            EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent(gameObject, transform.position, died.KillingBlow));
        }
    }
}
