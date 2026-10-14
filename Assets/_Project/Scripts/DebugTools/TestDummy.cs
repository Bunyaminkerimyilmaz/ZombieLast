using LastHaven.Core;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Silah testi kuklası: hasarı ve ölümü loglar; ölünce collider'ı kapanır ve görseli gizlenir,
    /// <c>_respawnDelay</c> sn sonra dolu canla geri gelir. Enemy layer'ındadır (isabet "düşman" sayılır) ama düşman değildir.
    /// Sahneye <c>TestDummiesSceneStep</c> kurar.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class TestDummy : MonoBehaviour
    {
        [SerializeField, Min(1f), Tooltip("Kuklanın canı; her geri gelişte dolar.")]
        private float _maxHealth = 100f;

        [SerializeField, Min(0f), Tooltip("Ölümden sonra geri gelme süresi (sn).")]
        private float _respawnDelay = 3f;

        [SerializeField, Tooltip("Ölüyken gizlenen görsel kök. Boşsa gizlenmez.")]
        private GameObject _visual;

        [SerializeField, Tooltip("Her hasarı konsola yaz.")]
        private bool _logDamage = true;

        private readonly Countdown _respawn = new Countdown();
        private Health _health;
        private Collider _collider;

        public bool IsRespawning => _respawn.IsRunning;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _collider = GetComponent<Collider>();
            _health.SetMax(_maxHealth, refill: true);
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
        }

        private void Update()
        {
            if (_respawn.Tick(Time.deltaTime))
            {
                Respawn();
            }
        }

        private void OnDamaged(DamagedEvent damaged)
        {
            if (_logDamage)
            {
                Debug.Log($"[Dummy] {name}: -{damaged.AppliedAmount:0.#} → {damaged.CurrentHealth:0.#}/{damaged.MaxHealth:0.#}", this);
            }
        }

        private void OnDied(DiedEvent died)
        {
            Debug.Log($"[Dummy] {name} öldü; {_respawnDelay:0.#} sn sonra geri gelecek.", this);
            SetPresent(false);
            _respawn.Start(_respawnDelay);
        }

        private void Respawn()
        {
            _health.SetMax(_maxHealth, refill: true);
            SetPresent(true);
            Debug.Log($"[Dummy] {name} geri geldi ({_maxHealth:0.#} can).", this);
        }

        private void SetPresent(bool present)
        {
            if (_collider != null)
            {
                _collider.enabled = present;
            }

            if (_visual != null)
            {
                _visual.SetActive(present);
            }
        }
    }
}
