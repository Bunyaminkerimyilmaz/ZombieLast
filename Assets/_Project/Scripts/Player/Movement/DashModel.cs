using System;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Atılma zamanlayıcısı (saf C#). Atılma süresince sabit hızla gider; bekleme süresi atılmanın başladığı anda başlar.
    /// Ayarlar her <see cref="TryStart"/>'ta verilir; Play'de data değişse veya perk bekleme süresini kısaltsa da geçerli olur.
    /// </summary>
    public sealed class DashModel
    {
        private Vector3 _direction;
        private float _speed;
        private float _dashRemaining;
        private float _cooldownRemaining;

        public bool IsDashing => _dashRemaining > 0f;

        /// <summary>Atılmaya hazır mı (atılmıyor ve bekleme bitti).</summary>
        public bool IsReady => !IsDashing && _cooldownRemaining <= 0f;

        public float CooldownRemaining => _cooldownRemaining;

        /// <summary>Son atılmanın yatay, normalize yönü.</summary>
        public Vector3 Direction => _direction;

        /// <summary>
        /// Hazırsa atılmayı başlatır. Yön yatay düzleme indirgenir; sıfırsa veya süre/mesafe geçersizse başlamaz.
        /// </summary>
        public bool TryStart(Vector3 direction, float distance, float duration, float cooldown)
        {
            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Atılma süresi pozitif olmalı.");
            }

            direction.y = 0f;
            if (!IsReady || distance <= 0f || direction.sqrMagnitude < PlanarMath.MinDirectionSqr)
            {
                return false;
            }

            _direction = direction.normalized;
            _speed = distance / duration;
            _dashRemaining = duration;
            _cooldownRemaining = cooldown;
            return true;
        }

        /// <summary>
        /// Zamanı ilerletir. Atılma sürüyorsa bu karedeki yer değiştirmeyi döndürür (toplamı tam mesafeyi verir), yoksa sıfır.
        /// </summary>
        public Vector3 Advance(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return Vector3.zero;
            }

            _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - deltaTime);
            if (!IsDashing)
            {
                return Vector3.zero;
            }

            float step = Mathf.Min(deltaTime, _dashRemaining);
            _dashRemaining -= step;
            return _direction * (_speed * step);
        }

        /// <summary>Atılmayı ve bekleme süresini sıfırlar (ör. yeniden doğuşta).</summary>
        public void Reset()
        {
            _dashRemaining = 0f;
            _cooldownRemaining = 0f;
        }
    }
}
