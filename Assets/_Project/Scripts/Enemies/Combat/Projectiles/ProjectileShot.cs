using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary><see cref="ArcProjectile"/>'in bir atışı: yay (başlangıç, hız, yerçekimi), çarpışma ve hasar bilgisi.</summary>
    public readonly struct ProjectileShot
    {
        public ProjectileShot(
            Vector3 start,
            Vector3 velocity,
            float gravity,
            float radius,
            int hitLayers,
            float maxLifetime,
            float damage,
            float structureMultiplier,
            GameObject source,
            GameObject impactVfx)
        {
            Start = start;
            Velocity = velocity;
            Gravity = gravity;
            Radius = radius;
            HitLayers = hitLayers;
            MaxLifetime = maxLifetime;
            Damage = damage;
            StructureMultiplier = structureMultiplier;
            Source = source;
            ImpactVfx = impactVfx;
        }

        public Vector3 Start { get; }

        /// <summary>Başlangıç hızı (<see cref="BallisticArc.LaunchVelocity"/>).</summary>
        public Vector3 Velocity { get; }

        /// <summary>Aşağı ivme (m/sn²).</summary>
        public float Gravity { get; }

        /// <summary>Çarpışma küresinin yarıçapı (m).</summary>
        public float Radius { get; }

        /// <summary>Merminin çarptığı layer'lar (oyuncu, yapı, zemin, engel).</summary>
        public int HitLayers { get; }

        /// <summary>Hiçbir şeye çarpmazsa bu kadar sn sonra havuza döner.</summary>
        public float MaxLifetime { get; }

        public float Damage { get; }

        /// <summary>Structure layer'ındaki hedefe çarpan.</summary>
        public float StructureMultiplier { get; }

        /// <summary>Atan düşman (hasar kaynağı).</summary>
        public GameObject Source { get; }

        /// <summary>Çarpınca havuzdan çıkan opsiyonel efekt; null ise atlanır.</summary>
        public GameObject ImpactVfx { get; }
    }
}
