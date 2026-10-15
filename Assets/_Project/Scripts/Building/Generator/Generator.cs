using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Üssün jeneratörü (GDD §12). Canı <see cref="GeneratorData"/>'dan kurar, kendini <see cref="ITargetRegistry"/>'ye
    /// <see cref="TargetType.Generator"/> olarak ekler (düşmanların varsayılan hedefi) ve <see cref="IPowerService"/> olarak kaydolur.
    /// Yıkılınca <see cref="GeneratorDestroyedEvent"/> gönderir.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class Generator : MonoBehaviour, ITargetable, IPowerService
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

        /// <summary>Yakıt tüketimi Gün 9'da gelecek; o zamana kadar güç hep var.</summary>
        public bool IsPowered => true;

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

            ServiceLocator.Register<IPowerService>(this);
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

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPowerService>(this);
        }

        private void OnDied(DiedEvent died)
        {
            EventBus<GeneratorDestroyedEvent>.Raise(new GeneratorDestroyedEvent(gameObject, transform.position, died.KillingBlow));

            GameObject source = died.KillingBlow.Source;
            Debug.Log($"[Base] GeneratorDestroyedEvent: jeneratör yıkıldı (son vuran: {(source != null ? source.name : "bilinmiyor")}).", this);
        }
    }
}
