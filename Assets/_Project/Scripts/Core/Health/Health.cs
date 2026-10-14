using System;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="HealthModel"/>'i saran Unity köprüsü. Hasarda <see cref="DamagedEvent"/>,
    /// ölümde <see cref="DiedEvent"/> gönderir; aynı olayları kendi sahibine C# event'i olarak da verir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [Header("Can")]
        [SerializeField, Min(1f), Tooltip("Başlangıç maksimum canı. Sahibi genelde kendi data'sından SetMax ile ayarlar.")]
        private float _maxHealth = 100f;

        private HealthModel _model;

        /// <summary>Bu nesne hasar aldığında (global olaydan önce) çağrılır.</summary>
        public event Action<DamagedEvent> Damaged;

        /// <summary>Bu nesne iyileştiğinde, gerçekte artan canla çağrılır.</summary>
        public event Action<float> Healed;

        /// <summary>Bu nesne öldüğünde (global olaydan önce) çağrılır.</summary>
        public event Action<DiedEvent> Died;

        public float Current => Model.Current;

        public float Max => Model.Max;

        /// <summary>Current / Max, 0..1 arası.</summary>
        public float Normalized => Model.Normalized;

        public bool IsAlive => Model.IsAlive;

        // Tembel kurulum: başka bir bileşen Awake'te SetMax çağırsa da çalışır.
        private HealthModel Model => _model ??= new HealthModel(_maxHealth);

        public void TakeDamage(DamageInfo damage)
        {
            float applied = Model.TakeDamage(damage.Amount);
            if (applied <= 0f)
            {
                return;
            }

            var damagedEvent = new DamagedEvent(gameObject, damage, applied, Model.Current, Model.Max);
            Damaged?.Invoke(damagedEvent);
            EventBus<DamagedEvent>.Raise(damagedEvent);

            if (Model.IsAlive)
            {
                return;
            }

            var diedEvent = new DiedEvent(gameObject, damage);
            Died?.Invoke(diedEvent);
            EventBus<DiedEvent>.Raise(diedEvent);
        }

        /// <summary>İyileştirir ve gerçekte artan canı döndürür. Ölüyken iyileşmez.</summary>
        public float Heal(float amount)
        {
            float applied = Model.Heal(amount);
            if (applied > 0f)
            {
                Healed?.Invoke(applied);
            }

            return applied;
        }

        /// <summary>
        /// Maksimum canı değiştirir. <paramref name="refill"/> true ise can dolar (ölüyse diriltir).
        /// </summary>
        public void SetMax(float max, bool refill = false)
        {
            Model.SetMax(max, refill);
            _maxHealth = max;
        }
    }
}
