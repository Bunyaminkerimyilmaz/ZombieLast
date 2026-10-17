using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Gün/gece ışığının üstüne binen geçici katman (saf): gece olayının sis ve ay/ambiyans rengi (GDD §14 Sis, Kan Ayı) ağırlıkla
    /// açılıp kapanır; şimşek (Fırtına) kısa süre ay ışığını parlatır. <see cref="DayNightLighting"/> taban değerleri hesaplar,
    /// <see cref="Apply"/> bu katmanı ekler.
    /// </summary>
    public sealed class LightingOverlay
    {
        private NightModifierSettings _modifier;
        private float _target;
        private float _fadeDuration;
        private float _flashElapsed = -1f;
        private float _flashDuration;
        private float _flashIntensity;
        private Color _flashColor;
        private Color _flashAmbient;

        /// <summary>Olay katmanının ağırlığı: 0 = yok, 1 = tam.</summary>
        public float Weight { get; private set; }

        public bool IsFlashing => _flashElapsed >= 0f;

        /// <summary>Şimşek gücü: tepede 1, süre sonunda 0 ((1 − t)² ile söner).</summary>
        public float FlashStrength
        {
            get
            {
                if (!IsFlashing)
                {
                    return 0f;
                }

                float remaining = 1f - Mathf.Clamp01(_flashElapsed / _flashDuration);
                return remaining * remaining;
            }
        }

        /// <summary>Katman tabandan farklı olabilir; her kare <see cref="Tick"/> + uygula.</summary>
        public bool IsActive => Weight > 0f || _target > 0f || IsFlashing;

        /// <summary>Olayın sis/ışık geçişini başlatır (<paramref name="fadeDuration"/> sn'de tam ağırlık). Önceki olayın yerine geçer.</summary>
        public void Begin(in NightModifierSettings modifier, float fadeDuration)
        {
            _modifier = modifier;
            _target = modifier.OverridesFog || modifier.OverridesLight ? 1f : 0f;
            _fadeDuration = Mathf.Max(0f, fadeDuration);
        }

        /// <summary>Olayın katmanını <paramref name="fadeDuration"/> sn'de söndürür.</summary>
        public void End(float fadeDuration)
        {
            _target = 0f;
            _fadeDuration = Mathf.Max(0f, fadeDuration);
        }

        /// <summary>Şimşek parlaması: tepede ay ışığına <paramref name="intensity"/> eklenir, renk ve ambiyans verilenlere kayar.</summary>
        public void Flash(float duration, float intensity, Color color, Color ambient)
        {
            _flashElapsed = 0f;
            _flashDuration = Mathf.Max(0.01f, duration);
            _flashIntensity = intensity;
            _flashColor = color;
            _flashAmbient = ambient;
        }

        /// <summary>Ağırlığı ve şimşeği ilerletir. Sonuç değiştiyse (yeniden uygulanmalı) true.</summary>
        public bool Tick(float deltaTime)
        {
            bool changed = false;
            if (Weight != _target)
            {
                Weight = _fadeDuration > 0f ? Mathf.MoveTowards(Weight, _target, deltaTime / _fadeDuration) : _target;
                changed = true;
            }

            if (IsFlashing)
            {
                _flashElapsed += deltaTime;
                if (_flashElapsed >= _flashDuration)
                {
                    _flashElapsed = -1f;
                }

                changed = true;
            }

            return changed;
        }

        /// <summary>Taban ışığa olay katmanını (ağırlıkla) ve şimşeği ekler.</summary>
        public LightingValues Apply(in LightingValues baseValues)
        {
            float weight = Weight;
            Color sunColor = baseValues.SunColor;
            Color ambient = baseValues.AmbientColor;
            float intensity = baseValues.SunIntensity;
            bool fogEnabled = baseValues.FogEnabled;
            Color fogColor = baseValues.FogColor;
            float fogDensity = baseValues.FogEnabled ? baseValues.FogDensity : 0f;

            if (weight > 0f && _modifier.OverridesLight)
            {
                sunColor = Color.Lerp(sunColor, _modifier.LightColor, weight);
                ambient = Color.Lerp(ambient, _modifier.AmbientColor, weight);
            }

            if (weight > 0f && _modifier.OverridesFog)
            {
                fogColor = fogEnabled ? Color.Lerp(fogColor, _modifier.FogColor, weight) : _modifier.FogColor;
                fogDensity = Mathf.Lerp(fogDensity, _modifier.FogDensity, weight);
                fogEnabled = fogDensity > 0f;
            }

            float flash = FlashStrength;
            if (flash > 0f)
            {
                sunColor = Color.Lerp(sunColor, _flashColor, flash);
                ambient = Color.Lerp(ambient, _flashAmbient, flash);
                intensity += _flashIntensity * flash;
            }

            return new LightingValues(baseValues.SunRotation, sunColor, intensity, ambient, fogEnabled, fogColor, fogDensity);
        }
    }
}
