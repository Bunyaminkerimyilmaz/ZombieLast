using System;

namespace LastHaven.Core
{
    /// <summary>
    /// Saf C# can modeli. Ölüyken hasar almaz ve iyileşmez; iyileşme <see cref="Max"/>'ı aşmaz.
    /// </summary>
    public sealed class HealthModel
    {
        /// <summary>Hasar uygulandığında, gerçekte düşen can miktarıyla çağrılır.</summary>
        public event Action<float> Damaged;

        /// <summary>İyileşme uygulandığında, gerçekte artan can miktarıyla çağrılır.</summary>
        public event Action<float> Healed;

        /// <summary>Can sıfıra indiğinde bir kez çağrılır.</summary>
        public event Action Died;

        public float Max { get; private set; }

        public float Current { get; private set; }

        public bool IsAlive => Current > 0f;

        /// <summary>Current / Max, 0..1 arası.</summary>
        public float Normalized => Current / Max;

        public HealthModel(float max)
        {
            ValidateMax(max);
            Max = max;
            Current = max;
        }

        /// <summary>Hasar uygular ve gerçekte düşen canı döndürür. Ölüyken veya miktar pozitif değilse 0 döner.</summary>
        public float TakeDamage(float amount)
        {
            if (!IsAlive || !(amount > 0f))
            {
                return 0f;
            }

            float applied = Math.Min(amount, Current);
            Current -= applied;
            Damaged?.Invoke(applied);

            if (Current <= 0f)
            {
                Current = 0f;
                Died?.Invoke();
            }

            return applied;
        }

        /// <summary>İyileştirir ve gerçekte artan canı döndürür. Ölüyken veya canı doluyken 0 döner.</summary>
        public float Heal(float amount)
        {
            if (!IsAlive || !(amount > 0f))
            {
                return 0f;
            }

            float applied = Math.Min(amount, Max - Current);
            if (applied <= 0f)
            {
                return 0f;
            }

            Current += applied;
            Healed?.Invoke(applied);
            return applied;
        }

        /// <summary>
        /// Maksimum canı değiştirir. Mevcut can yeni maksimumu aşamaz.
        /// <paramref name="refill"/> true ise can tamamen dolar (ölüyse diriltir; havuz ve yeniden doğma için).
        /// </summary>
        public void SetMax(float max, bool refill = false)
        {
            ValidateMax(max);
            Max = max;
            Current = refill ? max : Math.Min(Current, max);
        }

        private static void ValidateMax(float max)
        {
            if (!(max > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, "Maksimum can 0'dan büyük olmalı.");
            }
        }
    }
}
