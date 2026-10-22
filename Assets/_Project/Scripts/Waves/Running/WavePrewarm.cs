using System;
using System.Collections.Generic;
using LastHaven.Enemies;

namespace LastHaven.Waves
{
    /// <summary>
    /// Dalgaların havuz ihtiyacı (saf C#): tip başına bir gecede en çok kaç düşman olabileceği. Gece içindeki aynı tipin grupları
    /// toplanır (olay çarpanlı adetler, <see cref="WavePlan.BuildGroups"/>), canlı düşman tavanıyla sınırlanır ve gecelerin en büyüğü
    /// alınır. Oynanışta havuz büyümesin (Instantiate = GC + takılma) diye oyun başında bu kadar örnek hazırlanır.
    /// </summary>
    public static class WavePrewarm
    {
        /// <summary>
        /// Bir gecenin gruplarını işler: <paramref name="needs"/>'teki her tipin değeri, bu gecedeki toplamı (en çok
        /// <paramref name="maxAlive"/>) daha büyükse güncellenir. <paramref name="scratch"/> gece içi toplamlar için (temizlenir).
        /// </summary>
        public static void AddNight(IReadOnlyList<WaveGroup> groups, int maxAlive, Dictionary<EnemyData, int> needs,
            Dictionary<EnemyData, int> scratch)
        {
            if (groups == null)
            {
                throw new ArgumentNullException(nameof(groups));
            }

            scratch.Clear();
            for (int i = 0; i < groups.Count; i++)
            {
                WaveGroup group = groups[i];
                if (group.Enemy == null || group.Count <= 0)
                {
                    continue;
                }

                scratch.TryGetValue(group.Enemy, out int sum);
                scratch[group.Enemy] = sum + group.Count;
            }

            int cap = Math.Max(1, maxAlive);
            foreach (KeyValuePair<EnemyData, int> pair in scratch)
            {
                int need = Math.Min(pair.Value, cap);
                if (!needs.TryGetValue(pair.Key, out int current) || need > current)
                {
                    needs[pair.Key] = need;
                }
            }
        }

        /// <summary>Gündüz gezginleri geceye katıldığı için o tipin ihtiyacına en fazla gezgin sayısı eklenir.</summary>
        public static void AddWanderers(EnemyData wanderer, int maxWanderers, Dictionary<EnemyData, int> needs)
        {
            if (wanderer == null || maxWanderers <= 0)
            {
                return;
            }

            needs.TryGetValue(wanderer, out int current);
            needs[wanderer] = current + maxWanderers;
        }
    }
}
