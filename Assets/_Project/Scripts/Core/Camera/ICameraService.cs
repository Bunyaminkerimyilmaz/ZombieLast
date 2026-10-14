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

        /// <summary>Kamerayı sarsar. Şimdilik boş; Gün 14'te dolacak.</summary>
        void Shake(float strength);
    }
}
