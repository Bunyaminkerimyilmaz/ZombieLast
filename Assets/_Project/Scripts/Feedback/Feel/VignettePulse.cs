using System;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Ekran kenarındaki kırmızı vignette'in ağırlığı (saf C#; <see cref="ScreenVignette"/>'in mantığı). İki kaynak, büyüğü geçerli:
    /// hasar flaşı (<see cref="Flash"/>: anında tepe, süre boyunca doğrusal söner) ve düşük can nabzı (oyuncu canlı ve can oranı
    /// eşiğin altındayken en az..en çok arasında kosinüs nabzı; nabız her başladığında en azdan başlar).
    /// </summary>
    public sealed class VignettePulse
    {
        private const float TwoPi = (float)(2.0 * Math.PI);

        private readonly VignetteSettings _settings;
        private float _flashRemaining;
        private float _pulseTime;

        public VignettePulse(VignetteSettings settings)
        {
            _settings = settings;
        }

        /// <summary>Son <see cref="Tick"/>'in sonucu (0..1).</summary>
        public float Weight { get; private set; }

        /// <summary>Hasar flaşını tepeden başlatır.</summary>
        public void Flash()
        {
            _flashRemaining = _settings.FlashDuration;
        }

        /// <summary>
        /// İlerletir ve ağırlığı döndürür. <paramref name="healthRatio"/>: can / maks. can (0..1).
        /// </summary>
        public float Tick(float deltaTime, float healthRatio, bool alive)
        {
            float flash = 0f;
            if (_flashRemaining > 0f)
            {
                flash = _settings.FlashDuration > 0f ? _settings.FlashWeight * (_flashRemaining / _settings.FlashDuration) : 0f;
                _flashRemaining = Math.Max(0f, _flashRemaining - deltaTime);
            }

            float pulse = 0f;
            if (alive && healthRatio < _settings.LowHealthThreshold)
            {
                pulse = EvaluatePulse(_pulseTime);
                _pulseTime += deltaTime;
            }
            else
            {
                _pulseTime = 0f;
            }

            Weight = Clamp01(Math.Max(flash, pulse));
            return Weight;
        }

        private float EvaluatePulse(float time)
        {
            float period = _settings.PulsePeriod;
            float wave = period > 0f ? 0.5f - 0.5f * (float)Math.Cos(TwoPi * time / period) : 1f;
            return _settings.PulseMinWeight + (_settings.PulseMaxWeight - _settings.PulseMinWeight) * wave;
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }

    /// <summary><see cref="VignettePulse"/> ayarları (<see cref="FeedbackConfig"/>'ten).</summary>
    public readonly struct VignetteSettings
    {
        public VignetteSettings(
            float flashWeight, float flashDuration, float lowHealthThreshold, float pulsePeriod, float pulseMinWeight, float pulseMaxWeight)
        {
            FlashWeight = flashWeight;
            FlashDuration = flashDuration;
            LowHealthThreshold = lowHealthThreshold;
            PulsePeriod = pulsePeriod;
            PulseMinWeight = pulseMinWeight;
            PulseMaxWeight = pulseMaxWeight;
        }

        /// <summary>Flaşın tepe ağırlığı (0..1).</summary>
        public float FlashWeight { get; }

        /// <summary>Flaşın sönme süresi (sn).</summary>
        public float FlashDuration { get; }

        /// <summary>Can oranı bunun altındayken nabız atar (0.3 = %30).</summary>
        public float LowHealthThreshold { get; }

        /// <summary>Bir nabzın süresi (sn).</summary>
        public float PulsePeriod { get; }

        public float PulseMinWeight { get; }

        public float PulseMaxWeight { get; }
    }
}
