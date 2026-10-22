using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Ayarların varsayılanları (<c>Data/GameFlow/SettingsConfig.asset</c>; GDD §16). Sadece kayıtlı ayar yokken (ilk açılış) veya kayıt
    /// bozuksa kullanılır. Kamera preset'i ve sarsıntının ilk değerini üretici kamera ayarından alır.
    /// </summary>
    public sealed class SettingsConfig : ScriptableObject
    {
        [Header("Ses (0..1)")]
        [SerializeField, Range(0f, 1f), Tooltip("Genel ses (Master).")]
        private float _masterVolume = 1f;

        [SerializeField, Range(0f, 1f)]
        private float _musicVolume = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("Efekt ve arayüz sesleri (SFX).")]
        private float _sfxVolume = 1f;

        [Header("Kamera ve ekran")]
        [SerializeField, Tooltip("GDD §4: İzometrik varsayılan.")]
        private CameraPreset _cameraPreset = CameraPreset.Isometric;

        [SerializeField]
        private bool _screenShake = true;

        [SerializeField]
        private bool _fullscreen = true;

        public GameSettings Defaults => new GameSettings(_masterVolume, _musicVolume, _sfxVolume, _cameraPreset, _screenShake, _fullscreen);
    }
}
