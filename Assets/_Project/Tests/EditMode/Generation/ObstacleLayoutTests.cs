using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Test haritasının engel yerleşimini (GDD §17) ve varsayılan harita yapılandırmasını doğrular.</summary>
    public class ObstacleLayoutTests
    {
        private const float Epsilon = 1e-3f;

        private MapLayoutConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<MapLayoutConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void DefaultConfig_PlacesAllRequestedObstacles()
        {
            ObstacleLayoutSettings settings = _config.CreateObstacleSettings();

            List<ObstaclePlacement> placements = GenerateDefault();

            Assert.AreEqual(25, settings.Count);
            Assert.AreEqual(settings.Count, placements.Count);
        }

        [Test]
        public void SameSeed_ProducesIdenticalLayout()
        {
            List<ObstaclePlacement> first = GenerateDefault();
            List<ObstaclePlacement> second = GenerateDefault();

            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].ShapeIndex, second[i].ShapeIndex);
                Assert.AreEqual(first[i].Position, second[i].Position);
                Assert.AreEqual(first[i].YawDegrees, second[i].YawDegrees);
                Assert.AreEqual(first[i].Size, second[i].Size);
            }
        }

        [Test]
        public void DifferentSeed_ProducesDifferentLayout()
        {
            ObstacleLayoutSettings a = CreateSettings(seed: 1, count: 10);
            ObstacleLayoutSettings b = CreateSettings(seed: 2, count: 10);

            List<ObstaclePlacement> first = ObstacleLayout.Generate(a, _config.ObstacleShapes, null);
            List<ObstaclePlacement> second = ObstacleLayout.Generate(b, _config.ObstacleShapes, null);

            Assert.AreNotEqual(first[0].Position, second[0].Position);
        }

        [Test]
        public void AllObstacles_StayInsideRing()
        {
            ObstacleLayoutSettings settings = _config.CreateObstacleSettings();

            foreach (ObstaclePlacement placement in GenerateDefault())
            {
                float distance = placement.Position.magnitude;
                Assert.GreaterOrEqual(distance - placement.FootprintRadius, settings.RingMin - Epsilon);
                Assert.LessOrEqual(distance + placement.FootprintRadius, settings.RingMax + Epsilon);
            }
        }

        [Test]
        public void Obstacles_KeepSpacingFromEachOther()
        {
            ObstacleLayoutSettings settings = _config.CreateObstacleSettings();
            List<ObstaclePlacement> placements = GenerateDefault();

            for (int i = 0; i < placements.Count; i++)
            {
                for (int j = i + 1; j < placements.Count; j++)
                {
                    float distance = Vector2.Distance(placements[i].Position, placements[j].Position);
                    float required = placements[i].FootprintRadius + placements[j].FootprintRadius + settings.Spacing;
                    Assert.GreaterOrEqual(distance, required - Epsilon, $"Engel {i} ve {j} çok yakın.");
                }
            }
        }

        [Test]
        public void Obstacles_KeepClearOfSpawnZonesAndBaseArea()
        {
            ObstacleLayoutSettings settings = _config.CreateObstacleSettings();
            List<ExclusionZone> exclusions = _config.CreateObstacleExclusions();

            Assert.AreEqual(1 + CompassDirections.All.Count, exclusions.Count);
            foreach (ObstaclePlacement placement in GenerateDefault())
            {
                foreach (ExclusionZone zone in exclusions)
                {
                    float distance = Vector2.Distance(placement.Position, zone.Center);
                    Assert.GreaterOrEqual(distance, zone.Radius + placement.FootprintRadius + settings.Spacing - Epsilon);
                }
            }
        }

        [Test]
        public void ZeroWeightShape_IsNeverUsed()
        {
            var shapes = new[]
            {
                new ObstacleShape("Never", Vector3.one, 0f, 0f),
                new ObstacleShape("Always", Vector3.one, 1f, 0.2f),
            };

            List<ObstaclePlacement> placements = ObstacleLayout.Generate(CreateSettings(seed: 7, count: 20), shapes, null);

            Assert.AreEqual(20, placements.Count);
            foreach (ObstaclePlacement placement in placements)
            {
                Assert.AreEqual(1, placement.ShapeIndex);
            }
        }

        [Test]
        public void RingTooNarrowForShapes_ReturnsEmpty()
        {
            var settings = new ObstacleLayoutSettings(seed: 3, count: 5, ringMin: 10f, ringMax: 11f, spacing: 1f, maxAttemptsPerObstacle: 10);

            List<ObstaclePlacement> placements = ObstacleLayout.Generate(settings, _config.ObstacleShapes, null);

            Assert.AreEqual(0, placements.Count);
        }

        [Test]
        public void DefaultConfig_SpawnZonesStayInsideMapEdge()
        {
            float edge = _config.MapSize * 0.5f;

            Assert.LessOrEqual(_config.SpawnZoneDistance + _config.SpawnZoneRadius, edge);
            Assert.AreEqual(new Vector3(0f, 0f, 33f), _config.GetSpawnZoneCenter(CompassDirection.North));
            Assert.AreEqual(new Vector3(-33f, 0f, 0f), _config.GetSpawnZoneCenter(CompassDirection.West));
        }

        private List<ObstaclePlacement> GenerateDefault()
        {
            return ObstacleLayout.Generate(
                _config.CreateObstacleSettings(), _config.ObstacleShapes, _config.CreateObstacleExclusions());
        }

        private static ObstacleLayoutSettings CreateSettings(int seed, int count)
        {
            return new ObstacleLayoutSettings(seed, count, ringMin: 18f, ringMax: 38f, spacing: 2f, maxAttemptsPerObstacle: 50);
        }
    }
}
