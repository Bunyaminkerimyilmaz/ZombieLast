using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>
    /// Jetonların iniş noktaları (saf). Daire jeton sayısı kadar eşit dilime bölünür; her jeton kendi diliminde rastgele bir
    /// açıya ve [en kısa, en uzun] yatay uzaklığa düşer, böylece üst üste binmezler. Dilimlerin başlangıç açısı her
    /// saçılmada rastgeledir. Testler tohumlu <see cref="Random"/> verir.
    /// </summary>
    public sealed class CoinScatter
    {
        private const float FullTurnRadians = Mathf.PI * 2f;

        private readonly Random _random;

        public CoinScatter(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>0–360 arası rastgele açı (derece); jetonların dönüşü aynı fazda başlamasın.</summary>
        public float NextAngle()
        {
            return (float)_random.NextDouble() * 360f;
        }

        /// <summary>
        /// <paramref name="origin"/> (ölüm noktası, ayak) çevresinde <paramref name="count"/> iniş noktası üretir;
        /// yükseklik <c>origin.y + RestHeight</c>. <paramref name="results"/> temizlenip doldurulur.
        /// </summary>
        public void GetLandings(Vector3 origin, int count, CoinMotionSettings settings, List<Vector3> results)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();
            if (count <= 0)
            {
                return;
            }

            float sector = FullTurnRadians / count;
            float startAngle = Next01() * FullTurnRadians;
            float minDistance = settings.ScatterMinDistance;
            float maxDistance = settings.ScatterMaxDistance;
            float height = origin.y + settings.RestHeight;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (i + Next01()) * sector;
                float distance = Mathf.Lerp(minDistance, maxDistance, Next01());
                results.Add(new Vector3(
                    origin.x + Mathf.Sin(angle) * distance,
                    height,
                    origin.z + Mathf.Cos(angle) * distance));
            }
        }

        private float Next01()
        {
            return (float)_random.NextDouble();
        }
    }
}
