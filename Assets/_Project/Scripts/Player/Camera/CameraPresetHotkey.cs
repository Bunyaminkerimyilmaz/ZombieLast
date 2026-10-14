using LastHaven.Core;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.Player
{
    /// <summary>
    /// Geliştirme kısayolu: F9 kamera preset'ini sırayla değiştirir. Sadece editörde ve development build'de çalışır;
    /// sürüm build'inde bileşen boş kalır (sahnede "missing script" olmasın diye sınıf her zaman var).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraPresetHotkey : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Binding = "<Keyboard>/f9";

        private InputAction _action;
        private ICameraService _camera;

        private void Awake()
        {
            // Oyun girdisi değil, geliştirici kısayolu: IInputReader'a eklenmedi.
            _action = new InputAction("CycleCameraPreset", InputActionType.Button, Binding);
            _action.performed += OnPerformed;
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _camera);
        }

        private void OnEnable()
        {
            _action.Enable();
        }

        private void OnDisable()
        {
            _action.Disable();
        }

        private void OnDestroy()
        {
            _action.Dispose();
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            if (_camera == null)
            {
                return;
            }

            CameraPreset next = CameraPresets.Next(_camera.CurrentPreset);
            _camera.SetPreset(next);
            Debug.Log($"[Camera] Preset: {next}");
        }
#endif
    }
}
