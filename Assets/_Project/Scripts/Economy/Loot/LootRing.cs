using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>
    /// Sandıkların çıktığı halka (GDD §9: merkezden 22–38 m; saf). Nokta alana göre eşit dağılır (dış kenar seyrekleşmez);
    /// yükseklik merkezinkidir, zemin <c>LootCratePlacer</c>'da NavMesh'ten alınır.
    /// </summary>
    public readonly struct LootRing
    {
        private const float FullTurnRadians = 2f * Mathf.PI;

        public LootRing(float minRadius, float maxRadius)
        {
            MinRadius = Mathf.Max(0f, minRadius);
            MaxRadius = Mathf.Max(MinRadius, maxRadius);
        }

        public float MinRadius { get; }

        public float MaxRadius { get; }

        /// <summary>Halkada rastgele nokta (XZ; y = merkez).</summary>
        public Vector3 Sample(Vector3 center, Random random)
        {
            float minSquared = MinRadius * MinRadius;
            float maxSquared = MaxRadius * MaxRadius;
            float radius = Mathf.Sqrt(minSquared + (float)random.NextDouble() * (maxSquared - minSquared));
            float angle = (float)random.NextDouble() * FullTurnRadians;
            return center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        /// <summary>Nokta halkada mı (yatay uzaklık, sınırlar dahil).</summary>
        public bool Contains(Vector3 center, Vector3 point)
        {
            float distance = PlanarDistance.Between(center, point);
            return distance >= MinRadius && distance <= MaxRadius;
        }

        /// <summary>Nokta diğerlerinin hepsine en az <paramref name="minSpacing"/> (yatay, dahil) uzak mı.</summary>
        public static bool HasSpacing(Vector3 point, IReadOnlyList<Vector3> others, float minSpacing)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (PlanarDistance.Between(point, others[i]) < minSpacing)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
