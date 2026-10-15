using LastHaven.Building;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Yerleşmiş yapı kaydı: obje ↔ hücre eşlemesi (inşa modunda üzerine gelinen yapı ve kaldırma).</summary>
    public class PlacedStructureMapTests
    {
        private GameObject _wall;
        private GameObject _other;

        [SetUp]
        public void SetUp()
        {
            _wall = new GameObject("Wall");
            _other = new GameObject("Other");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_wall);
            Object.DestroyImmediate(_other);
        }

        [Test]
        public void Add_MapsEveryCellToStructure()
        {
            var map = new PlacedStructureMap();

            Assert.IsTrue(map.Add(_wall, Placed(new RectInt(4, 5, 3, 1))));

            Assert.AreEqual(1, map.Count);
            for (int x = 4; x < 7; x++)
            {
                Assert.IsTrue(map.TryGetAt(new Vector2Int(x, 5), out GameObject found, out PlacedStructure placed), $"x={x}");
                Assert.AreSame(_wall, found);
                Assert.AreEqual(new RectInt(4, 5, 3, 1), placed.Cells);
            }

            Assert.IsFalse(map.TryGetAt(new Vector2Int(7, 5), out _, out _));
            Assert.IsFalse(map.TryGetAt(new Vector2Int(4, 6), out _, out _));
        }

        [Test]
        public void Add_SameObjectOrOverlappingCells_IsRejected()
        {
            var map = new PlacedStructureMap();
            map.Add(_wall, Placed(new RectInt(0, 0, 3, 1)));

            Assert.IsFalse(map.Add(_wall, Placed(new RectInt(10, 10, 1, 1))), "Aynı obje.");
            Assert.IsFalse(map.Add(_other, Placed(new RectInt(2, 0, 2, 2))), "Ortak hücre (2, 0).");
            Assert.IsFalse(map.TryGetAt(new Vector2Int(3, 1), out _, out _), "Reddedilen kayıt hücre bırakmaz.");
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void Remove_FreesCellsAndReturnsRecord()
        {
            var map = new PlacedStructureMap();
            map.Add(_wall, Placed(new RectInt(0, 0, 3, 1)));

            Assert.IsTrue(map.Remove(_wall, out PlacedStructure removed));

            Assert.AreEqual(new RectInt(0, 0, 3, 1), removed.Cells);
            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.TryGetAt(new Vector2Int(1, 0), out _, out _));
            Assert.IsFalse(map.TryGet(_wall, out _));
            Assert.IsFalse(map.Remove(_wall, out _), "İkinci kez yok.");
            Assert.IsTrue(map.Add(_other, Placed(new RectInt(1, 0, 1, 1))), "Hücre yeniden kullanılabilir.");
        }

        [Test]
        public void Remove_UnknownOrNull_ReturnsFalse()
        {
            var map = new PlacedStructureMap();

            Assert.IsFalse(map.Remove(_other, out _));
            Assert.IsFalse(map.Remove(null, out _));
            Assert.IsFalse(map.TryGet(null, out _));
        }

        [Test]
        public void TryFind_WalksUpFromChildToRegisteredRoot()
        {
            var map = new PlacedStructureMap();
            map.Add(_wall, Placed(new RectInt(0, 0, 3, 1)));
            var child = new GameObject("Visual/Body");
            child.transform.SetParent(_wall.transform, false);

            Assert.IsTrue(map.TryFind(child.transform, out PlacedStructure placed), "Kullanıcı modelinde collider çocukta olabilir.");
            Assert.AreEqual(new RectInt(0, 0, 3, 1), placed.Cells);
            Assert.IsTrue(map.TryFind(_wall.transform, out _));
            Assert.IsFalse(map.TryFind(_other.transform, out _));
            Assert.IsFalse(map.TryFind(null, out _));
        }

        [Test]
        public void Add_Null_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new PlacedStructureMap().Add(null, Placed(new RectInt(0, 0, 1, 1))));
        }

        private static PlacedStructure Placed(RectInt cells)
        {
            return new PlacedStructure(null, null, cells, 0);
        }
    }
}
