using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Ana menünün arka plan kamerası: <see cref="MenuConfig"/>'teki merkezin etrafında yavaşça döner ve merkeze bakar
    /// (<see cref="OrbitPose"/>; gerçek zamanla). Kameranın kendisi bu objede; oyun kamerası (Cinemachine) değil.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuCameraOrbit : MonoBehaviour
    {
        [SerializeField, Tooltip("Yörünge ayarları (Data/UI/MenuConfig.asset). Sahne adımı bağlar.")]
        private MenuConfig _config;

        private float _angle;

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("MenuCameraOrbit: MenuConfig atanmamış; Tools/Last Haven/Build Main Menu Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _angle = _config.StartAngle;
            ApplyPose();
        }

        private void Update()
        {
            _angle = OrbitPose.Advance(_angle, _config.OrbitSpeed, Time.unscaledDeltaTime);
            ApplyPose();
        }

        private void ApplyPose()
        {
            OrbitPose.Evaluate(_config.OrbitCenter, _config.OrbitRadius, _config.OrbitHeight, _config.LookHeight, _angle,
                _config.LookYawOffset, out Vector3 position, out Quaternion rotation);
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
