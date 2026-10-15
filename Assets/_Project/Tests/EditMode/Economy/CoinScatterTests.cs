using System.Collections.Generic;
using LastHaven.Economy;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Tests.EditMode
{
    /// <summary>CoinScatter (7.2): iniş noktaları uzaklık aralığında, zemin yüksekliğinde ve dilimlere dağılmış.</summary>
    public class CoinScatterTests
    {
        private const float Tolerance = 0.0001f;

        private readonly CoinMotionSettings _settings = new CoinMotionSettings();
        private readonly List<Vector3> _landings = new List<Vector3>();
        private readonly Vector3 _origin = new Vector3(4f, 0.5f, -7f);

        [Test]
        public void GetLandings_InDistanceRange_AtRestHeight()
        {
            var scatter = new CoinScatter(new Random(1));

            for (int round = 0; round < 50; round++)
            {
                scatter.GetLandings(_origin, 5, _settings, _landings);

                Assert.AreEqual(5, _landings.Count);
                foreach (Vector3 landing in _landings)
                {
                    float distance = Vector2.Distance(new Vector2(landing.x, landing.z), new Vector2(_origin.x, _origin.z));
                    Assert.That(distance, Is.InRange(_settings.ScatterMinDistance - Tolerance, _settings.ScatterMaxDistance + Tolerance));
                    Assert.AreEqual(_origin.y + _settings.RestHeight, landing.y, Tolerance);
                }
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public void GetLandings_EachCoinInOwnSector(int count)
        {
            var scatter = new CoinScatter(new Random(7));
            float sector = 360f / count;

            for (int round = 0; round < 50; round++)
            {
                scatter.GetLandings(_origin, count, _settings, _landings);

                float first = AngleOf(_landings[0]);
                for (int i = 1; i < count; i++)
                {
                    // i. jeton, ilkinin açısından (i − 1) ile (i + 1) dilim ileride olmalı.
                    float offset = Mathf.Repeat(AngleOf(_landings[i]) - first, 360f);
                    Assert.That(offset, Is.InRange((i - 1) * sector, (i + 1) * sector), $"jeton {i}, tur {round}");
                }
            }
        }

        [Test]
        public void GetLandings_ZeroCount_ClearsResults()
        {
            var scatter = new CoinScatter(new Random(1));
            _landings.Add(Vector3.one);

            scatter.GetLandings(_origin, 0, _settings, _landings);

            Assert.IsEmpty(_landings);
        }

        [Test]
        public void GetLandings_SameSeed_SameResult()
        {
            var first = new List<Vector3>();
            new CoinScatter(new Random(42)).GetLandings(_origin, 4, _settings, first);
            new CoinScatter(new Random(42)).GetLandings(_origin, 4, _settings, _landings);

            CollectionAssert.AreEqual(first, _landings);
        }

        [Test]
        public void NextAngle_InFullTurn()
        {
            var scatter = new CoinScatter(new Random(3));

            for (int i = 0; i < 100; i++)
            {
                Assert.That(scatter.NextAngle(), Is.InRange(0f, 360f));
            }
        }

        private float AngleOf(Vector3 landing)
        {
            // CoinScatter açıyı +Z'den +X'e doğru ölçer (sin → x, cos → z).
            return Mathf.Atan2(landing.x - _origin.x, landing.z - _origin.z) * Mathf.Rad2Deg;
        }
    }
}
