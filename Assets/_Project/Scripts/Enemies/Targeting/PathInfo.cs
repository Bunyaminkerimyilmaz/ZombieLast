using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın o anki yolunun durumu (GDD §7 kural 4 için). <see cref="IsBlocked"/>: yol yarım kaldı (NavMeshAgent
    /// <c>PathPartial</c>) ve sonu, yolun gittiği hedefe saldırı menzilinden uzak. Menzil içinde biten yarım yol tıkalı sayılmaz:
    /// hedef NavMesh dışında olsa da (ör. carve'lı barikatın merkezi) oradan vurulabilir.
    /// </summary>
    public readonly struct PathInfo
    {
        private PathInfo(bool isBlocked, Vector3 end)
        {
            IsBlocked = isBlocked;
            End = end;
        }

        /// <summary>Yol yok, tamam veya hesaplanıyor.</summary>
        public static PathInfo None => default;

        /// <summary>Yol tıkalı: hedefe ulaşmıyor.</summary>
        public bool IsBlocked { get; }

        /// <summary>Yarım yolun bittiği nokta (ajanın gidebildiği en yakın yer). Sadece <see cref="IsBlocked"/> iken anlamlı.</summary>
        public Vector3 End { get; }

        /// <summary>
        /// Yarım yoldan bilgi kurar: yolun sonu hedefin yüzeyine <paramref name="attackRange"/>'den uzaksa tıkalı.
        /// </summary>
        /// <param name="endDistanceToTarget">Yolun sonundan hedefin yüzeyine yatay uzaklık (m).</param>
        public static PathInfo FromPartialPath(Vector3 end, float endDistanceToTarget, float attackRange)
        {
            return new PathInfo(endDistanceToTarget > attackRange, end);
        }
    }
}
