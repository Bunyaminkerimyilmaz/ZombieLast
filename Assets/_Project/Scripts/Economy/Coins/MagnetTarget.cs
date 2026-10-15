using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>Jetonları çeken nokta: oyuncunun toplama noktası ve mıknatıs yarıçapı (bir karelik anlık değer).</summary>
    public readonly struct MagnetTarget
    {
        public MagnetTarget(Vector3 position, float radius, bool canAttract)
        {
            Position = position;
            Radius = radius;
            CanAttract = canAttract;
        }

        /// <summary>Çekecek oyuncu yok.</summary>
        public static MagnetTarget None => default;

        /// <summary>Jetonların uçtuğu nokta (oyuncunun gövdesi, dünya).</summary>
        public Vector3 Position { get; }

        /// <summary>Mıknatıs yarıçapı (m, yatay).</summary>
        public float Radius { get; }

        /// <summary>Yeni jeton çekebilir mi (oyuncu canlı). Uçmaya başlamış jetonlar yine de varır.</summary>
        public bool CanAttract { get; }

        /// <summary>Nokta yatay (XZ) yarıçapın içinde mi (sınır dahil)?</summary>
        public bool IsInRange(Vector3 point)
        {
            float dx = point.x - Position.x;
            float dz = point.z - Position.z;
            return dx * dx + dz * dz <= Radius * Radius;
        }
    }
}
