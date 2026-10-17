using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Gezgin modunun ayarları (GDD §9 gündüz tehlikesi; saf). Düşman <see cref="Center"/> merkezli halkada
    /// (<see cref="MinRadius"/>–<see cref="MaxRadius"/>) kısa ayaklarla dolaşır: rastgele bir noktaya yürür, varınca
    /// <see cref="MinPause"/>–<see cref="MaxPause"/> bekler, sonra yeni nokta seçer. Halkanın dışındaysa (ör. oyuncuyu kovaladıktan
    /// sonra) halkaya geri yürür. Değerler Waves'teki gezgin ayarlarından gelir.
    /// </summary>
    public readonly struct WanderSettings
    {
        // Ayak hedefi için halkada kalan nokta arama denemesi; bulunamazsa halkada rastgele nokta seçilir.
        private const int LegAttempts = 8;
        private const float FullTurnRadians = 2f * Mathf.PI;

        public WanderSettings(Vector3 center, float minRadius, float maxRadius, float minLegDistance, float maxLegDistance,
            float minPause, float maxPause, float maxLegDuration, float arrivalTolerance, float navMeshSampleDistance)
        {
            Center = center;
            MinRadius = Mathf.Max(0f, minRadius);
            MaxRadius = Mathf.Max(MinRadius, maxRadius);
            MinLegDistance = Mathf.Max(0f, minLegDistance);
            MaxLegDistance = Mathf.Max(MinLegDistance, maxLegDistance);
            MinPause = Mathf.Max(0f, minPause);
            MaxPause = Mathf.Max(MinPause, maxPause);
            MaxLegDuration = Mathf.Max(0f, maxLegDuration);
            ArrivalTolerance = Mathf.Max(0f, arrivalTolerance);
            NavMeshSampleDistance = Mathf.Max(0f, navMeshSampleDistance);
        }

        /// <summary>Halkanın merkezi (üs merkezi).</summary>
        public Vector3 Center { get; }

        /// <summary>Halkanın iç yarıçapı (m).</summary>
        public float MinRadius { get; }

        /// <summary>Halkanın dış yarıçapı (m).</summary>
        public float MaxRadius { get; }

        /// <summary>Bir ayağın en kısa uzunluğu (m).</summary>
        public float MinLegDistance { get; }

        /// <summary>Bir ayağın en uzun uzunluğu (m).</summary>
        public float MaxLegDistance { get; }

        /// <summary>Ayak arası en kısa bekleme (sn).</summary>
        public float MinPause { get; }

        /// <summary>Ayak arası en uzun bekleme (sn).</summary>
        public float MaxPause { get; }

        /// <summary>Bir ayak en fazla bu kadar sürer (sn); takılan düşman sonra durup yeni nokta seçer.</summary>
        public float MaxLegDuration { get; }

        /// <summary>Hedefe kalan mesafe (durma mesafesinin üstüne) bu kadarsa varılmış sayılır (m).</summary>
        public float ArrivalTolerance { get; }

        /// <summary>Seçilen noktanın en yakın NavMesh noktası en fazla bu kadar uzakta aranır (m).</summary>
        public float NavMeshSampleDistance { get; }

        /// <summary>Nokta halkada mı (yatay uzaklık, sınırlar dahil).</summary>
        public bool Contains(Vector3 point)
        {
            float distance = PlanarDistance.Between(Center, point);
            return distance >= MinRadius && distance <= MaxRadius;
        }

        /// <summary>Halkada, alana göre eşit dağılımlı rastgele nokta (y = merkez). <paramref name="random01"/> [0, 1) üretir.</summary>
        public Vector3 SampleRing(Func<float> random01)
        {
            float minSquared = MinRadius * MinRadius;
            float maxSquared = MaxRadius * MaxRadius;
            float radius = Mathf.Sqrt(minSquared + random01() * (maxSquared - minSquared));
            float angle = random01() * FullTurnRadians;
            return Center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        /// <summary>
        /// Sonraki ayağın hedefi: <paramref name="from"/>'dan rastgele yönde <see cref="MinLegDistance"/>–<see cref="MaxLegDistance"/>
        /// uzaktaki ve halkada kalan bir nokta (y = <paramref name="from"/>); birkaç denemede bulunamazsa (ör. halkanın dışındaysa)
        /// halkada rastgele nokta.
        /// </summary>
        public Vector3 PickLegDestination(Vector3 from, Func<float> random01)
        {
            for (int attempt = 0; attempt < LegAttempts; attempt++)
            {
                float distance = Mathf.Lerp(MinLegDistance, MaxLegDistance, random01());
                float angle = random01() * FullTurnRadians;
                Vector3 candidate = from + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
                if (Contains(candidate))
                {
                    return candidate;
                }
            }

            return SampleRing(random01);
        }

        /// <summary>Ayak arası bekleme: <see cref="MinPause"/>–<see cref="MaxPause"/> arası (<paramref name="random01"/> 0..1).</summary>
        public float PickPause(float random01)
        {
            return Mathf.Lerp(MinPause, MaxPause, Mathf.Clamp01(random01));
        }
    }
}
