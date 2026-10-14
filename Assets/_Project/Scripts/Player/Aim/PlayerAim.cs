using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Nişan: fare modunda imleçten nişan yüksekliğindeki yatay düzleme ışın atar; gamepad'de sağ çubuk yönüne döner
    /// (çubuk boştaysa yürüme yönüne). Oyuncuyu nişan yönüne çevirir.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10)] // PlayerMotor'dan sonra: bu karenin pozisyonunu ve yürüme yönünü kullanır.
    [RequireComponent(typeof(PlayerMotor), typeof(Health))]
    public sealed class PlayerAim : MonoBehaviour
    {
        // İmleç oyuncunun bu kadar yakınındaysa (m) yön değişmez; tam üstündeyken titremesin.
        private const float MinPointerDistance = 0.1f;

        [SerializeField, Tooltip("Oyuncu ayarları (GDD §5).")]
        private PlayerData _data;

        private PlayerMotor _motor;
        private Health _health;
        private IInputReader _input;
        private ICameraService _camera;
        private Vector3 _aimPoint;
        private Vector3 _aimDirection = Vector3.forward;

        /// <summary>Nişan noktası (dünya), nişan düzleminin yüksekliğinde.</summary>
        public Vector3 AimPoint => _aimPoint;

        /// <summary>Yatay, normalize nişan yönü.</summary>
        public Vector3 AimDirection => _aimDirection;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _health = GetComponent<Health>();
            if (_data == null)
            {
                Debug.LogError("PlayerAim: PlayerData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
                return;
            }

            _aimDirection = PlanarMath.FlatDirection(Vector3.zero, transform.forward, Vector3.forward, 0f);
            _aimPoint = GetPointAlongAim(transform.position);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _input))
            {
                Debug.LogWarning("PlayerAim: IInputReader bulunamadı; oyuncu nişan almayacak.", this);
            }

            if (!ServiceLocator.TryGet(out _camera))
            {
                Debug.LogWarning("PlayerAim: ICameraService bulunamadı; fare nişanı çalışmayacak.", this);
            }
        }

        private void Update()
        {
            if (_input == null || !_health.IsAlive)
            {
                return;
            }

            Vector3 origin = transform.position;
            if (_input.IsUsingGamepad)
            {
                AimWithStick(origin);
            }
            else
            {
                AimWithPointer(origin);
            }

            transform.rotation = Quaternion.LookRotation(_aimDirection, Vector3.up);
        }

        private void AimWithPointer(Vector3 origin)
        {
            Camera camera = _camera != null ? _camera.Camera : null;
            if (camera == null)
            {
                return;
            }

            Ray ray = camera.ScreenPointToRay(_input.AimScreenPosition);
            float planeHeight = origin.y + _data.AimHeight;
            if (!PlanarMath.TryIntersectHorizontalPlane(ray.origin, ray.direction, planeHeight, out Vector3 point))
            {
                return;
            }

            _aimPoint = point;
            _aimDirection = PlanarMath.FlatDirection(origin, point, _aimDirection, MinPointerDistance);
        }

        private void AimWithStick(Vector3 origin)
        {
            Vector2 stick = _input.AimStick;
            Vector3 direction = stick.sqrMagnitude > 0f
                ? PlanarMath.CameraRelative(stick, _camera != null ? _camera.Yaw : 0f)
                : _motor.MoveDirection;

            if (direction.sqrMagnitude >= PlanarMath.MinDirectionSqr)
            {
                _aimDirection = direction.normalized;
            }

            _aimPoint = GetPointAlongAim(origin);
        }

        private Vector3 GetPointAlongAim(Vector3 origin)
        {
            Vector3 point = origin + _aimDirection * _data.GamepadAimDistance;
            point.y = origin.y + _data.AimHeight;
            return point;
        }
    }
}
