using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Gece olayı (GDD §14: Kan Ayı, Sis, Fırtına). Şimdilik sadece kimlik ve metin; istihbarat olay adını buradan okur.
    /// Etkiler (düşman sayısı/hız/jeton çarpanı, sis, ışık, güç kesintisi) Gün 12.3'te eklenecek.
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

        public string Id => _id;

        public string DisplayName => _displayName;

        public string Description => _description;
    }
}
