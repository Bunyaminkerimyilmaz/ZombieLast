using System.Collections.Generic;
using System.Text.RegularExpressions;
using LastHaven.Core;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastHaven.Tests.EditMode
{
    /// <summary>SpawnZoneSet (yön → bölge) ve SpawnZone.GetRandomPoint (dalga spawn noktası).</summary>
    public class SpawnZoneSetTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                Object.DestroyImmediate(_objects[i]);
            }

            _objects.Clear();
        }

        [Test]
        public void TryGet_FindsZoneByDirection()
        {
            SpawnZone north = CreateZone(CompassDirection.North, Vector3.zero, 6f);
            SpawnZone west = CreateZone(CompassDirection.West, Vector3.zero, 6f);
            var set = new SpawnZoneSet(new[] { north, null, west });

            Assert.IsTrue(set.TryGet(CompassDirection.North, out SpawnZone foundNorth));
            Assert.AreSame(north, foundNorth);
            Assert.IsTrue(set.TryGet(CompassDirection.West, out SpawnZone foundWest));
            Assert.AreSame(west, foundWest);
            Assert.IsFalse(set.TryGet(CompassDirection.East, out SpawnZone missing));
            Assert.IsNull(missing);
        }

        [Test]
        public void DuplicateDirection_KeepsFirst_WithWarning()
        {
            SpawnZone first = CreateZone(CompassDirection.South, Vector3.zero, 6f);
            SpawnZone second = CreateZone(CompassDirection.South, Vector3.zero, 6f);

            LogAssert.Expect(LogType.Warning, new Regex("South yönünde birden fazla SpawnZone"));
            var set = new SpawnZoneSet(new[] { first, second });

            Assert.IsTrue(set.TryGet(CompassDirection.South, out SpawnZone found));
            Assert.AreSame(first, found);
        }

        [Test]
        public void NullList_HasNoZones()
        {
            var set = new SpawnZoneSet(null);

            Assert.IsFalse(set.TryGet(CompassDirection.North, out _));
        }

        [Test]
        public void GetRandomPoint_StaysInsideZoneCircle_AtCenterHeight()
        {
            var center = new Vector3(0f, 0.5f, 33f);
            SpawnZone zone = CreateZone(CompassDirection.North, center, 6f);

            for (int i = 0; i < 200; i++)
            {
                Vector3 point = zone.GetRandomPoint();
                Vector3 offset = point - center;

                Assert.AreEqual(center.y, point.y, 0.0001f);
                Assert.LessOrEqual(new Vector2(offset.x, offset.z).magnitude, 6f + 0.0001f);
            }
        }

        private SpawnZone CreateZone(CompassDirection direction, Vector3 position, float radius)
        {
            var gameObject = new GameObject($"Zone_{direction}");
            _objects.Add(gameObject);
            gameObject.transform.position = position;

            var zone = gameObject.AddComponent<SpawnZone>();
            var serialized = new SerializedObject(zone);
            serialized.FindProperty("_direction").enumValueIndex = (int)direction;
            serialized.FindProperty("_radius").floatValue = radius;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return zone;
        }
    }
}
