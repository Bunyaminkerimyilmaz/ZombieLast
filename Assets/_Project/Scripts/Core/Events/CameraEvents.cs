using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Kamera sarsıntısı isteği (ör. boss'un yere vurması). Dinleyen kamera (Gün 14: <see cref="ICameraService.Shake"/>) şiddeti
    /// uygular; ayarlarda sarsıntı kapalıysa yok sayar. Kimse dinlemiyorsa etkisizdir.
    /// </summary>
    public readonly struct CameraShakeRequestedEvent : IEvent
    {
        public CameraShakeRequestedEvent(float strength, Vector3 position)
        {
            Strength = strength;
            Position = position;
        }

        /// <summary>Sarsıntı şiddeti (0 = yok; 1 ≈ güçlü darbe).</summary>
        public float Strength { get; }

        /// <summary>Darbenin dünya konumu (uzaklığa göre zayıflatmak isteyen dinleyici için).</summary>
        public Vector3 Position { get; }
    }
}
