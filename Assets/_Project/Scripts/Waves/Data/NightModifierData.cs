using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Gece olayı (GDD §14: Kan Ayı, Sis, Fırtına). Asset'leri üretici oluşturur: <c>Data/Waves/Modifiers/</c>. Gece başında
    /// <see cref="ToSettings"/> ile <see cref="NightModifierActivatedEvent"/>'e taşınır (<see cref="NightModifierController"/>).
    /// Düşman adedi <see cref="WavePlan"/>'da, hızı doğuşta uygulanır; sis/ışık, jeton ve güç kesintisini dinleyiciler uygular.
    /// Çarpan 1 / kutu kapalı = o etki yok.
    /// </summary>
    public sealed class NightModifierData : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField, Tooltip("Modüller arası referans için sabit id (ör. \"blood_moon\"). Değiştirme.")]
        private string _id = "modifier";

        [SerializeField, Tooltip("Arayüzde görünen ad (Türkçe).")]
        private string _displayName = "Olay";

        [SerializeField, TextArea, Tooltip("İstihbarat panelinde görünen açıklama (Türkçe).")]
        private string _description = string.Empty;

        [Header("Düşmanlar ve ödül")]
        [SerializeField, Min(0f), Tooltip("Gecenin düşman adedi çarpanı (boss hariç; adet en yakın tam sayıya yuvarlanır). Kan Ayı 1.5.")]
        private float _enemyCountMultiplier = 1f;

        [SerializeField, Min(0f), Tooltip("Düşman yürüme hızı çarpanı (yavaşlatma ve boss öfkesiyle çarpılır). Sis 1.1.")]
        private float _enemySpeedMultiplier = 1f;

        [SerializeField, Min(0f), Tooltip("Jeton değeri çarpanı (IRunModifiers CoinValue'ya geçici yüzde; gece bitince kalkar). Kan Ayı 2.")]
        private float _coinMultiplier = 1f;

        [Header("Sis")]
        [SerializeField, Tooltip("Açıksa gece sisi aşağıdaki renk ve yoğunluğa geçer (Sis).")]
        private bool _overrideFog;

        [SerializeField, Tooltip("Sis rengi.")]
        private Color _fogColor = new Color(0.14f, 0.16f, 0.2f);

        [SerializeField, Min(0f), Tooltip("Exponential Squared sis yoğunluğu. Kamera oyuncudan ~22 m: 0.035 ≈ oyuncu %45 sisli, 12 m ötesi neredeyse görünmez.")]
        private float _fogDensity = 0.035f;

        [Header("Ay ışığı")]
        [SerializeField, Tooltip("Açıksa ay ışığı (directional) ve ambiyans aşağıdaki renklere geçer (Kan Ayı: kırmızı).")]
        private bool _overrideLight;

        [SerializeField, Tooltip("Ay ışığının rengi.")]
        private Color _lightColor = new Color(0.95f, 0.2f, 0.14f);

        [SerializeField, Tooltip("Ortam ışığının rengi.")]
        private Color _ambientColor = new Color(0.32f, 0.08f, 0.08f);

        [Header("Güç kesintisi")]
        [SerializeField, Tooltip("Açıksa jeneratör gücü periyodik kesilir (Fırtına; şimşekle birlikte).")]
        private bool _powerOutages;

        [SerializeField, Min(0.1f), Tooltip("Kesinti periyodu (sn): gece başından her bu kadar saniyede bir kesinti başlar. Fırtına 25.")]
        private float _outageInterval = 25f;

        [SerializeField, Min(0f), Tooltip("Kesinti süresi (sn). Fırtına 5.")]
        private float _outageDuration = 5f;

        public string Id => _id;

        public string DisplayName => _displayName;

        public string Description => _description;

        public float EnemyCountMultiplier => _enemyCountMultiplier;

        public float EnemySpeedMultiplier => _enemySpeedMultiplier;

        public float CoinMultiplier => _coinMultiplier;

        /// <summary>Olayın tüm etkileri (Core olayına taşınır).</summary>
        public NightModifierSettings ToSettings()
        {
            return new NightModifierSettings(
                _id, _displayName, _description, _enemyCountMultiplier, _enemySpeedMultiplier, _coinMultiplier,
                _overrideFog, _fogColor, _fogDensity, _overrideLight, _lightColor, _ambientColor,
                _powerOutages, _outageInterval, _outageDuration);
        }
    }
}
