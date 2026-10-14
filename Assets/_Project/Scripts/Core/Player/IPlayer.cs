using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Oyuncu karakteri servisi. Kamera, düşmanlar, jetonlar ve UI oyuncuyu buradan okur.</summary>
    public interface IPlayer
    {
        Transform Transform { get; }

        /// <summary>Ayak pozisyonu (dünya).</summary>
        Vector3 Position { get; }

        /// <summary>Nişan noktası (dünya). Nişan düzleminin yüksekliğindedir (namlu yüksekliği).</summary>
        Vector3 AimPoint { get; }

        /// <summary>Yatay (y = 0), normalize nişan yönü. Oyuncu bu yöne bakar.</summary>
        Vector3 AimDirection { get; }

        bool IsAlive { get; }

        /// <summary>Jeton mıknatısı yarıçapı (m).</summary>
        float PickupRadius { get; }
    }
}
