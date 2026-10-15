using LastHaven.Core;
using LastHaven.Enemies;

namespace LastHaven.Waves
{
    /// <summary><see cref="WaveRunner"/>'ın dışarı verdiği tek düşmanlık spawn isteği.</summary>
    public readonly struct WaveSpawnRequest
    {
        public WaveSpawnRequest(EnemyData enemy, CompassDirection direction, int groupIndex, int indexInGroup, bool isBoss)
        {
            Enemy = enemy;
            Direction = direction;
            GroupIndex = groupIndex;
            IndexInGroup = indexInGroup;
            IsBoss = isBoss;
        }

        public EnemyData Enemy { get; }

        /// <summary>Düşmanın doğacağı spawn bölgesinin yönü.</summary>
        public CompassDirection Direction { get; }

        /// <summary>Grubun <see cref="WaveRunner.Start"/>'a verilen listedeki indeksi.</summary>
        public int GroupIndex { get; }

        /// <summary>Grup içindeki sırası (0 tabanlı).</summary>
        public int IndexInGroup { get; }

        public bool IsBoss { get; }
    }
}
