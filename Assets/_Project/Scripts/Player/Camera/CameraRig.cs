using LastHaven.Core;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// <see cref="ICameraService"/>: Cinemachine 3 kurulumunu yönetir. Main Camera'da CinemachineBrain, ayrı objede
    /// CinemachineCamera + CinemachineFollow (dünya ofseti) + CinemachineImpulseListener. Açı preset'ten gelir ve sabittir (rotasyon
    /// bileşeni yok). Takip hedefi "CameraTarget", oyuncudan nişan noktasına doğru look-ahead kadar kayar.
    /// Sarsıntı: <see cref="Shake"/> ve <see cref="CameraShakeRequestedEvent"/> istekleri <see cref="CameraShakeModel"/>'de toplanır
    /// (konumlu istek takip hedefinden uzaklıkla zayıflar), LateUpdate'te kare başına tek <see cref="CinemachineImpulseSource"/> impulse'u
    /// (ekran düzleminde rastgele yön) üretilir. Ayarlarda kapalıysa (<see cref="ShakeEnabled"/>) hiçbir şey olmaz.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(20)] // PlayerAim'den (10) sonra: bu karenin nişan noktasını kullanır.
    public sealed class CameraRig : MonoBehaviour, ICameraService
    {
        [Header("Ayarlar")]
        [SerializeField, Tooltip("Kamera ayarları (GDD §4).")]
        private CameraConfig _config;

        [Header("Bağlantılar (üretici bağlar)")]
        [SerializeField, Tooltip("CinemachineBrain'li ana kamera.")]
        private Camera _camera;

        [SerializeField]
        private CinemachineCamera _cinemachineCamera;

        [SerializeField]
        private CinemachineFollow _follow;

        [SerializeField, Tooltip("Takip hedefi; oyuncu ile nişan noktası arasında kayar.")]
        private Transform _target;

        [SerializeField, Tooltip("Sarsıntı kaynağı. CinemachineCamera'da CinemachineImpulseListener olmalı.")]
        private CinemachineImpulseSource _impulseSource;

        private IPlayer _player;
        private CameraPreset _currentPreset;
        private float _yaw;
        private CameraShakeModel _shake;

        public Camera Camera => _camera;

        public CameraPreset CurrentPreset => _currentPreset;

        public float Yaw => _yaw;

        public bool ShakeEnabled
        {
            get => _shake != null && _shake.Enabled;
            set
            {
                if (_shake != null)
                {
                    _shake.Enabled = value;
                }
            }
        }

        public void SetPreset(CameraPreset preset)
        {
            _currentPreset = preset;
            CameraPresetSettings settings = _config.GetPreset(preset);
            _yaw = settings.Yaw;

            // Rotasyon bileşeni yok: CinemachineCamera açıyı kendi transform'undan alır.
            _cinemachineCamera.transform.rotation = settings.Rotation;
            _follow.FollowOffset = settings.FollowOffset;

            LensSettings lens = _cinemachineCamera.Lens;
            lens.FieldOfView = settings.FieldOfView;
            _cinemachineCamera.Lens = lens;
        }

        public void Shake(float strength)
        {
            _shake?.Request(strength);
        }

        private void Awake()
        {
            if (_config == null || _camera == null || _cinemachineCamera == null || _follow == null || _target == null
                || _impulseSource == null)
            {
                Debug.LogError("CameraRig: referanslar eksik; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            _follow.TrackerSettings.PositionDamping = Vector3.one * _config.FollowDamping;
            _cinemachineCamera.Follow = _target;
            SetPreset(_config.DefaultPreset);
            ConfigureImpulse();
            _shake = _config.CreateShakeModel();
            ServiceLocator.Register<ICameraService>(this);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _player))
            {
                Debug.LogWarning("CameraRig: IPlayer bulunamadı; kamera kimseyi takip etmeyecek.", this);
                return;
            }

            // İlk karede sönümleme olmadan oyuncunun üstüne otursun.
            _target.position = _player.Position;
            _cinemachineCamera.PreviousStateIsValid = false;
        }

        private void OnEnable()
        {
            EventBus<CameraShakeRequestedEvent>.Subscribe(OnShakeRequested);
        }

        private void OnDisable()
        {
            EventBus<CameraShakeRequestedEvent>.Unsubscribe(OnShakeRequested);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ICameraService>(this);
        }

        private void Update()
        {
            if (_player == null)
            {
                return;
            }

            Vector3 position = _player.Position;
            _target.position = position + LookAhead.Compute(position, _player.AimPoint, _config.LookAheadDistance);
        }

        private void LateUpdate()
        {
            // Bu karenin istekleri toplandı; impulse beyin kamerayı güncellemeden eklenir.
            if (_shake != null && _shake.TryConsume(out float strength))
            {
                Vector2 direction = Random.insideUnitCircle;
                direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.up;
                _impulseSource.GenerateImpulseWithVelocity((Vector3)direction * (strength * _config.ShakeAmplitude));
            }
        }

        private void OnShakeRequested(CameraShakeRequestedEvent request)
        {
            if (_shake == null)
            {
                return;
            }

            if (request.HasPosition)
            {
                _shake.Request(request.Strength, PlanarDistance.Between(_target.position, request.Position));
            }
            else
            {
                _shake.Request(request.Strength);
            }
        }

        // Kaynak her yerde aynı hissedilir (konum zayıflaması CameraShakeModel'de); süre ve eğri ayarlardan.
        private void ConfigureImpulse()
        {
            CinemachineImpulseDefinition impulse = _impulseSource.ImpulseDefinition;
            impulse.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            impulse.ImpulseShape = _config.ShakeShape;
            impulse.ImpulseDuration = _config.ShakeDuration;
        }
    }
}
