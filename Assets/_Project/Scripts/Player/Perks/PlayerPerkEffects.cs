using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncu tarafındaki perk etkileri (GDD §13), oyuncu kökünde: maks. can (<see cref="IRunModifiers"/> PlayerMaxHealth; Kalın
    /// Deri +25 — iyileşme perk'in kendi etkisiyle <see cref="HealRequestedEvent"/>'ten gelir) ve Vampir (oyuncunun her öldürmesinde
    /// <see cref="PlayerData.VampireHealPerKill"/> can). Maks. can artınca mevcut can değişmez, azalırsa kırpılır; yeniden doğuş
    /// yeni maksimumla dolar. Diğer oyuncu perk'leri: Adrenalin <see cref="PlayerMotor"/>, Mıknatıs <see cref="PlayerCharacter"/>,
    /// silah statları <see cref="WeaponController"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(PlayerCharacter))]
    public sealed class PlayerPerkEffects : MonoBehaviour
    {
        private const string LogPrefix = "[Player] ";

        [SerializeField, Tooltip("Oyuncu ayarları: taban maks. can ve Vampir iyileşmesi (GDD §5, §13). Üretici bağlar.")]
        private PlayerData _data;

        private Health _health;
        private IRunModifiers _modifiers;
        private bool _started;

        // Health.SetMax pozitif ister; negatif modifier'lar canı buraya kırpar.
        private const float MinMaxHealth = 1f;

        /// <summary>Maks. can: taban + düz eklemeler (Kalın Deri), sonra yüzdeler; en az 1. Çarpan yoksa taban.</summary>
        public static float CalculateMaxHealth(float baseMax, IRunModifiers modifiers)
        {
            float max = modifiers != null ? modifiers.Get(StatType.PlayerMaxHealth, baseMax) : baseMax;
            return Math.Max(MinMaxHealth, max);
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (_data == null)
            {
                Debug.LogError("PlayerPerkEffects: PlayerData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            // Yoksa perk etkisi yok (WeaponController zaten uyarır).
            ServiceLocator.TryGet(out _modifiers);
            _started = true;
            Connect();
        }

        private void OnEnable()
        {
            EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);

            // Servis Start'ta alınır; ilk bağlantıyı Start yapar.
            if (_started)
            {
                Connect();
            }
        }

        private void OnDisable()
        {
            EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
            if (_modifiers != null)
            {
                _modifiers.StatChanged -= OnStatChanged;
            }
        }

        private void Connect()
        {
            if (_modifiers == null)
            {
                return;
            }

            _modifiers.StatChanged -= OnStatChanged;
            _modifiers.StatChanged += OnStatChanged;
            RefreshMaxHealth();
        }

        private void OnStatChanged(StatType stat)
        {
            if (stat == StatType.PlayerMaxHealth)
            {
                RefreshMaxHealth();
            }
        }

        private void RefreshMaxHealth()
        {
            float max = CalculateMaxHealth(_data.MaxHealth, _modifiers);
            if (Mathf.Approximately(max, _health.Max))
            {
                return;
            }

            _health.SetMax(max);
            Debug.Log($"{LogPrefix}Maks. can: {_health.Max:0.#} (can {_health.Current:0.#}).", this);
        }

        private void OnEnemyKilled(EnemyKilledEvent killed)
        {
            if (killed.KilledByPlayer && _modifiers != null && _modifiers.HasFlag(PerkFlag.Vampire) && _health.IsAlive)
            {
                _health.Heal(_data.VampireHealPerKill);
            }
        }
    }
}
