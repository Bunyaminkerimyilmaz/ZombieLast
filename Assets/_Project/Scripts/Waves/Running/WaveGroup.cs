using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;

namespace LastHaven.Waves
{
    /// <summary>
    /// <see cref="WaveRunner"/>'ın girdi grubu: <see cref="SpawnGroup"/>'un (veya boss'un) doğrulanmış, değişmez hali.
    /// Düşman verisi runner için opaktır; olduğu gibi <see cref="WaveSpawnRequest"/>'e aktarılır.
    /// </summary>
    public readonly struct WaveGroup
    {
        /// <param name="enemy">Düşman tipi; runner kullanmaz, isteğe aktarır.</param>
        /// <param name="count">Negatifse 0 sayılır.</param>
        /// <param name="spawnInterval">Ardışık spawn arası (sn); negatifse 0.</param>
        /// <param name="startDelay">Gece başından ilk spawn'a kadar (sn); negatifse 0.</param>
        /// <param name="directions">Spawn'ların sırayla dağıtılacağı yönler. Adet 0'dan büyükse boş olamaz.</param>
        /// <param name="isBoss">Boss spawn'ı mı (isteğe aktarılır).</param>
        public WaveGroup(EnemyData enemy, int count, float spawnInterval, float startDelay,
            IReadOnlyList<CompassDirection> directions, bool isBoss = false)
        {
            Enemy = enemy;
            Count = Math.Max(0, count);
            SpawnInterval = Math.Max(0f, spawnInterval);
            StartDelay = Math.Max(0f, startDelay);
            Directions = directions ?? Array.Empty<CompassDirection>();
            IsBoss = isBoss;
        }

        public EnemyData Enemy { get; }

        public int Count { get; }

        public float SpawnInterval { get; }

        public float StartDelay { get; }

        public IReadOnlyList<CompassDirection> Directions { get; }

        public bool IsBoss { get; }
    }
}
