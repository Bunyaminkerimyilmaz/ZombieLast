using System;
using System.Collections.Generic;

namespace LastHaven.Core
{
    /// <summary>
    /// Bir gecenin istihbarat özeti (<see cref="IWaveService.GetTonightInfo"/>). Core gece verisini bilmediği için düşmanlar
    /// id ve görünen adla verilir.
    /// </summary>
    public sealed class NightInfo
    {
        public NightInfo(int nightIndex, int nightNumber, IReadOnlyList<CompassDirection> directions,
            IReadOnlyList<NightEnemyInfo> enemies, string eventName, string eventDescription = null)
        {
            NightIndex = nightIndex;
            NightNumber = nightNumber;
            Directions = directions ?? Array.Empty<CompassDirection>();
            Enemies = enemies ?? Array.Empty<NightEnemyInfo>();
            EventName = eventName ?? string.Empty;
            EventDescription = eventDescription ?? string.Empty;

            for (int i = 0; i < Enemies.Count; i++)
            {
                TotalEnemies += Enemies[i].Count;
                HasBoss |= Enemies[i].IsBoss;
            }
        }

        /// <summary>0 tabanlı gece indeksi.</summary>
        public int NightIndex { get; }

        /// <summary>Arayüzde görünen gece numarası (1 tabanlı, gece verisinden).</summary>
        public int NightNumber { get; }

        /// <summary>Saldırı yönleri, tekrarsız ve enum sırasıyla (K, D, G, B).</summary>
        public IReadOnlyList<CompassDirection> Directions { get; }

        /// <summary>Düşman tipleri ve adetleri, gece verisindeki ilk görünme sırasıyla. Boss ayrı satırdır.</summary>
        public IReadOnlyList<NightEnemyInfo> Enemies { get; }

        /// <summary>Gece olayının görünen adı (ör. "Sis"); olay yoksa boş.</summary>
        public string EventName { get; }

        /// <summary>Gece olayının açıklaması (ör. "Görüş azalır, düşmanlar %10 hızlı"); olay yoksa boş.</summary>
        public string EventDescription { get; }

        public bool HasEvent => EventName.Length > 0;

        public bool HasBoss { get; }

        /// <summary>Gecede doğacak toplam düşman (boss dahil).</summary>
        public int TotalEnemies { get; }

        /// <summary>Bu gece bu yönden saldırı var mı.</summary>
        public bool HasDirection(CompassDirection direction)
        {
            for (int i = 0; i < Directions.Count; i++)
            {
                if (Directions[i] == direction)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary><see cref="NightInfo"/> satırı: bir düşman tipi ve o gece kaç tane geleceği.</summary>
    public readonly struct NightEnemyInfo
    {
        public NightEnemyInfo(string enemyId, string displayName, int count, bool isBoss)
        {
            EnemyId = enemyId;
            DisplayName = displayName;
            Count = count;
            IsBoss = isBoss;
        }

        /// <summary>Düşman verisinin id'si (EnemyData.Id).</summary>
        public string EnemyId { get; }

        /// <summary>Arayüzde görünen ad (EnemyData.DisplayName).</summary>
        public string DisplayName { get; }

        public int Count { get; }

        public bool IsBoss { get; }
    }
}
