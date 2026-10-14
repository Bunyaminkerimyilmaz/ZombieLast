using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>Kamera ayarları (GDD §4). Asset'i üretici oluşturur: <c>Data/Camera/CameraConfig.asset</c>.</summary>
    public sealed class CameraConfig : ScriptableObject
    {
        [Header("Preset'ler")]
        [SerializeField, Tooltip("Oyun başındaki preset.")]
        private CameraPreset _defaultPreset = CameraPreset.Isometric;

        [SerializeField, Tooltip("İzometrik (varsayılan): pitch 50°, yaw 45°, 22 m, FOV 35.")]
        private CameraPresetSettings _isometric = new CameraPresetSettings(50f, 45f, 22f, 35f);

        [SerializeField, Tooltip("Tepeden: pitch 75°, yaw 0°, 24 m, FOV 40.")]
        private CameraPresetSettings _topDown = new CameraPresetSettings(75f, 0f, 24f, 40f);

        [Header("Takip")]
        [SerializeField, Min(0f), Tooltip("Kamera oyuncudan nişan noktasına doğru en fazla bu kadar kayar (m).")]
        private float _lookAheadDistance = 3f;

        [SerializeField, Min(0f), Tooltip("Takip yumuşatması: Cinemachine pozisyon sönümlemesi. 0 = anında; büyüdükçe kamera daha geç yetişir.")]
        private float _followDamping = 0.3f;

        public CameraPreset DefaultPreset => _defaultPreset;

        public float LookAheadDistance => _lookAheadDistance;

        public float FollowDamping => _followDamping;

        public CameraPresetSettings GetPreset(CameraPreset preset)
        {
            return preset == CameraPreset.TopDown ? _topDown : _isometric;
        }
    }
}
