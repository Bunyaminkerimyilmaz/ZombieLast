using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Boss eki (GDD §7 Brute): <see cref="Enemy"/>'nin yanında kökte durur. Doğunca <see cref="BossSpawnedEvent"/>, canı değişince
    /// <see cref="BossHealthChangedEvent"/>, ölünce <see cref="BossDefeatedEvent"/> gönderir (HUD boss barı). Can %50'nin altına
    /// düşünce öfkelenir (<see cref="BossRage"/>): hız ×1.4 (<see cref="SpeedSource.Boss"/>; yavaşlatmayla çarpılır) ve görsel
    /// koyulaşır (<see cref="HitFlash"/>'ın <see cref="TintLayer.Base"/> katmanı). Yere vurma saldırı stratejisinde
    /// (<see cref="GroundSlamAttack"/>), geri itilmezlik verisinde (<see cref="EnemyData.KnockbackResistance"/> = 1).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Enemy), typeof(Health))]
    public sealed class BossBehaviour : MonoBehaviour, IPoolable
    {
        private const string LogPrefix = "[Enemy] ";

        [SerializeField, Tooltip("Öfke eşiği, hızı ve rengi. Üretici bağlar.")]
        private BossConfig _config;

        private readonly BossRage _rage = new BossRage();
        private Enemy _enemy;
        private Health _health;
        private HitFlash _flash;
        private bool _announced;

        /// <summary>Bu doğuşta öfkelendi mi.</summary>
        public bool IsEnraged => _rage.IsEnraged;

        public void OnSpawned()
        {
            _rage.Reset();
            if (_flash != null)
            {
                _flash.ClearTint(TintLayer.Base);
            }

            Announce();
        }

        public void OnDespawned()
        {
            _announced = false;
        }

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _health = GetComponent<Health>();
            _flash = GetComponent<HitFlash>();
            if (_config == null)
            {
                Debug.LogError("BossBehaviour: BossConfig atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
                return;
            }

            // Havuzlanan obje: abonelik ömür boyu bir kez (OnEnable/OnDisable her doğuşta delegate ayırırdı). Health aynı objede.
            _health.Damaged += OnDamaged;
            _health.Healed += OnHealed;
            _health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Damaged -= OnDamaged;
                _health.Healed -= OnHealed;
                _health.Died -= OnDied;
            }
        }

        private void Start()
        {
            // Sahneye elle konmuş (havuzdan gelmeyen) boss da duyurulsun.
            Announce();
        }

        private void Announce()
        {
            EnemyData data = _enemy.Data;
            if (_announced || data == null)
            {
                return;
            }

            _announced = true;
            EventBus<BossSpawnedEvent>.Raise(new BossSpawnedEvent(gameObject, data.Id, data.DisplayName, data.MaxHealth));
        }

        private void OnDamaged(DamagedEvent damaged)
        {
            EventBus<BossHealthChangedEvent>.Raise(new BossHealthChangedEvent(gameObject, damaged.CurrentHealth, damaged.MaxHealth));
            if (_config != null && _rage.TryEnrage(_health.Normalized, _config.EnrageHealthFraction, _health.IsAlive))
            {
                Enrage();
            }
        }

        private void OnHealed(float amount)
        {
            EventBus<BossHealthChangedEvent>.Raise(new BossHealthChangedEvent(gameObject, _health.Current, _health.Max));
        }

        private void OnDied(DiedEvent died)
        {
            if (!_announced)
            {
                return;
            }

            _announced = false;
            GameObject source = died.KillingBlow.Source;
            bool killedByPlayer = source != null && source.CompareTag(GameTags.Player);
            EventBus<BossDefeatedEvent>.Raise(new BossDefeatedEvent(gameObject, _enemy.Data.Id, killedByPlayer));
        }

        private void Enrage()
        {
            _enemy.SetSpeedMultiplier(SpeedSource.Boss, _config.EnragedSpeedMultiplier);
            if (_flash != null)
            {
                _flash.SetTint(TintLayer.Base, _config.EnragedTint);
            }

            Debug.Log(FormattableString.Invariant(
                $"{LogPrefix}{_enemy.Data.DisplayName} öfkelendi: hız ×{_config.EnragedSpeedMultiplier:0.##} ({_enemy.MaxSpeed:0.##} m/sn)."), this);
        }
    }
}
