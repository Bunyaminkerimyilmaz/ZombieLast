using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Editor
{
    /// <summary>Engel yerleşiminin ayarları. Yarıçaplar harita merkezine göredir (m).</summary>
    public readonly struct ObstacleLayoutSettings
    {
        public ObstacleLayoutSettings(int seed, int count, float ringMin, float ringMax, float spacing, int maxAttemptsPerObstacle)
        {
            Seed = seed;
            Count = count;
            RingMin = ringMin;
            RingMax = ringMax;
            Spacing = spacing;
            MaxAttemptsPerObstacle = maxAttemptsPerObstacle;
        }

        public int Seed { get; }

        public int Count { get; }

        /// <summary>Engelin hiçbir parçası bu yarıçapın içine girmez.</summary>
        public float RingMin { get; }

        /// <summary>Engelin hiçbir parçası bu yarıçapın dışına çıkmaz.</summary>
        public float RingMax { get; }

        /// <summary>Engeller arası ve yasak bölgelere en az boşluk.</summary>
        public float Spacing { get; }

        public int MaxAttemptsPerObstacle { get; }
    }

    /// <summary>Engel konamayacak yatay daire (XZ düzleminde).</summary>
    public readonly struct ExclusionZone
    {
        public ExclusionZone(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public Vector2 Center { get; }

        public float Radius { get; }
    }

    /// <summary>Yerleştirilmiş tek engel. <see cref="Position"/> XZ düzlemindedir.</summary>
    public readonly struct ObstaclePlacement
    {
        public ObstaclePlacement(int shapeIndex, Vector2 position, float yawDegrees, Vector3 size)
        {
            ShapeIndex = shapeIndex;
            Position = position;
            YawDegrees = yawDegrees;
            Size = size;
        }

        public int ShapeIndex { get; }

        public Vector2 Position { get; }

        public float YawDegrees { get; }

        public Vector3 Size { get; }

        /// <summary>Hangi yöne dönerse dönsün kutuyu içine alan dairenin yarıçapı.</summary>
        public float FootprintRadius => GetFootprintRadius(Size);

        public static float GetFootprintRadius(Vector3 size)
        {
            return 0.5f * Mathf.Sqrt(size.x * size.x + size.z * size.z);
        }
    }

    /// <summary>
    /// Sabit seed ile halka içine çakışmayan engeller yerleştirir (saf C#). Aynı girdi her zaman aynı sonucu verir.
    /// Engeller dönüşten bağımsız çevre daireleriyle karşılaştırılır; bu yüzden yerleşim biraz cömert boşluk bırakır.
    /// </summary>
    public static class ObstacleLayout
    {
        /// <summary>
        /// En fazla <see cref="ObstacleLayoutSettings.Count"/> engel yerleştirir. Yer kalmazsa daha azını döndürür.
        /// </summary>
        public static List<ObstaclePlacement> Generate(
            in ObstacleLayoutSettings settings,
            IReadOnlyList<ObstacleShape> shapes,
            IReadOnlyList<ExclusionZone> exclusions)
        {
            var placements = new List<ObstaclePlacement>(Mathf.Max(0, settings.Count));
            float totalWeight = GetTotalWeight(shapes);
            if (settings.Count <= 0 || totalWeight <= 0f)
            {
                return placements;
            }

            var random = new Random(settings.Seed);
            int maxAttempts = settings.Count * Mathf.Max(1, settings.MaxAttemptsPerObstacle);
            for (int attempt = 0; attempt < maxAttempts && placements.Count < settings.Count; attempt++)
            {
                int shapeIndex = PickShape(shapes, totalWeight, random);
                ObstacleShape shape = shapes[shapeIndex];
                float scale = 1f + ((float)random.NextDouble() * 2f - 1f) * shape.SizeJitter;
                Vector3 size = shape.Size * scale;
                float footprint = ObstaclePlacement.GetFootprintRadius(size);

                float innerRadius = settings.RingMin + footprint;
                float outerRadius = settings.RingMax - footprint;
                if (outerRadius < innerRadius)
                {
                    continue;
                }

                // Alana göre düzgün dağılım: yarıçapın karesi doğrusal seçilir.
                float radius = Mathf.Sqrt(Mathf.Lerp(innerRadius * innerRadius, outerRadius * outerRadius, (float)random.NextDouble()));
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float yaw = (float)random.NextDouble() * 360f;
                var position = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

                if (IsBlocked(position, footprint, settings.Spacing, placements, exclusions))
                {
                    continue;
                }

                placements.Add(new ObstaclePlacement(shapeIndex, position, yaw, size));
            }

            return placements;
        }

        private static float GetTotalWeight(IReadOnlyList<ObstacleShape> shapes)
        {
            float total = 0f;
            for (int i = 0; i < shapes.Count; i++)
            {
                total += Mathf.Max(0f, shapes[i].Weight);
            }

            return total;
        }

        private static int PickShape(IReadOnlyList<ObstacleShape> shapes, float totalWeight, Random random)
        {
            float roll = (float)random.NextDouble() * totalWeight;
            int lastUsable = 0;
            for (int i = 0; i < shapes.Count; i++)
            {
                float weight = Mathf.Max(0f, shapes[i].Weight);
                if (weight <= 0f)
                {
                    continue;
                }

                lastUsable = i;
                if (roll < weight)
                {
                    return i;
                }

                roll -= weight;
            }

            // Kayan nokta yuvarlaması için: son kullanılabilir şekil.
            return lastUsable;
        }

        private static bool IsBlocked(
            Vector2 position,
            float footprint,
            float spacing,
            List<ObstaclePlacement> placements,
            IReadOnlyList<ExclusionZone> exclusions)
        {
            if (exclusions != null)
            {
                for (int i = 0; i < exclusions.Count; i++)
                {
                    float minDistance = exclusions[i].Radius + footprint + spacing;
                    if ((position - exclusions[i].Center).sqrMagnitude < minDistance * minDistance)
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < placements.Count; i++)
            {
                float minDistance = placements[i].FootprintRadius + footprint + spacing;
                if ((position - placements[i].Position).sqrMagnitude < minDistance * minDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
