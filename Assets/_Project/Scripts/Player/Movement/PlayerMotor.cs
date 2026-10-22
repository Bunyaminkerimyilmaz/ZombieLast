using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// CharacterController ile hareket: kamera yaw'ına göre yürüme, yerçekimi ve atılma (GDD §5).
    /// Atılma süresince <see cref="IsInvulnerable"/> true olur. Zamanlayıcı mantığı <see cref="DashModel"/>'de. Bekleme süresi
    /// <see cref="IRunModifiers"/> DashCooldown'la değişir (Adrenalin −%40), her atılmada okunur.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        // Yerdeyken küçük bir aşağı hız: CharacterController.isGrounded kararlı kalsın diye.
        private const float GroundedVerticalVelocity = -2f;

        [SerializeField, Tooltip("Oyuncu ayarları (GDD §5).")]
        private PlayerData _data;

        private readonly DashModel _dash = new DashModel();
        private CharacterController _controller;
        private Health _health;
        private IInputReader _input;
        private ICameraService _camera;
        private IRunModifiers _modifiers;
        private Vector3 _moveDirection;
        private float _verticalVelocity;
        private bool _dashRequested;
        private bool _started;

        public bool IsDashing => _dash.IsDashing;

        /// <summary>Atılma süresince true; bu sürede hasar alınmaz.</summary>
        public bool IsInvulnerable => _dash.IsDashing;

        public float DashCooldownRemaining => _dash.CooldownRemaining;

        /// <summary>Son karedeki gerçek yatay hız (m/sn); duvara takılınca düşer. Animasyonun yürüme hızı.</summary>
        public float Speed { get; private set; }

        /// <summary>Bu karenin kameraya göre çevrilmiş yürüme yönü (yatay, uzunluk 0..1).</summary>
        public Vector3 MoveDirection => _moveDirection;

        /// <summary>Atılma bekleme süresi: taban (PlayerData) × run çarpanı (Adrenalin); en az 0. <paramref name="modifiers"/> null ise taban.</summary>
        public static float CalculateDashCooldown(float baseCooldown, IRunModifiers modifiers)
        {
            return modifiers != null ? Mathf.Max(0f, modifiers.Get(StatType.DashCooldown, baseCooldown)) : baseCooldown;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<Health>();
            if (_data == null)
            {
                Debug.LogError("PlayerMotor: PlayerData atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _input))
            {
                Debug.LogWarning("PlayerMotor: IInputReader bulunamadı; oyuncu hareket etmeyecek. Sahnede [Systems]/Input var mı?", this);
            }

            if (!ServiceLocator.TryGet(out _camera))
            {
                Debug.LogWarning("PlayerMotor: ICameraService bulunamadı; hareket dünya eksenine göre olacak (W = kuzey).", this);
            }

            // Yoksa bekleme süresi sadece PlayerData'dan (WeaponController zaten uyarır).
            ServiceLocator.TryGet(out _modifiers);

            _started = true;
            SubscribeInput();
        }

        private void OnEnable()
        {
            // Servis Start'ta alınır; ilk aboneliği Start yapar.
            if (_started)
            {
                SubscribeInput();
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.DashPressed -= OnDashPressed;
            }

            _dashRequested = false;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Vector2 moveInput = _input != null && _health.IsAlive ? _input.MoveInput : Vector2.zero;
            float cameraYaw = _camera != null ? _camera.Yaw : 0f;
            _moveDirection = PlanarMath.CameraRelative(moveInput, cameraYaw);

            if (_dashRequested)
            {
                _dashRequested = false;
                TryDash();
            }

            bool dashing = _dash.IsDashing;
            Vector3 dashDisplacement = _dash.Advance(deltaTime);
            Vector3 horizontal = dashing ? dashDisplacement : _moveDirection * (_data.MoveSpeed * deltaTime);

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = GroundedVerticalVelocity;
            }

            _verticalVelocity += Physics.gravity.y * deltaTime;
            _controller.Move(horizontal + Vector3.up * (_verticalVelocity * deltaTime));

            Vector3 velocity = _controller.velocity;
            velocity.y = 0f;
            Speed = velocity.magnitude;
        }

        private void TryDash()
        {
            if (!_health.IsAlive)
            {
                return;
            }

            // Hareket girdisi yoksa baktığı yöne atılır.
            Vector3 direction = _moveDirection.sqrMagnitude >= PlanarMath.MinDirectionSqr ? _moveDirection : transform.forward;
            _dash.TryStart(direction, _data.DashDistance, _data.DashDuration, CalculateDashCooldown(_data.DashCooldown, _modifiers));
        }

        private void SubscribeInput()
        {
            if (_input != null)
            {
                _input.DashPressed += OnDashPressed;
            }
        }

        private void OnDashPressed()
        {
            // Atılma Update'te, o karenin hareket yönüyle başlar.
            _dashRequested = true;
        }
    }
}
