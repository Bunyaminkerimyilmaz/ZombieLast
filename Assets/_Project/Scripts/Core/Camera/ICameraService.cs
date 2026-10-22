using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Oyun kamerası servisi: preset seçimi, kameraya göre hareket için yaw ve sarsıntı.</summary>
    public interface ICameraService
    {
        /// <summary>Ekrana çizen ana kamera (ışın atma, dünyadan ekrana yansıtma için).</summary>
        Camera Camera { get; }

        CameraPreset CurrentPreset { get; }

        /// <summary>Etkin preset'in yatay açısı (derece). Hareket ve gamepad nişanı buna göre çevrilir; W = ekranın yukarısı.</summary>
        float Yaw { get; }

        void SetPreset(CameraPreset preset);

        /// <summary>
        /// Ekran sarsıntısı açık mı (GDD §16 ayarı; başlangıç değeri kamera ayarlarından). Kapalıyken <see cref="Shake"/> ve
        /// <see cref="CameraShakeRequestedEvent"/> etkisizdir. Ayarlar ekranı (14.3) bunu yazar.
        /// </summary>
        bool ShakeEnabled { get; set; }

        /// <summary>
        /// Kamerayı uzaklıktan bağımsız sarsar (0 = yok; 1 ≈ güçlü darbe). Aynı karedeki istekler toplanmaz, en güçlüsü uygulanır.
        /// Konumlu istek için <see cref="CameraShakeRequestedEvent"/> gönder.
        /// </summary>
        void Shake(float strength);
    }
}
