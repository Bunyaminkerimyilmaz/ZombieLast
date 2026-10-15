using System.Globalization;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// [Systems]/Economy: oyuncunun kombosu (GDD §10). Saf <see cref="ComboTracker"/>'ı her kare ilerletir,
    /// <see cref="PlayerDamagedEvent"/>'te sıfırlar ve durumu <see cref="ComboChangedEvent"/> ile duyurur (kombo sürerken her kare).
    /// Öldürmeleri <see cref="CoinDropper"/> bildirir (<see cref="RegisterKill"/>): böylece jeton, öldürme anındaki çarpanı
    /// olay sırasından bağımsız alır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboService : MonoBehaviour
    {
        private const string LogPrefix = "[Economy] ";

        [SerializeField, Tooltip("Kombo penceresi ve eşikleri. Üretici bağlar.")]
        private EconomyConfig _config;

        private ComboTracker _tracker;

        /// <summary>Debug ve testler için; Awake'ten önce veya config yoksa null.</summary>
        public ComboTracker Tracker => _tracker;

        /// <summary>
        /// Öldürmeyi komboya işler ve o düşmanın jeton çarpanını döndürür. Sadece oyuncunun öldürmeleri sayılır ve çarpan alır;
        /// taret, tuzak ve zincir öldürmeleri kombo sayılmaz, çarpanları <see cref="ComboTracker.BaseMultiplier"/>.
        /// </summary>
        public float RegisterKill(bool killedByPlayer)
        {
            if (_tracker == null || !killedByPlayer)
            {
                return ComboTracker.BaseMultiplier;
            }

            float previous = _tracker.Multiplier;
            float multiplier = _tracker.RegisterKill();
            if (multiplier > previous)
            {
                Debug.Log($"{LogPrefix}Kombo ×{FormatMultiplier(multiplier)} ({_tracker.Count} öldürme).", this);
            }

            Raise();
            return multiplier;
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("ComboService: EconomyConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _tracker = _config.CreateComboTracker();
        }

        private void OnEnable()
        {
            EventBus<PlayerDamagedEvent>.Subscribe(OnPlayerDamaged);
        }

        private void OnDisable()
        {
            EventBus<PlayerDamagedEvent>.Unsubscribe(OnPlayerDamaged);
        }

        private void Update()
        {
            if (!_tracker.IsActive)
            {
                return;
            }

            int count = _tracker.Count;
            float multiplier = _tracker.Multiplier;
            if (_tracker.Tick(Time.deltaTime))
            {
                LogEnded(count, multiplier, "pencere doldu");
            }

            Raise();
        }

        private void OnPlayerDamaged(PlayerDamagedEvent damaged)
        {
            int count = _tracker.Count;
            float multiplier = _tracker.Multiplier;
            if (_tracker.Reset())
            {
                LogEnded(count, multiplier, "hasar alındı");
                Raise();
            }
        }

        private void Raise()
        {
            EventBus<ComboChangedEvent>.Raise(new ComboChangedEvent(_tracker.Count, _tracker.Multiplier, _tracker.WindowRatio));
        }

        private void LogEnded(int count, float multiplier, string reason)
        {
            // Sadece eşiğe ulaşmış kombolar loglanır (her tek öldürmede log olmasın).
            if (multiplier > ComboTracker.BaseMultiplier)
            {
                Debug.Log($"{LogPrefix}Kombo bitti ({reason}): {count} öldürme, ×{FormatMultiplier(multiplier)}.", this);
            }
        }

        // tr-TR'de ondalık ayırıcı virgül olmasın.
        private static string FormatMultiplier(float multiplier)
        {
            return multiplier.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
