using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>
    /// Sandık için nokta arar (GDD §9): halkada rastgele nokta (<see cref="LootRing"/>) → en yakın NavMesh noktası (oyuncu
    /// yürüyerek ulaşabilsin) → hâlâ halkada, diğer sandıklardan <see cref="LootConfig.MinCrateSpacing"/> uzak ve engel/yapıyla
    /// çakışmıyor (<see cref="LootConfig.BlockingMask"/>) mu. NavMesh oyun başında bake edildiği için gündüz hazırdır.
    /// </summary>
    public sealed class LootCratePlacer
    {
        private const float FullTurnDegrees = 360f;

        private readonly LootConfig _config;
        private readonly Random _random;

        public LootCratePlacer(LootConfig config, Random random)
        {
            _config = config;
            _random = random;
        }

        /// <summary>
        /// <see cref="LootConfig.PlacementAttempts"/> denemede uygun nokta (zemin) ve rastgele yön bulursa true.
        /// </summary>
        public bool TryFindPoint(Vector3 center, IReadOnlyList<Vector3> placed, out Vector3 point, out Quaternion rotation)
        {
            LootRing ring = _config.CreateRing();
            float clearance = _config.ObstacleClearance;
            for (int attempt = 0; attempt < _config.PlacementAttempts; attempt++)
            {
                Vector3 candidate = ring.Sample(center, _random);
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, _config.NavMeshSampleDistance, NavMesh.AllAreas))
                {
                    continue;
                }

                point = hit.position;
                if (!ring.Contains(center, point)
                    || !LootRing.HasSpacing(point, placed, _config.MinCrateSpacing)
                    || Physics.CheckSphere(point + Vector3.up * clearance, clearance, _config.BlockingMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                rotation = Quaternion.Euler(0f, (float)_random.NextDouble() * FullTurnDegrees, 0f);
                return true;
            }

            point = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }
    }
}
