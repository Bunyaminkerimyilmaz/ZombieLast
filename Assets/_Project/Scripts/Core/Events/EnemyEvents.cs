using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Bir düşman öldüğünde gönderilir (ölüm anında, cesedi batmadan önce). Jeton, XP, kombo ve istatistik bunu dinler.
    /// </summary>
    public readonly struct EnemyKilledEvent : IEvent
    {
        public EnemyKilledEvent(Vector3 position, string enemyId, int moneyReward, int xpReward, bool killedByPlayer)
        {
            Position = position;
            EnemyId = enemyId;
            MoneyReward = moneyReward;
            XpReward = xpReward;
            KilledByPlayer = killedByPlayer;
        }

        /// <summary>Düşmanın öldüğü yer (ayak, dünya).</summary>
        public Vector3 Position { get; }

        /// <summary>Düşman tipinin id'si (EnemyData.Id).</summary>
        public string EnemyId { get; }

        /// <summary>Taban para ödülü (kombo ve perk çarpanları uygulanmamış).</summary>
        public int MoneyReward { get; }

        public int XpReward { get; }

        /// <summary>Öldüren hasarın kaynağı oyuncuysa true (taret, tuzak, patlama zinciri değil).</summary>
        public bool KilledByPlayer { get; }
    }
}
