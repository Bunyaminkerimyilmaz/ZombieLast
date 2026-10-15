using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yerleşmiş yapıların kaydı (saf C#): kök obje → kayıt ve hücre → kök obje. İnşa modunda imlecin altındaki yapıyı
    /// (hover) ve satış/yıkılmada boşalacak hücreleri bulmak için. Hücre doluluğu (yerleşim kuralı) <see cref="GridService"/>'te.
    /// </summary>
    public sealed class PlacedStructureMap
    {
        private readonly Dictionary<GameObject, PlacedStructure> _byObject = new Dictionary<GameObject, PlacedStructure>();
        private readonly Dictionary<Vector2Int, GameObject> _byCell = new Dictionary<Vector2Int, GameObject>();

        public int Count => _byObject.Count;

        /// <summary>Yapıyı ve hücrelerini ekler. Obje zaten kayıtlıysa veya hücrelerden biri başka yapınınsa hiçbir şey yapmaz, false.</summary>
        public bool Add(GameObject structure, PlacedStructure placed)
        {
            if (structure == null)
            {
                throw new ArgumentNullException(nameof(structure));
            }

            if (_byObject.ContainsKey(structure))
            {
                return false;
            }

            foreach (Vector2Int cell in placed.Cells.allPositionsWithin)
            {
                if (_byCell.ContainsKey(cell))
                {
                    return false;
                }
            }

            _byObject.Add(structure, placed);
            foreach (Vector2Int cell in placed.Cells.allPositionsWithin)
            {
                _byCell.Add(cell, structure);
            }

            return true;
        }

        /// <summary>Yapıyı ve hücrelerini siler; kayıtlı değilse false.</summary>
        public bool Remove(GameObject structure, out PlacedStructure placed)
        {
            if (structure == null || !_byObject.TryGetValue(structure, out placed))
            {
                placed = default;
                return false;
            }

            _byObject.Remove(structure);
            foreach (Vector2Int cell in placed.Cells.allPositionsWithin)
            {
                _byCell.Remove(cell);
            }

            return true;
        }

        public bool TryGet(GameObject structure, out PlacedStructure placed)
        {
            placed = default;
            return structure != null && _byObject.TryGetValue(structure, out placed);
        }

        /// <summary>Obje veya üst objelerinden biri kayıtlı bir yapıysa onun kaydı (ışın isabeti çocuk collider'a gelse de bulunur).</summary>
        public bool TryFind(Transform hit, out PlacedStructure placed)
        {
            for (Transform current = hit; current != null; current = current.parent)
            {
                if (_byObject.TryGetValue(current.gameObject, out placed))
                {
                    return true;
                }
            }

            placed = default;
            return false;
        }

        /// <summary>Hücreyi kaplayan yapı.</summary>
        public bool TryGetAt(Vector2Int cell, out GameObject structure, out PlacedStructure placed)
        {
            if (_byCell.TryGetValue(cell, out structure))
            {
                placed = _byObject[structure];
                return true;
            }

            placed = default;
            return false;
        }
    }
}
