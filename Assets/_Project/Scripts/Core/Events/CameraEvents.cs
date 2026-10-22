using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Kamera sarsıntısı isteği (boss'un yere vurması, patlama, ağır öldürme...). Kamera (<c>CameraRig</c>) dinler: konumlu istek
    /// kameranın odağına uzaklıkla zayıflar, konumsuz (genel) istek her yerde aynıdır (ör. şimşek). Ayarlarda sarsıntı kapalıysa
    /// (<see cref="ICameraService.ShakeEnabled"/>) yok sayılır. Aynı karedeki istekler toplanmaz, en güçlüsü uygulanır.
    /// </summary>
    public readonly struct CameraShakeRequestedEvent : IEvent
    {
        /// <summary>Konumlu istek: kameranın odağından uzaklaştıkça zayıflar.</summary>
        public CameraShakeRequestedEvent(float strength, Vector3 position)
        {
            Strength = strength;
            Position = position;
            HasPosition = true;
        }

        /// <summary>Genel istek: uzaklıktan bağımsız (ör. şimşek).</summary>
        public CameraShakeRequestedEvent(float strength)
        {
            Strength = strength;
            Position = Vector3.zero;
            HasPosition = false;
        }

        /// <summary>Sarsıntı şiddeti (0 = yok; 1 ≈ güçlü darbe).</summary>
        public float Strength { get; }

        /// <summary>Darbenin dünya konumu; sadece <see cref="HasPosition"/> true ise anlamlı.</summary>
        public Vector3 Position { get; }

        /// <summary>false: genel istek, uzaklıkla zayıflamaz.</summary>
        public bool HasPosition { get; }
    }
}
