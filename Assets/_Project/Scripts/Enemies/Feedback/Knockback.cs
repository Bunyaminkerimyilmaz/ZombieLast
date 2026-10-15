using System;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Geri itilme (saf C#): verilen yatay yönde, mesafeyi süreye yayarak kare başına yer değiştirme üretir.
    /// Son karede fazlası kırpılır; toplam tam mesafe kadar olur. Yeni vuruş itmeyi baştan başlatır (pompalının saçmaları üst üste binmez).
    /// </summary>
    public sealed class Knockback
    {
        private const float MinDirectionSqr = 0.0001f;

        private Vector3 _direction;
        private float _speed;

        /// <summary>Kalan itilme mesafesi (m).</summary>
        public float RemainingDistance { get; private set; }

        public bool IsActive => RemainingDistance > 0f;

        /// <summary>Yatay itme yönü (normalize); hiç başlamadıysa sıfır.</summary>
        public Vector3 Direction => _direction;

        /// <summary>Dirençli mesafe: 0 = tam itilir, 1 = hiç itilmez (GDD §7: Tank, Brute).</summary>
        public static float ResistedDistance(float distance, float resistance)
        {
            return Math.Max(0f, distance) * (1f - Mathf.Clamp01(resistance));
        }

        /// <summary>
        /// İtmeyi başlatır. Yön yatay düzleme indirgenir. Mesafe 0 veya yön (yatayda) sıfırsa bir şey yapmaz ve false döner.
        /// Süre 0 ise itme ilk <see cref="Advance"/>'te tamamlanır.
        /// </summary>
        public bool Start(Vector3 direction, float distance, float duration)
        {
            direction.y = 0f;
            if (distance <= 0f || direction.sqrMagnitude < MinDirectionSqr)
            {
                return false;
            }

            _direction = direction.normalized;
            RemainingDistance = distance;
            _speed = duration > 0f ? distance / duration : float.PositiveInfinity;
            return true;
        }

        /// <summary>Bu karenin yer değiştirmesini döndürür; itme yoksa sıfır.</summary>
        public Vector3 Advance(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return Vector3.zero;
            }

            float step = Math.Min(RemainingDistance, _speed * deltaTime);
            RemainingDistance -= step;
            return _direction * step;
        }

        public void Stop()
        {
            RemainingDistance = 0f;
        }
    }
}
