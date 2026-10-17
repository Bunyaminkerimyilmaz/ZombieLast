using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın durum efektleri (<see cref="IStatusEffectReceiver"/>; 9.3). Yavaşlatma: en güçlüsü geçerli (<see cref="SlowEffects"/>),
    /// <see cref="Enemy"/>'nin hız çarpanıyla <c>NavMeshAgent.speed</c>'e uygulanır (taban hız × (1 − oran)); yavaşlamışken
    /// Visual'a hafif mavi ton (<see cref="HitFlash.SetTint"/>; HitFlash yoksa ton atlanır). Ölünce ve havuza dönünce temizlenir.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Enemy), typeof(Health))]
    public sealed class EnemyStatusEffects : MonoBehaviour, IStatusEffectReceiver, IPoolable
    {
        [Header("Yavaşlatma")]
        [SerializeField, Tooltip("Yavaşlamışken Visual'ın rengi buna karışır: RGB ton, alfa karışım oranı (0 = yok, 1 = tamamen bu renk).")]
        private Color _slowTint = new Color(0.35f, 0.6f, 1f, 0.45f);

        private readonly SlowEffects _slows = new SlowEffects();
        private Enemy _enemy;
        private Health _health;
        private HitFlash _flash;
        private float _appliedSlow;

        public bool IsSlowed => _slows.IsSlowed;

        /// <summary>Geçerli (en güçlü) yavaşlatma oranı; yoksa 0.</summary>
        public float SlowFraction => _slows.Current;

        public Color SlowTint => _slowTint;

        public void ApplySlow(float fraction, float duration)
        {
            EnsureInitialized();
            if (!_health.IsAlive)
            {
                return;
            }

            _slows.Apply(fraction, duration);
            Refresh();
        }

        public void OnSpawned()
        {
            // Hız çarpanını Enemy her doğuşta 1'e kurar; burada sadece durum ve ton sıfırlanır.
            EnsureInitialized();
            _slows.Clear();
            _appliedSlow = 0f;
            if (_flash != null)
            {
                _flash.ClearTint();
            }
        }

        public void OnDespawned()
        {
            ClearEffects();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
            ClearEffects();
        }

        private void Update()
        {
            if (_slows.Count == 0)
            {
                return;
            }

            _slows.Tick(Time.deltaTime);
            Refresh();
        }

        // Geçerli oran değişince hızı ve (yavaşlama başlayıp bitince) tonu günceller.
        private void Refresh()
        {
            float slow = _slows.Current;
            if (Mathf.Approximately(slow, _appliedSlow))
            {
                return;
            }

            bool wasSlowed = _appliedSlow > 0f;
            _appliedSlow = slow;
            _enemy.SetSpeedMultiplier(SpeedSource.Status, 1f - slow);
            if (_flash == null || wasSlowed == slow > 0f)
            {
                return;
            }

            if (slow > 0f)
            {
                _flash.SetTint(_slowTint);
            }
            else
            {
                _flash.ClearTint();
            }
        }

        private void OnDied(DiedEvent died)
        {
            ClearEffects();
        }

        private void ClearEffects()
        {
            EnsureInitialized();
            _slows.Clear();
            Refresh();
        }

        // Tembel kurulum: EditMode testlerinde (Awake yokken) de çalışır.
        private void EnsureInitialized()
        {
            if (_health != null)
            {
                return;
            }

            _enemy = GetComponent<Enemy>();
            _health = GetComponent<Health>();
            _flash = GetComponent<HitFlash>();
        }
    }
}
