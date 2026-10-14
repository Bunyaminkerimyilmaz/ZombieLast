using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Proje layer'ları. İndeksler sabittir; Setup Project bunları TagManager'a aynı indekslerle yazar.
    /// </summary>
    public static class GameLayers
    {
        public const int Ground = 6;
        public const int Player = 7;
        public const int Enemy = 8;
        public const int Structure = 9;
        public const int Pickup = 10;
        public const int Obstacle = 11;
        public const int EnemyProjectile = 12;
        public const int Interactable = 13;

        public const string GroundName = "Ground";
        public const string PlayerName = "Player";
        public const string EnemyName = "Enemy";
        public const string StructureName = "Structure";
        public const string PickupName = "Pickup";
        public const string ObstacleName = "Obstacle";
        public const string EnemyProjectileName = "EnemyProjectile";
        public const string InteractableName = "Interactable";

        public const int GroundMask = 1 << Ground;
        public const int PlayerMask = 1 << Player;
        public const int EnemyMask = 1 << Enemy;
        public const int StructureMask = 1 << Structure;
        public const int PickupMask = 1 << Pickup;
        public const int ObstacleMask = 1 << Obstacle;
        public const int EnemyProjectileMask = 1 << EnemyProjectile;
        public const int InteractableMask = 1 << Interactable;

        /// <summary>Runtime NavMesh bake'ine giren layer'lar (sadece Ground ve Obstacle).</summary>
        public const int NavMeshBakeMask = GroundMask | ObstacleMask;

        /// <summary>Tüm proje layer'ları (indeks, ad). Setup Project bu listeyi kullanır.</summary>
        public static readonly IReadOnlyList<(int Index, string Name)> All = new[]
        {
            (Ground, GroundName),
            (Player, PlayerName),
            (Enemy, EnemyName),
            (Structure, StructureName),
            (Pickup, PickupName),
            (Obstacle, ObstacleName),
            (EnemyProjectile, EnemyProjectileName),
            (Interactable, InteractableName),
        };

        /// <summary>Tek layer'ın maskesi.</summary>
        public static int ToMask(int layer)
        {
            return 1 << layer;
        }

        /// <summary>Verilen layer'ları tek maskede birleştirir. Oynanış sırasında değil, kurulumda kullan.</summary>
        public static LayerMask Combine(params int[] layers)
        {
            int mask = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                mask |= 1 << layers[i];
            }

            return mask;
        }

        /// <summary>Maske bu layer'ı içeriyor mu?</summary>
        public static bool Contains(LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
