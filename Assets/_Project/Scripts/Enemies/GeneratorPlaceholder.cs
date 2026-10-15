using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// GEÇİCİ jeneratör: düşmanların varsayılan hedefi (GDD §7 kural 3) olsun diye üs merkezinde durur.
    /// <see cref="Health"/> + <see cref="ITargetable"/> (<see cref="TargetType.Generator"/>). Gün 5'te gerçek Generator gelince silinecek.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class GeneratorPlaceholder : MonoBehaviour, ITargetable
    {
        [SerializeField, Min(1f), Tooltip("Jeneratör canı (GDD §12).")]
        private float _maxHealth = 1500f;

        private Health _health;
        private ITargetRegistry _registry;
        private bool _started;

        public Transform Transform => transform;

        public TargetType TargetType => TargetType.Generator;

        public bool IsTargetable => isActiveAndEnabled && _health.IsAlive;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _health.SetMax(_maxHealth, refill: true);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("GeneratorPlaceholder: ITargetRegistry bulunamadı; düşmanlar jeneratörü hedefleyemeyecek.", this);
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
            Debug.Log($"[GeneratorPlaceholder] Jeneratör yıkıldı (son vuran: {(died.KillingBlow.Source != null ? died.KillingBlow.Source.name : "bilinmiyor")}).", this);
        }
    }
}
