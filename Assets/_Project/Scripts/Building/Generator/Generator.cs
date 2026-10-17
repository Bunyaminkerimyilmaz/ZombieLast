using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Üssün jeneratörü (GDD §12). Canı <see cref="GeneratorData"/>'dan kurar, kendini <see cref="ITargetRegistry"/>'ye
    /// <see cref="TargetType.Generator"/> olarak ekler (düşmanların varsayılan hedefi). Yıkılınca <see cref="GeneratorDestroyedEvent"/>
    /// gönderir. Yakıt ve güç aynı objedeki <see cref="GeneratorPower"/>'da, acil yakıt <see cref="EmergencyFuelStation"/>'da.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class Generator : MonoBehaviour, ITargetable
    {
        [SerializeField, Tooltip("Jeneratör ayarları (GDD §12).")]
        private GeneratorData _data;

        private Health _health;
        private ITargetRegistry _registry;
        private bool _started;

        public GeneratorData Data => _data;

        public Transform Transform => transform;

        public TargetType TargetType => TargetType.Generator;

        public bool IsTargetable => isActiveAndEnabled && _health.IsAlive;

        public Health Health => _health != null ? _health : _health = GetComponent<Health>();

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (_data != null)
            {
                _health.SetMax(_data.MaxHealth, refill: true);
            }
            else
            {
                Debug.LogError("Generator: GeneratorData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("Generator: ITargetRegistry bulunamadı; düşmanlar jeneratörü hedefleyemeyecek. Sahnede [Systems]/TargetRegistry var mı?", this);
            }

            _started = true;
            _registry?.Register(this);
        }

        private void OnEnable()
        {
            _health.Died += OnDied;

            // Servis Start'ta alınır; ilk kaydı Start yapar.
            if (_started)
            {
                _registry?.Register(this);
            }
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
            _registry?.Unregister(this);
        }

        private void OnDied(DiedEvent died)
        {
            EventBus<GeneratorDestroyedEvent>.Raise(new GeneratorDestroyedEvent(gameObject, transform.position, died.KillingBlow));

            GameObject source = died.KillingBlow.Source;
            Debug.Log($"[Base] GeneratorDestroyedEvent: jeneratör yıkıldı (son vuran: {(source != null ? source.name : "bilinmiyor")}).", this);
        }
    }
}
