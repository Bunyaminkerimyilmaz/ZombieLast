using LastHaven.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// [Gameplay]/Lighting: gündüz/gece ışığı (GDD §18). <see cref="PhaseChangedEvent"/>'i dinler; alacakaranlık boyunca gündüzden
    /// geceye, şafak başında geceden gündüze yumuşak geçer (<see cref="DayNightBlend"/>). Directional light, ambiyans ve sis
    /// iki <see cref="LightingProfile"/> arasında harmanlanır; gündüz ve gece Global Volume'lerinin weight'leri birlikte kayar.
    /// Üstüne gece olayının katmanı biner (<see cref="LightingOverlay"/>, GDD §14): <see cref="NightModifierActivatedEvent"/> sisi ve ay
    /// rengini yumuşakça değiştirir, <see cref="NightModifierEndedEvent"/> geri alır; Fırtına kesintisi başlarken
    /// (<see cref="PowerLostEvent"/>, <see cref="PowerLossReason.Outage"/>) şimşek çakar ve hafif <see cref="CameraShakeRequestedEvent"/>
    /// gider. Gündüzle başlar. Sadece geçiş veya olay katmanı değişirken çalışır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DayNightLighting : MonoBehaviour
    {
        [Header("Profiller")]
        [SerializeField, Tooltip("Gündüz ışığı. Üretici bağlar.")]
        private LightingProfile _dayProfile;

        [SerializeField, Tooltip("Gece ışığı. Üretici bağlar.")]
        private LightingProfile _nightProfile;

        [SerializeField, Tooltip("Şafak geçiş süresi buradan okunur; alacakaranlık geçişi evre süresini kullanır. Üretici bağlar.")]
        private GameConfig _config;

        [SerializeField, Tooltip("Gece olayının ışık geçişi ve Fırtına şimşeği. Üretici bağlar.")]
        private NightEventLighting _eventLighting;

        [Header("Sahne")]
        [SerializeField, Tooltip("Güneş/ay (directional light).")]
        private Light _sun;

        [SerializeField, Tooltip("Gündüz post-process'i (Global Volume). Weight = 1 − gece oranı.")]
        private Volume _dayVolume;

        [SerializeField, Tooltip("Gece post-process'i (Global Volume). Weight = gece oranı.")]
        private Volume _nightVolume;

        private readonly LightingOverlay _overlay = new LightingOverlay();
        private DayNightBlend _blend;

        /// <summary>Uygulanan gece oranı (yumuşatılmış): 0 = gündüz, 1 = gece.</summary>
        public float NightAmount => _blend != null ? _blend.Eased : 0f;

        /// <summary>Gece olayı katmanı (ağırlık, şimşek). Testler ve hata ayıklama için.</summary>
        public LightingOverlay Overlay => _overlay;

        /// <summary>Geçiş sürüyor mu.</summary>
        public bool IsTransitioning => _blend != null && _blend.IsTransitioning;

        private void Awake()
        {
            if (_dayProfile == null || _nightProfile == null || _config == null || _eventLighting == null)
            {
                Debug.LogError("DayNightLighting: ışık profilleri, GameConfig veya NightEventLighting atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _blend = new DayNightBlend(_config.DawnLightTransitionDuration);
            Apply();
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<NightModifierActivatedEvent>.Subscribe(OnModifierActivated);
            EventBus<NightModifierEndedEvent>.Subscribe(OnModifierEnded);
            EventBus<PowerLostEvent>.Subscribe(OnPowerLost);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<NightModifierActivatedEvent>.Unsubscribe(OnModifierActivated);
            EventBus<NightModifierEndedEvent>.Unsubscribe(OnModifierEnded);
            EventBus<PowerLostEvent>.Unsubscribe(OnPowerLost);
        }

        private void Update()
        {
            bool changed = _blend.Tick(Time.deltaTime);
            changed |= _overlay.IsActive && _overlay.Tick(Time.deltaTime);
            if (changed)
            {
                Apply();
            }
        }

        private void OnModifierActivated(NightModifierActivatedEvent activated)
        {
            if (_eventLighting != null)
            {
                _overlay.Begin(activated.Modifier, _eventLighting.FadeDuration);
            }
        }

        private void OnModifierEnded(NightModifierEndedEvent ended)
        {
            if (_eventLighting != null)
            {
                _overlay.End(_eventLighting.FadeDuration);
            }
        }

        // Fırtına: kesinti başlarken şimşek (GDD §14: şimşek flaşı + hafif sarsıntı).
        private void OnPowerLost(PowerLostEvent lost)
        {
            if (lost.Reason != PowerLossReason.Outage || _eventLighting == null)
            {
                return;
            }

            _overlay.Flash(_eventLighting.LightningDuration, _eventLighting.LightningIntensity, _eventLighting.LightningColor,
                _eventLighting.LightningAmbient);
            // Şimşek her yerde aynı hissedilir: konumsuz (genel) istek.
            EventBus<CameraShakeRequestedEvent>.Raise(new CameraShakeRequestedEvent(_eventLighting.LightningShake));
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (_blend != null && _blend.OnPhaseChanged(changed.Phase, changed.Duration))
            {
                Apply();
            }
        }

        private void Apply()
        {
            float night = _blend.Eased;
            LightingValues baseValues = LightingValues.Lerp(_dayProfile.Values, _nightProfile.Values, night);
            _overlay.Apply(baseValues).ApplyTo(_sun);

            if (_dayVolume != null)
            {
                _dayVolume.weight = 1f - night;
            }

            if (_nightVolume != null)
            {
                _nightVolume.weight = night;
            }
        }
    }
}
