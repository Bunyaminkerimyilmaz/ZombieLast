using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// <see cref="WaveSpawner"/>'ın spawn bölgesi işleri: isteğin yönündeki <see cref="SpawnZone"/>'dan rastgele nokta seçip
    /// <see cref="IEnemySpawner"/>'la düşmanı doğurur ve bölge merkezlerini verir (istihbarat okları). Spawner servisi tembel alınır:
    /// gece, sahibinin Start'ından önce başlatılsa da çalışsın.
    /// </summary>
    internal sealed class ZoneSpawner
    {
        private readonly SpawnZoneSet _zones;
        private readonly Object _context;
        private IEnemySpawner _enemySpawner;

        /// <param name="context">Uyarıların bağlamı.</param>
        public ZoneSpawner(IReadOnlyList<SpawnZone> zones, Object context)
        {
            _zones = new SpawnZoneSet(zones);
            _context = context;
        }

        /// <summary>Yönün bölge merkezi; bölge yoksa false.</summary>
        public bool TryGetCenter(CompassDirection direction, out Vector3 center)
        {
            bool found = _zones.TryGet(direction, out SpawnZone zone);
            center = found ? zone.Center : Vector3.zero;
            return found;
        }

        /// <summary>İsteğin yönündeki bölgede düşmanı doğurur. Doğan düşmanın kök objesi; doğurulamadıysa null (nedeni loglanır).</summary>
        public GameObject Spawn(WaveSpawnRequest request)
        {
            if (_enemySpawner == null && !ServiceLocator.TryGet(out _enemySpawner))
            {
                Debug.LogWarning("[Waves] IEnemySpawner bulunamadı; düşman doğurulamadı. Sahnede [Systems]/EnemySpawner var mı?", _context);
                return null;
            }

            if (!_zones.TryGet(request.Direction, out SpawnZone zone))
            {
                Debug.LogWarning($"[Waves] {request.Direction} yönünde SpawnZone yok; '{request.Enemy.Id}' doğurulamadı.", _context);
                return null;
            }

            return _enemySpawner.Spawn(request.Enemy.Id, zone.GetRandomPoint()); // Başarısızsa EnemySpawner nedenini logladı.
        }
    }
}
