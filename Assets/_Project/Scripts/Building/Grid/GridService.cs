using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa ızgarası (GDD §11, saf C#): dünya ↔ hücre dönüşümü, döndürülmüş ayak izi, dolu hücre takibi, üs alanı ve
    /// jeneratör yasak bölgesi kontrolleri.
    /// </summary>
    /// <remarks>
    /// Izgaranın orijini üs merkezidir (= jeneratör merkezi): hücre (0, 0) merkezin kuzeydoğusundaki ilk hücredir, jeneratör
    /// ızgara çizgilerinin kesiştiği noktada durur. Hücre koordinatı X = dünya X, Y = dünya Z. Ayak izi dünya eksenlerine
    /// hizalı bir <see cref="RectInt"/>'tir (90°'lik dönüşler en ve boyu yer değiştirir).
    /// </remarks>
    public sealed class GridService
    {
        // Kenarları değen dikdörtgenler çakışmış sayılmasın diye (float yuvarlaması).
        private const float OverlapEpsilon = 0.0001f;

        private readonly IBaseArea _area;
        private readonly HashSet<Vector2Int> _occupied = new HashSet<Vector2Int>();
        private readonly Vector3 _origin;
        private readonly float _cellSize;
        private readonly float _keepOutHalfExtent;

        /// <param name="area">Üs alanı; orijin bu anki merkezidir (ızgara sonradan kaymaz).</param>
        /// <param name="cellSize">Hücre kenarı (m), pozitif.</param>
        /// <param name="generatorKeepOutHalfExtent">Merkezdeki kare yasak bölgenin yarı kenarı (m); 0 = yasak yok.</param>
        public GridService(IBaseArea area, float cellSize, float generatorKeepOutHalfExtent)
        {
            if (cellSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Hücre boyu pozitif olmalı.");
            }

            _area = area ?? throw new ArgumentNullException(nameof(area));
            _cellSize = cellSize;
            _keepOutHalfExtent = Mathf.Max(0f, generatorKeepOutHalfExtent);
            _origin = area.Center;
        }

        /// <summary>Izgaranın orijini (üs merkezi, yer seviyesi).</summary>
        public Vector3 Origin => _origin;

        public float CellSize => _cellSize;

        public float GeneratorKeepOutHalfExtent => _keepOutHalfExtent;

        public int OccupiedCellCount => _occupied.Count;

        /// <summary>Dönüş adımını 0–3'e indirir (-1 → 3, 5 → 1).</summary>
        public static int NormalizeRotation(int rotationSteps)
        {
            return ((rotationSteps % 4) + 4) % 4;
        }

        /// <summary>Ayak izini 90°'lik adımlarla döndürür: tek adımda en ve boy yer değiştirir. Kenarlar en az 1'e kırpılır.</summary>
        public static Vector2Int RotateFootprint(Vector2Int footprint, int rotationSteps)
        {
            int width = Mathf.Max(1, footprint.x);
            int depth = Mathf.Max(1, footprint.y);
            return NormalizeRotation(rotationSteps) % 2 == 0 ? new Vector2Int(width, depth) : new Vector2Int(depth, width);
        }

        /// <summary>Noktanın içinde olduğu hücre (yükseklik yok sayılır). Hücre sınırı bir sonraki hücreye aittir.</summary>
        public Vector2Int WorldToCell(Vector3 world)
        {
            return new Vector2Int(
                Mathf.FloorToInt((world.x - _origin.x) / _cellSize),
                Mathf.FloorToInt((world.z - _origin.z) / _cellSize));
        }

        /// <summary>Hücrenin merkezi, orijin yüksekliğinde (dünya).</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return new Vector3(_origin.x + (cell.x + 0.5f) * _cellSize, _origin.y, _origin.z + (cell.y + 0.5f) * _cellSize);
        }

        /// <summary>
        /// Noktayı ortalayan döndürülmüş ayak izi. Tek kenarda noktanın hücresi ortadadır; çift kenarda noktaya en yakın
        /// ızgara çizgisi ortadadır (2×2, imlecin en yakın köşesine oturur).
        /// </summary>
        public RectInt GetFootprintCells(Vector3 worldPoint, Vector2Int footprint, int rotationSteps)
        {
            Vector2Int size = RotateFootprint(footprint, rotationSteps);
            float u = (worldPoint.x - _origin.x) / _cellSize;
            float v = (worldPoint.z - _origin.z) / _cellSize;
            int minX = Mathf.FloorToInt(u - size.x * 0.5f + 0.5f);
            int minY = Mathf.FloorToInt(v - size.y * 0.5f + 0.5f);
            return new RectInt(minX, minY, size.x, size.y);
        }

        /// <summary>Hücre dikdörtgeninin merkezi, orijin yüksekliğinde (dünya). Yapının pivotu buraya konur.</summary>
        public Vector3 GetFootprintCenter(RectInt cells)
        {
            Vector2 center = cells.center;
            return new Vector3(_origin.x + center.x * _cellSize, _origin.y, _origin.z + center.y * _cellSize);
        }

        /// <summary>Ayak izinin tamamı üs alanında mı (dört köşe de <see cref="IBaseArea.Contains"/>; daire dışbükey olduğu için yeterli).</summary>
        public bool IsInsideArea(RectInt cells)
        {
            float minX = _origin.x + cells.xMin * _cellSize;
            float maxX = _origin.x + cells.xMax * _cellSize;
            float minZ = _origin.z + cells.yMin * _cellSize;
            float maxZ = _origin.z + cells.yMax * _cellSize;
            float y = _origin.y;

            return _area.Contains(new Vector3(minX, y, minZ))
                   && _area.Contains(new Vector3(maxX, y, minZ))
                   && _area.Contains(new Vector3(minX, y, maxZ))
                   && _area.Contains(new Vector3(maxX, y, maxZ));
        }

        /// <summary>Ayak izi jeneratörün kare yasak bölgesine giriyor mu. Kenarı değmek girmek sayılmaz.</summary>
        public bool OverlapsGeneratorKeepOut(RectInt cells)
        {
            if (_keepOutHalfExtent <= 0f)
            {
                return false;
            }

            float limit = _keepOutHalfExtent - OverlapEpsilon;
            return cells.xMin * _cellSize < limit && cells.xMax * _cellSize > -limit
                   && cells.yMin * _cellSize < limit && cells.yMax * _cellSize > -limit;
        }

        public bool IsOccupied(Vector2Int cell)
        {
            return _occupied.Contains(cell);
        }

        /// <summary>Dikdörtgendeki hücrelerden biri dolu mu.</summary>
        public bool IsAnyOccupied(RectInt cells)
        {
            foreach (Vector2Int cell in cells.allPositionsWithin)
            {
                if (_occupied.Contains(cell))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Izgara kurallarına göre sonuç: alan dışı → jeneratör çevresi → dolu → geçerli (çakışma ve para bakılmaz).</summary>
        public PlacementStatus CheckPlacement(RectInt cells)
        {
            if (!IsInsideArea(cells))
            {
                return PlacementStatus.OutsideArea;
            }

            if (OverlapsGeneratorKeepOut(cells))
            {
                return PlacementStatus.NearGenerator;
            }

            return IsAnyOccupied(cells) ? PlacementStatus.Occupied : PlacementStatus.Valid;
        }

        /// <summary>
        /// Hücreleri doldurur. Biri bile doluysa hiçbirini doldurmaz ve false döner. Alan ve jeneratör kuralına bakmaz;
        /// önce <see cref="CheckPlacement"/> çağrılmalı.
        /// </summary>
        public bool TryOccupy(RectInt cells)
        {
            if (IsAnyOccupied(cells))
            {
                return false;
            }

            foreach (Vector2Int cell in cells.allPositionsWithin)
            {
                _occupied.Add(cell);
            }

            return true;
        }

        /// <summary>Hücreleri boşaltır; boşaltılan (dolu olan) hücre sayısını döndürür.</summary>
        public int Release(RectInt cells)
        {
            int released = 0;
            foreach (Vector2Int cell in cells.allPositionsWithin)
            {
                if (_occupied.Remove(cell))
                {
                    released++;
                }
            }

            return released;
        }

        /// <summary>Tüm hücreleri boşaltır.</summary>
        public void Clear()
        {
            _occupied.Clear();
        }
    }
}
