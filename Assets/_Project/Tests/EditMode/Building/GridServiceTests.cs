using System;
using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// İnşa ızgarası (GDD §11): dünya ↔ hücre, döndürülmüş ayak izi, üs alanı, jeneratör yasak bölgesi, doluluk.
    /// Varsayılan kurulum: merkez orijin, yarıçap 14, hücre 1 m, yasak bölge yarı kenarı 3 (jeneratör 1 + 2 m).
    /// </summary>
    public class GridServiceTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Constructor_NonPositiveCellSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridService(new FakeArea(Vector3.zero, 14f), 0f, 3f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridService(new FakeArea(Vector3.zero, 14f), -1f, 3f));
        }

        [Test]
        public void Constructor_NullArea_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GridService(null, 1f, 3f));
        }

        [Test]
        public void Origin_IsAreaCenter()
        {
            var center = new Vector3(10f, 2f, -5f);
            GridService grid = Create(center);

            Assert.AreEqual(center, grid.Origin);
        }

        [Test]
        public void WorldToCell_UsesFloorAroundOrigin()
        {
            GridService grid = Create();

            Assert.AreEqual(new Vector2Int(0, 0), grid.WorldToCell(new Vector3(0.1f, 0f, 0.9f)));
            Assert.AreEqual(new Vector2Int(-1, -1), grid.WorldToCell(new Vector3(-0.1f, 0f, -0.1f)));
            Assert.AreEqual(new Vector2Int(5, -3), grid.WorldToCell(new Vector3(5.5f, 0f, -2.5f)));
        }

        [Test]
        public void WorldToCell_BoundaryBelongsToNextCell()
        {
            GridService grid = Create();

            Assert.AreEqual(new Vector2Int(1, 0), grid.WorldToCell(new Vector3(1f, 0f, 0f)));
        }

        [Test]
        public void WorldToCell_IgnoresHeight()
        {
            GridService grid = Create();

            Assert.AreEqual(grid.WorldToCell(new Vector3(2.5f, 0f, 3.5f)), grid.WorldToCell(new Vector3(2.5f, 40f, 3.5f)));
        }

        [Test]
        public void WorldToCell_UsesOriginOffsetAndCellSize()
        {
            var grid = new GridService(new FakeArea(new Vector3(10f, 0f, -5f), 14f), 2f, 3f);

            Assert.AreEqual(new Vector2Int(0, 0), grid.WorldToCell(new Vector3(10.5f, 0f, -4.5f)));
            Assert.AreEqual(new Vector2Int(1, -1), grid.WorldToCell(new Vector3(13.9f, 0f, -5.1f)));
        }

        [Test]
        public void CellToWorld_IsCellCenterAtOriginHeight()
        {
            GridService grid = Create(new Vector3(0f, 2f, 0f));

            AssertVector(new Vector3(0.5f, 2f, 0.5f), grid.CellToWorld(new Vector2Int(0, 0)));
            AssertVector(new Vector3(-2.5f, 2f, 4.5f), grid.CellToWorld(new Vector2Int(-3, 4)));
        }

        [TestCase(0, 0)]
        [TestCase(-7, 3)]
        [TestCase(12, -12)]
        public void CellToWorld_RoundTripsThroughWorldToCell(int x, int y)
        {
            var grid = new GridService(new FakeArea(new Vector3(3f, 0f, -8f), 14f), 0.5f, 3f);
            var cell = new Vector2Int(x, y);

            Assert.AreEqual(cell, grid.WorldToCell(grid.CellToWorld(cell)));
        }

        [TestCase(0, 0)]
        [TestCase(3, 3)]
        [TestCase(4, 0)]
        [TestCase(5, 1)]
        [TestCase(-1, 3)]
        [TestCase(-6, 2)]
        public void NormalizeRotation_WrapsToZeroToThree(int steps, int expected)
        {
            Assert.AreEqual(expected, GridService.NormalizeRotation(steps));
        }

        [TestCase(0, 3, 1)]
        [TestCase(1, 1, 3)]
        [TestCase(2, 3, 1)]
        [TestCase(3, 1, 3)]
        [TestCase(-1, 1, 3)]
        public void RotateFootprint_OddStepsSwapSides(int steps, int expectedX, int expectedY)
        {
            Assert.AreEqual(new Vector2Int(expectedX, expectedY), GridService.RotateFootprint(new Vector2Int(3, 1), steps));
        }

        [Test]
        public void RotateFootprint_ClampsToAtLeastOneCell()
        {
            Assert.AreEqual(Vector2Int.one, GridService.RotateFootprint(Vector2Int.zero, 0));
            Assert.AreEqual(new Vector2Int(1, 2), GridService.RotateFootprint(new Vector2Int(2, -4), 1));
        }

        [Test]
        public void GetFootprintCells_SingleCell_IsCellUnderPoint()
        {
            GridService grid = Create();
            var point = new Vector3(5.7f, 0f, -2.2f);

            RectInt cells = grid.GetFootprintCells(point, Vector2Int.one, 0);

            Assert.AreEqual(new RectInt(5, -3, 1, 1), cells);
            Assert.AreEqual(grid.WorldToCell(point), cells.min);
        }

        [Test]
        public void GetFootprintCells_OddFootprint_PointCellIsInTheMiddle()
        {
            GridService grid = Create();
            var point = new Vector3(5.5f, 0f, 5.5f);

            Assert.AreEqual(new RectInt(4, 5, 3, 1), grid.GetFootprintCells(point, new Vector2Int(3, 1), 0));
            Assert.AreEqual(new RectInt(5, 4, 1, 3), grid.GetFootprintCells(point, new Vector2Int(3, 1), 1), "90°: dikey.");
            Assert.AreEqual(new RectInt(4, 5, 3, 1), grid.GetFootprintCells(point, new Vector2Int(3, 1), 2), "180°: yine yatay.");
        }

        [Test]
        public void GetFootprintCells_EvenFootprint_SnapsToNearestGridCorner()
        {
            GridService grid = Create();

            Assert.AreEqual(new RectInt(4, 4, 2, 2), grid.GetFootprintCells(new Vector3(5.2f, 0f, 5.3f), new Vector2Int(2, 2), 0));
            Assert.AreEqual(new RectInt(5, 5, 2, 2), grid.GetFootprintCells(new Vector3(5.8f, 0f, 5.9f), new Vector2Int(2, 2), 0));
            Assert.AreEqual(new RectInt(-6, -2, 2, 2), grid.GetFootprintCells(new Vector3(-4.6f, 0f, -0.6f), new Vector2Int(2, 2), 3));
        }

        [Test]
        public void GetFootprintCells_UsesOriginOffset()
        {
            GridService grid = Create(new Vector3(10f, 0f, 10f));

            Assert.AreEqual(new RectInt(0, 0, 1, 1), grid.GetFootprintCells(new Vector3(10.5f, 0f, 10.5f), Vector2Int.one, 0));
        }

        [Test]
        public void GetFootprintCenter_IsRectCenterAtOriginHeight()
        {
            GridService grid = Create(new Vector3(0f, 1f, 0f));

            AssertVector(new Vector3(5.5f, 1f, 5.5f), grid.GetFootprintCenter(new RectInt(4, 5, 3, 1)));
            AssertVector(new Vector3(5f, 1f, 5f), grid.GetFootprintCenter(new RectInt(4, 4, 2, 2)));
        }

        [Test]
        public void GetFootprintCenter_OddFootprint_CentersOnPointCell()
        {
            GridService grid = Create();
            var point = new Vector3(-6.3f, 0f, 2.8f);

            RectInt cells = grid.GetFootprintCells(point, new Vector2Int(3, 1), 1);

            AssertVector(grid.CellToWorld(grid.WorldToCell(point)), grid.GetFootprintCenter(cells));
        }

        [Test]
        public void IsInsideArea_AllCornersMustBeInside()
        {
            GridService grid = Create();

            Assert.IsTrue(grid.IsInsideArea(new RectInt(8, 9, 1, 1)), "En uzak köşe (9, 10): 13.45 m.");
            Assert.IsFalse(grid.IsInsideArea(new RectInt(9, 9, 1, 1)), "En uzak köşe (10, 10): 14.14 m.");
            Assert.IsFalse(grid.IsInsideArea(new RectInt(11, 0, 3, 1)), "Sağ kenar 14'te ama üst köşe dışarıda.");
        }

        [Test]
        public void IsInsideArea_BoundaryIsInclusive()
        {
            var grid = new GridService(new FakeArea(Vector3.zero, 5f), 1f, 0f);

            Assert.IsTrue(grid.IsInsideArea(new RectInt(0, 0, 3, 4)), "Köşe (3, 4) tam 5 m'de.");
            Assert.IsFalse(grid.IsInsideArea(new RectInt(0, 0, 4, 4)));
        }

        [Test]
        public void IsInsideArea_FollowsAreaCenter()
        {
            GridService grid = Create(new Vector3(30f, 0f, 30f));

            Assert.IsTrue(grid.IsInsideArea(new RectInt(5, 5, 2, 2)));
            Assert.IsFalse(grid.IsInsideArea(new RectInt(-30, -30, 1, 1)), "Dünya orijini bu üssün dışında.");
        }

        [Test]
        public void OverlapsGeneratorKeepOut_InsideSquare_IsForbidden()
        {
            GridService grid = Create();

            Assert.IsTrue(grid.OverlapsGeneratorKeepOut(new RectInt(0, 0, 1, 1)));
            Assert.IsTrue(grid.OverlapsGeneratorKeepOut(new RectInt(2, 2, 1, 1)), "Jeneratör kenarından (1 m) 2 m içinde.");
            Assert.IsTrue(grid.OverlapsGeneratorKeepOut(new RectInt(-3, -3, 1, 1)));
            Assert.IsTrue(grid.OverlapsGeneratorKeepOut(new RectInt(2, -6, 3, 9)), "Yasak bölgeyi kesen uzun yapı.");
        }

        [Test]
        public void OverlapsGeneratorKeepOut_TouchingEdge_IsAllowed()
        {
            GridService grid = Create();

            Assert.IsFalse(grid.OverlapsGeneratorKeepOut(new RectInt(3, 0, 1, 1)));
            Assert.IsFalse(grid.OverlapsGeneratorKeepOut(new RectInt(-4, -1, 1, 3)));
            Assert.IsFalse(grid.OverlapsGeneratorKeepOut(new RectInt(-1, 3, 3, 1)));
            Assert.IsFalse(grid.OverlapsGeneratorKeepOut(new RectInt(3, 3, 2, 2)), "Köşeden değiyor.");
        }

        [Test]
        public void OverlapsGeneratorKeepOut_UsesCellSize()
        {
            var grid = new GridService(new FakeArea(Vector3.zero, 14f), 0.5f, 3f);

            Assert.IsTrue(grid.OverlapsGeneratorKeepOut(new RectInt(5, 0, 1, 1)), "2.5–3 m.");
            Assert.IsFalse(grid.OverlapsGeneratorKeepOut(new RectInt(6, 0, 1, 1)), "3–3.5 m.");
        }

        [Test]
        public void OverlapsGeneratorKeepOut_ZeroExtent_NeverForbidden()
        {
            var grid = new GridService(new FakeArea(Vector3.zero, 14f), 1f, 0f);

            Assert.IsFalse(grid.OverlapsGeneratorKeepOut(new RectInt(0, 0, 1, 1)));
            Assert.AreEqual(0f, grid.GeneratorKeepOutHalfExtent);
        }

        [Test]
        public void OverlapsGeneratorKeepOut_IsCenteredOnArea()
        {
            GridService grid = Create(new Vector3(10f, 0f, 10f));

            Assert.IsTrue(grid.OverlapsGeneratorKeepOut(grid.GetFootprintCells(new Vector3(10.5f, 0f, 10.5f), Vector2Int.one, 0)));
            Assert.IsFalse(grid.OverlapsGeneratorKeepOut(grid.GetFootprintCells(new Vector3(15.5f, 0f, 10.5f), Vector2Int.one, 0)));
        }

        [Test]
        public void CheckPlacement_FreeCellInArea_IsValid()
        {
            Assert.AreEqual(PlacementStatus.Valid, Create().CheckPlacement(new RectInt(5, 5, 3, 1)));
        }

        [Test]
        public void CheckPlacement_ReportsAreaBeforeGenerator()
        {
            var grid = new GridService(new FakeArea(Vector3.zero, 4f), 1f, 3f);

            Assert.AreEqual(PlacementStatus.OutsideArea, grid.CheckPlacement(new RectInt(2, 0, 3, 1)), "Hem alan dışı hem jeneratör çevresi.");
            Assert.AreEqual(PlacementStatus.NearGenerator, grid.CheckPlacement(new RectInt(0, 0, 1, 1)));
        }

        [Test]
        public void CheckPlacement_ReportsGeneratorBeforeOccupied()
        {
            GridService grid = Create();
            var cells = new RectInt(1, 1, 1, 1);
            grid.TryOccupy(cells);

            Assert.AreEqual(PlacementStatus.NearGenerator, grid.CheckPlacement(cells));
        }

        [Test]
        public void TryOccupy_ThenOverlappingPlacement_IsOccupied()
        {
            GridService grid = Create();

            Assert.IsTrue(grid.TryOccupy(new RectInt(5, 5, 3, 1)));

            Assert.AreEqual(PlacementStatus.Occupied, grid.CheckPlacement(new RectInt(7, 4, 1, 3)), "Bir hücre ortak.");
            Assert.AreEqual(PlacementStatus.Valid, grid.CheckPlacement(new RectInt(8, 5, 1, 3)), "Yan yana: hücre ortak değil.");
            Assert.AreEqual(PlacementStatus.Valid, grid.CheckPlacement(new RectInt(5, 6, 3, 1)), "Üst sıra.");
        }

        [Test]
        public void TryOccupy_MarksEveryCellOfRotatedFootprint()
        {
            GridService grid = Create();
            RectInt cells = grid.GetFootprintCells(new Vector3(-6.5f, 0f, 6.5f), new Vector2Int(3, 1), 1);

            grid.TryOccupy(cells);

            Assert.AreEqual(3, grid.OccupiedCellCount);
            Assert.IsTrue(grid.IsOccupied(new Vector2Int(-7, 5)));
            Assert.IsTrue(grid.IsOccupied(new Vector2Int(-7, 6)));
            Assert.IsTrue(grid.IsOccupied(new Vector2Int(-7, 7)));
            Assert.IsFalse(grid.IsOccupied(new Vector2Int(-6, 6)));
        }

        [Test]
        public void TryOccupy_PartialOverlap_FailsAndOccupiesNothing()
        {
            GridService grid = Create();
            grid.TryOccupy(new RectInt(5, 5, 1, 1));

            Assert.IsFalse(grid.TryOccupy(new RectInt(4, 5, 3, 1)));
            Assert.AreEqual(1, grid.OccupiedCellCount);
            Assert.IsFalse(grid.IsOccupied(new Vector2Int(4, 5)));
            Assert.IsFalse(grid.IsOccupied(new Vector2Int(6, 5)));
        }

        [Test]
        public void IsAnyOccupied_ChecksWholeRect()
        {
            GridService grid = Create();
            grid.TryOccupy(new RectInt(6, 6, 1, 1));

            Assert.IsTrue(grid.IsAnyOccupied(new RectInt(5, 5, 2, 2)));
            Assert.IsFalse(grid.IsAnyOccupied(new RectInt(4, 4, 2, 2)));
        }

        [Test]
        public void Release_FreesCellsAndReturnsCount()
        {
            GridService grid = Create();
            var cells = new RectInt(5, 5, 2, 2);
            grid.TryOccupy(cells);

            Assert.AreEqual(4, grid.Release(cells));
            Assert.AreEqual(0, grid.OccupiedCellCount);
            Assert.AreEqual(PlacementStatus.Valid, grid.CheckPlacement(cells));
            Assert.AreEqual(0, grid.Release(cells), "Boş hücre ikinci kez boşaltılmaz.");
        }

        [Test]
        public void Release_OnlyTouchesGivenCells()
        {
            GridService grid = Create();
            grid.TryOccupy(new RectInt(5, 5, 3, 1));

            Assert.AreEqual(1, grid.Release(new RectInt(7, 5, 2, 2)));
            Assert.AreEqual(2, grid.OccupiedCellCount);
        }

        [Test]
        public void Clear_FreesEverything()
        {
            GridService grid = Create();
            grid.TryOccupy(new RectInt(5, 5, 3, 1));
            grid.TryOccupy(new RectInt(-8, -8, 2, 2));

            grid.Clear();

            Assert.AreEqual(0, grid.OccupiedCellCount);
        }

        private static GridService Create(Vector3 center = default)
        {
            return new GridService(new FakeArea(center, 14f), 1f, 3f);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
            Assert.AreEqual(expected.z, actual.z, Tolerance, "z");
        }

        private sealed class FakeArea : IBaseArea
        {
            public FakeArea(Vector3 center, float radius)
            {
                Center = center;
                Radius = radius;
            }

            public Vector3 Center { get; }

            public float Radius { get; }

            public bool Contains(Vector3 position)
            {
                return BaseAreaMath.Contains(Center, Radius, position);
            }
        }
    }
}
