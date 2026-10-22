using LastHaven.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>Kamera ayarları (GDD §4, sarsıntı GDD §16). Asset'i üretici oluşturur: <c>Data/Camera/CameraConfig.asset</c>.</summary>
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

        [Header("Sarsıntı")]
        [SerializeField, Tooltip("Ekran sarsıntısı açık mı (GDD §16 ayarı). Oyun başında okunur; ayarlar ekranı (14.3) ICameraService.ShakeEnabled'ı yazar.")]
        private bool _shakeEnabled = true;

        [SerializeField, Min(0f), Tooltip("Şiddet 1'deki sarsıntı genliği (m, ekran düzleminde). Eğrinin tepesi bunun ~1.4 katı.")]
        private float _shakeAmplitude = 0.4f;

        [SerializeField, Min(0.01f), Tooltip("Bir sarsıntının süresi (sn). Hepsi aynı süreyi kullanır (Cinemachine süreyi canlı okur).")]
        private float _shakeDuration = 0.3f;

        [SerializeField, Tooltip("Sarsıntının eğrisi (Cinemachine impulse şekli). Custom seçilirse kaynağın kendi eğrisi kullanılır.")]
        private CinemachineImpulseDefinition.ImpulseShapes _shakeShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;

        [SerializeField, Min(0f), Tooltip("Tek sarsıntının en büyük şiddeti; büyük istekler buna kırpılır.")]
        private float _maxShakeStrength = 1f;

        [SerializeField, Min(0f), Tooltip("Konumlu istek kameranın odağından bu uzaklığa (m) kadar tam şiddette.")]
        private float _shakeFullRadius = 12f;

        [SerializeField, Min(0f), Tooltip("Konumlu istek bu uzaklıkta (m) ve ötesinde hissedilmez; arada doğrusal azalır.")]
        private float _shakeZeroRadius = 35f;

        public CameraPreset DefaultPreset => _defaultPreset;

        public float LookAheadDistance => _lookAheadDistance;

        public float FollowDamping => _followDamping;

        public bool ShakeEnabled => _shakeEnabled;

        public float ShakeAmplitude => _shakeAmplitude;

        public float ShakeDuration => _shakeDuration;

        public CinemachineImpulseDefinition.ImpulseShapes ShakeShape => _shakeShape;

        public CameraPresetSettings GetPreset(CameraPreset preset)
        {
            return preset == CameraPreset.TopDown ? _topDown : _isometric;
        }

        /// <summary>Bu ayarlarla sarsıntı modeli (açık/kapalı başlangıcı dahil).</summary>
        public CameraShakeModel CreateShakeModel()
        {
            return new CameraShakeModel(_maxShakeStrength, _shakeFullRadius, _shakeZeroRadius, _shakeEnabled);
        }
    }
}
