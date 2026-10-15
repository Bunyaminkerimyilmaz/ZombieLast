using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Gündüz/gece ışık geçişinin ilerlemesi (saf C#): 0 = gündüz, 1 = gece. Evre değişince hedef alır (<see cref="PhaseLighting"/>),
    /// <see cref="Tick"/> ile hedefe doğrusal ilerler. Süre tam geçişin (0↔1) süresidir; yarım kalan geçişten dönüş orantılı kısalır.
    /// <list type="bullet">
    /// <item>Dusk: geceye, evre süresi boyunca (GDD §3: 4 sn).</item>
    /// <item>Night: gece; bitmemiş geçiş hemen tamamlanır.</item>
    /// <item>Day, Dawn, Victory: gündüze, şafak geçiş süresiyle.</item>
    /// <item>Boot, GameOver: değişmez (sürmekte olan geçiş devam eder).</item>
    /// </list>
    /// </summary>
    public sealed class DayNightBlend
    {
        private const float Day = 0f;
        private const float Night = 1f;

        private readonly float _dawnTransitionDuration;
        private float _speed;

        /// <param name="dawnTransitionDuration">Geceden gündüze geçiş süresi (sn). 0 = anında.</param>
        public DayNightBlend(float dawnTransitionDuration)
        {
            _dawnTransitionDuration = Mathf.Max(0f, dawnTransitionDuration);
        }

        /// <summary>Doğrusal ilerleme: 0 = gündüz, 1 = gece. Gündüzle başlar.</summary>
        public float Value { get; private set; }

        /// <summary>Hedef: 0 = gündüz, 1 = gece.</summary>
        public float Target { get; private set; }

        public bool IsTransitioning => Value != Target;

        /// <summary>Yumuşatılmış ilerleme (smoothstep): geçiş yavaş başlar, yavaş biter. Işık bununla harmanlanır.</summary>
        public float Eased => Value * Value * (3f - 2f * Value);

        /// <summary>Evre değişimini işler.</summary>
        /// <param name="phaseDuration">Evrenin süresi (<see cref="PhaseChangedEvent.Duration"/>); sadece Dusk'ta kullanılır.</param>
        /// <returns>Evre ışığı etkiliyorsa true (değer bu çağrıda anında değişmiş olabilir; çağıran ışığı yeniden uygulamalı).</returns>
        public bool OnPhaseChanged(GamePhase phase, float phaseDuration)
        {
            switch (PhaseLighting.For(phase))
            {
                case LightingTarget.Night:
                    SetTarget(Night, phase == GamePhase.Dusk ? phaseDuration : 0f);
                    return true;
                case LightingTarget.Day:
                    SetTarget(Day, _dawnTransitionDuration);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Hedefe ilerler.</summary>
        /// <returns>Değer değiştiyse true.</returns>
        public bool Tick(float deltaTime)
        {
            if (!IsTransitioning || deltaTime <= 0f)
            {
                return false;
            }

            Value = Mathf.MoveTowards(Value, Target, _speed * deltaTime);
            return true;
        }

        private void SetTarget(float target, float fullDuration)
        {
            Target = target;
            if (fullDuration <= 0f)
            {
                Value = target;
                _speed = 0f;
                return;
            }

            _speed = 1f / fullDuration;
        }
    }
}
