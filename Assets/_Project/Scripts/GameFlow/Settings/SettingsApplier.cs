using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// [Systems]/Settings: kamera ve ekran ayarlarını uygular (GDD §16) — kamera preset'i ve ekran sarsıntısı
    /// (<see cref="ICameraService"/>; ana menüde kamera servisi yok, atlanır) ve tam ekran (<see cref="Screen.fullScreen"/>; editörde
    /// etkisiz). Başlangıçta Start'ta, sonra her <see cref="SettingsChangedEvent"/>'te. Ses düzeylerini Audio modülü uygular.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SettingsApplier : MonoBehaviour
    {
        private ICameraService _camera;

        private void OnEnable()
        {
            EventBus<SettingsChangedEvent>.Subscribe(OnSettingsChanged);
        }

        private void OnDisable()
        {
            EventBus<SettingsChangedEvent>.Unsubscribe(OnSettingsChanged);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _camera);
            if (ServiceLocator.TryGet(out ISettingsService settings))
            {
                Apply(settings.Current);
            }
        }

        private void OnSettingsChanged(SettingsChangedEvent changed)
        {
            Apply(changed.Settings);
        }

        private void Apply(GameSettings settings)
        {
            if (_camera is Object cameraObject ? cameraObject != null : _camera != null)
            {
                if (_camera.CurrentPreset != settings.CameraPreset)
                {
                    _camera.SetPreset(settings.CameraPreset);
                }

                _camera.ShakeEnabled = settings.ScreenShake;
            }

            if (Screen.fullScreen != settings.Fullscreen)
            {
                Screen.fullScreen = settings.Fullscreen;
            }
        }
    }
}
