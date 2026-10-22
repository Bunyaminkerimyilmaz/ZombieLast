using System;

namespace LastHaven.Player
{
    /// <summary>
    /// Kamera sarsıntısı istekleri (saf C#; <see cref="CameraRig"/>'in mantığı). Konumlu istek kameranın odağından uzaklıkla zayıflar
    /// (<see cref="Falloff"/>); şiddet [0, en büyük]'e kırpılır. Aynı karedeki istekler toplanmaz: en güçlüsü bekler, köprü kare
    /// sonunda <see cref="TryConsume"/> ile tek sarsıntı üretir (pompalıyla aynı karede ölen üç zombi tek sarsıntı). Kapalıyken
    /// istekler yok sayılır.
    /// </summary>
    public sealed class CameraShakeModel
    {
        private readonly float _maxStrength;
        private readonly float _fullRadius;
        private readonly float _zeroRadius;
        private bool _enabled;

        public CameraShakeModel(float maxStrength, float fullRadius, float zeroRadius, bool enabled)
        {
            _maxStrength = Math.Max(0f, maxStrength);
            _fullRadius = Math.Max(0f, fullRadius);
            _zeroRadius = Math.Max(0f, zeroRadius);
            _enabled = enabled;
        }

        /// <summary>Kapatılınca bekleyen istek de silinir.</summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (!value)
                {
                    Pending = 0f;
                }
            }
        }

        /// <summary>Bu karede bekleyen en güçlü istek (kırpılmış, zayıflatılmış).</summary>
        public float Pending { get; private set; }

        /// <summary>Uzaklıktan bağımsız istek.</summary>
        public void Request(float strength)
        {
            if (!_enabled || !(strength > 0f))
            {
                return;
            }

            float clamped = Math.Min(strength, _maxStrength);
            if (clamped > Pending)
            {
                Pending = clamped;
            }
        }

        /// <summary>Kameranın odağından <paramref name="distance"/> m uzaktaki darbe.</summary>
        public void Request(float strength, float distance)
        {
            Request(strength * Falloff(distance, _fullRadius, _zeroRadius));
        }

        /// <summary>Bekleyen isteği alır ve sıfırlar. İstek yoksa false.</summary>
        public bool TryConsume(out float strength)
        {
            strength = Pending;
            Pending = 0f;
            return strength > 0f;
        }

        /// <summary>
        /// Uzaklık çarpanı: <paramref name="fullRadius"/> içinde 1, <paramref name="zeroRadius"/> ve ötesinde 0, arada doğrusal.
        /// Sıfır yarıçapı tam yarıçaptan büyük değilse tam yarıçapın dışı 0.
        /// </summary>
        public static float Falloff(float distance, float fullRadius, float zeroRadius)
        {
            if (distance <= fullRadius)
            {
                return 1f;
            }

            if (zeroRadius <= fullRadius || distance >= zeroRadius)
            {
                return 0f;
            }

            return 1f - (distance - fullRadius) / (zeroRadius - fullRadius);
        }
    }
}
