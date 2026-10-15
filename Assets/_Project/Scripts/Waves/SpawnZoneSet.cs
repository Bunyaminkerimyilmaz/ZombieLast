using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>Yön → <see cref="SpawnZone"/> eşlemesi. Aynı yönde birden fazla bölge varsa ilki kullanılır (uyarıyla).</summary>
    public sealed class SpawnZoneSet
    {
        private readonly SpawnZone[] _byDirection = new SpawnZone[CompassDirections.All.Count];

        public SpawnZoneSet(IReadOnlyList<SpawnZone> zones)
        {
            if (zones == null)
            {
                return;
            }

            for (int i = 0; i < zones.Count; i++)
            {
                SpawnZone zone = zones[i];
                int index = zone != null ? (int)zone.Direction : -1;
                if (index < 0 || index >= _byDirection.Length)
                {
                    continue;
                }

                if (_byDirection[index] != null)
                {
                    Debug.LogWarning($"[Waves] {zone.Direction} yönünde birden fazla SpawnZone var; '{zone.name}' kullanılmayacak.", zone);
                    continue;
                }

                _byDirection[index] = zone;
            }
        }

        /// <summary>Yönün bölgesi; yoksa (veya yok edildiyse) false.</summary>
        public bool TryGet(CompassDirection direction, out SpawnZone zone)
        {
            int index = (int)direction;
            zone = index >= 0 && index < _byDirection.Length ? _byDirection[index] : null;
            return zone != null;
        }
    }
}
