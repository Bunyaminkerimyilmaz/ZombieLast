using System;

namespace LastHaven.UI
{
    /// <summary>
    /// Uçan yazının ömrü boyunca hareketi (saf C#): yükselme (yavaşlayarak, 0..1), boy (ilk anda büyük, kısa sürede normale iner) ve
    /// opaklık (sona doğru söner). İlerleme 0 = yeni, 1 = bitti.
    /// </summary>
    public sealed class FloatingTextMotion
    {
        private readonly float _popFraction;
        private readonly float _popScale;
        private readonly float _fadeStart;

        public FloatingTextMotion(float popFraction, float popScale, float fadeStartFraction)
        {
            _popFraction = Clamp01(popFraction);
            _popScale = Math.Max(1f, popScale);
            _fadeStart = Clamp01(fadeStartFraction);
        }

        public FloatingTextPose Evaluate(float progress)
        {
            float t = Clamp01(progress);
            float remaining = 1f - t;
            float rise = 1f - remaining * remaining;

            float scale = t < _popFraction ? _popScale + (1f - _popScale) * (t / _popFraction) : 1f;
            float alpha = t <= _fadeStart ? 1f : _fadeStart < 1f ? 1f - (t - _fadeStart) / (1f - _fadeStart) : 0f;
            return new FloatingTextPose(rise, scale, alpha);
        }

        /// <summary>
        /// Hasara göre boy: <paramref name="smallDamage"/> ve altı <paramref name="smallScale"/>, <paramref name="bigDamage"/> ve üstü
        /// <paramref name="bigScale"/>, arada doğrusal.
        /// </summary>
        public static float DamageScale(float damage, float smallDamage, float bigDamage, float smallScale, float bigScale)
        {
            if (bigDamage <= smallDamage)
            {
                return damage >= bigDamage ? bigScale : smallScale;
            }

            if (damage <= smallDamage)
            {
                return smallScale;
            }

            if (damage >= bigDamage)
            {
                return bigScale;
            }

            return smallScale + (bigScale - smallScale) * ((damage - smallDamage) / (bigDamage - smallDamage));
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }

    /// <summary>Uçan yazının bir anki durumu.</summary>
    public readonly struct FloatingTextPose
    {
        public FloatingTextPose(float rise, float scale, float alpha)
        {
            Rise = rise;
            Scale = scale;
            Alpha = alpha;
        }

        /// <summary>Yükselme oranı (0..1; yükseklikle çarpılır).</summary>
        public float Rise { get; }

        /// <summary>Boy katsayısı (vuruş büyümesi; hasar boyuyla çarpılır).</summary>
        public float Scale { get; }

        /// <summary>Opaklık (0..1).</summary>
        public float Alpha { get; }
    }
}
