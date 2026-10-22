using System;
using System.Collections.Generic;
using System.Text;

namespace LastHaven.Editor
{
    /// <summary>
    /// Bir gecenin denge toplamları (saf): gruplar (olay çarpanı uygulanmış adetle) eklenir; toplam düşman canı, taban para ödülü
    /// (gecenin jeton çarpanıyla; kombo ve perk hariç), XP, son spawn anı ve içerik özeti hesaplanır. Para kesirli toplanır, sonunda
    /// yuvarlanır.
    /// </summary>
    public sealed class NightTotals
    {
        private readonly List<string> _order = new List<string>();
        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly float _coinMultiplier;
        private double _money;

        /// <param name="coinMultiplier">Gece olayının jeton çarpanı (Kan Ayı 2); negatifse 0.</param>
        public NightTotals(float coinMultiplier = 1f)
        {
            _coinMultiplier = Math.Max(0f, coinMultiplier);
        }

        /// <summary>Boss dahil düşman sayısı.</summary>
        public int EnemyCount { get; private set; }

        public int BossCount { get; private set; }

        public double TotalHealth { get; private set; }

        /// <summary>Taban ödül × jeton çarpanı, en yakın tam sayıya yuvarlı.</summary>
        public int TotalMoney => (int)Math.Round(_money, MidpointRounding.AwayFromZero);

        public int TotalXp { get; private set; }

        /// <summary>Gece başından son spawn'a kadar geçen süre (sn; canlı tavanı yüzünden bekleme hariç).</summary>
        public float SpawnDuration { get; private set; }

        /// <summary>Saniyede ortalama gelen can (toplam can / spawn süresi): dalgaya yetişmek için gereken en az DPS. Süre 0 ise 0.</summary>
        public double HealthPerSecond => SpawnDuration > 0f ? TotalHealth / SpawnDuration : 0d;

        /// <summary>
        /// Bir grup ekler. <paramref name="startDelay"/> + (adet − 1) × <paramref name="interval"/> son spawn anıdır.
        /// Adet 0 veya altındaysa yok sayılır.
        /// </summary>
        public void AddGroup(string displayName, int count, float health, int moneyReward, int xpReward, float startDelay, float interval,
            bool isBoss = false)
        {
            if (count <= 0)
            {
                return;
            }

            EnemyCount += count;
            if (isBoss)
            {
                BossCount += count;
            }

            TotalHealth += (double)count * health;
            _money += (double)count * moneyReward * _coinMultiplier;
            TotalXp += count * xpReward;
            SpawnDuration = Math.Max(SpawnDuration, startDelay + (count - 1) * Math.Max(0f, interval));

            string key = isBoss ? "Boss: " + displayName : displayName;
            if (_counts.TryGetValue(key, out int existing))
            {
                _counts[key] = existing + count;
            }
            else
            {
                _counts.Add(key, count);
                _order.Add(key);
            }
        }

        /// <summary>İçerik özeti, ilk görünme sırasıyla: "30 Yürüyen, 8 Koşucu, Boss: Brute". Boss adedi 1 ise sayı yazılmaz.</summary>
        public string DescribeContents()
        {
            var text = new StringBuilder();
            for (int i = 0; i < _order.Count; i++)
            {
                if (i > 0)
                {
                    text.Append(", ");
                }

                string key = _order[i];
                int count = _counts[key];
                bool singleBoss = count == 1 && key.StartsWith("Boss: ", StringComparison.Ordinal);
                if (!singleBoss)
                {
                    text.Append(count).Append(' ');
                }

                text.Append(key);
            }

            return text.ToString();
        }
    }
}
