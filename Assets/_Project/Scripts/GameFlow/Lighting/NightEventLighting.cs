using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Gece olaylarının (GDD §14) ışık ayarları: olayın sis/ay rengi geçiş süresi ve Fırtına şimşeği (güç kesintisi başlarken
    /// directional light anlık parlar + hafif kamera sarsıntısı isteği). Olayın kendi renkleri olay verisinde. Asset'i üretici oluşturur:
    /// <c>Data/Lighting/NightEventLighting.asset</c>; <see cref="DayNightLighting"/> bağlıdır. Değerler GDD'de yok.
    /// </summary>
    public sealed class NightEventLighting : ScriptableObject
    {
        [Header("Olay ışığı")]
        [SerializeField, Min(0f), Tooltip("Olayın sis ve ay rengine geçiş (ve geri dönüş) süresi (sn). 0 = anında.")]
        private float _fadeDuration = 3f;

        [Header("Şimşek (Fırtına)")]
        [SerializeField, Min(0.01f), Tooltip("Şimşek parlamasının süresi (sn); başta en parlak, hızla söner.")]
        private float _lightningDuration = 0.3f;

        [SerializeField, Min(0f), Tooltip("Parlamanın tepesinde ay ışığına eklenen yoğunluk.")]
        private float _lightningIntensity = 3f;

        [SerializeField, Tooltip("Parlamanın tepesinde ay ışığının rengi.")]
        private Color _lightningColor = new Color(0.85f, 0.9f, 1f);

        [SerializeField, Tooltip("Parlamanın tepesinde ortam ışığı (gölgeler de aydınlansın).")]
        private Color _lightningAmbient = new Color(0.55f, 0.6f, 0.75f);

        [SerializeField, Min(0f), Tooltip("Şimşekle istenen kamera sarsıntısı (CameraShakeRequestedEvent). Hafif.")]
        private float _lightningShake = 0.25f;

        public float FadeDuration => _fadeDuration;

        public float LightningDuration => _lightningDuration;

        public float LightningIntensity => _lightningIntensity;

        public Color LightningColor => _lightningColor;

        public Color LightningAmbient => _lightningAmbient;

        public float LightningShake => _lightningShake;
    }
}
