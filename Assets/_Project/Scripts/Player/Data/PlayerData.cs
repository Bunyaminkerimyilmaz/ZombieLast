using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>Oyuncu ayarları (GDD §5). Asset'i üretici oluşturur: <c>Data/Player/PlayerData.asset</c>.</summary>
    public sealed class PlayerData : ScriptableObject
    {
        [Header("Can")]
        [SerializeField, Min(1f), Tooltip("Maksimum can. Doğal yenilenme yok.")]
        private float _maxHealth = 100f;

        [SerializeField, Min(0f), Tooltip("Hasar aldıktan sonraki dokunulmazlık süresi (sn).")]
        private float _hitInvulnerabilityDuration = 0.3f;

        [Header("Hareket")]
        [SerializeField, Min(0f), Tooltip("Yürüme hızı (m/sn). Hareket kamera yönüne göredir.")]
        private float _moveSpeed = 6f;

        [Header("Atılma (dash)")]
        [SerializeField, Min(0f), Tooltip("Atılmada katedilen mesafe (m).")]
        private float _dashDistance = 5f;

        [SerializeField, Min(0.01f), Tooltip("Atılmanın süresi (sn). Bu süre boyunca hasar alınmaz.")]
        private float _dashDuration = 0.18f;

        [SerializeField, Min(0f), Tooltip("Atılma bekleme süresi (sn). Atılmanın başladığı andan itibaren sayılır.")]
        private float _dashCooldown = 2f;

        [Header("Nişan")]
        [SerializeField, Min(0f), Tooltip("Nişan düzleminin ayaktan yüksekliği (m). Fare ışını bu yükseklikteki yatay düzleme atılır. Namlu (Muzzle) yüksekliğiyle aynı tut.")]
        private float _aimHeight = 1.2f;

        [SerializeField, Min(0f), Tooltip("Gamepad'de nişan noktasının oyuncuya uzaklığı (m). Kamera kayması bu noktayı kullanır.")]
        private float _gamepadAimDistance = 6f;

        [Header("Toplama")]
        [SerializeField, Min(0f), Tooltip("Jeton mıknatısı yarıçapı (m).")]
        private float _pickupRadius = 3f;

        [Header("Perk'ler (GDD §13)")]
        [SerializeField, Min(0f), Tooltip("Vampir: oyuncunun her öldürmesinde iyileşen can.")]
        private float _vampireHealPerKill = 2f;

        public float MaxHealth => _maxHealth;

        public float HitInvulnerabilityDuration => _hitInvulnerabilityDuration;

        public float MoveSpeed => _moveSpeed;

        public float DashDistance => _dashDistance;

        public float DashDuration => _dashDuration;

        public float DashCooldown => _dashCooldown;

        public float AimHeight => _aimHeight;

        public float GamepadAimDistance => _gamepadAimDistance;

        public float PickupRadius => _pickupRadius;

        public float VampireHealPerKill => _vampireHealPerKill;
    }
}
