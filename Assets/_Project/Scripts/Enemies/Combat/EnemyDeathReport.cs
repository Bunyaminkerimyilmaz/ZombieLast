using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>Düşman ölümünün duyurusu (<see cref="EnemyKilledEvent"/>): ödüller data'dan, öldüren oyuncu mu hasar kaynağından.</summary>
    public static class EnemyDeathReport
    {
        /// <param name="position">Ölüm noktası, zemin yüzeyinde (jetonlar buraya düşer).</param>
        public static void Raise(EnemyData data, Vector3 position, in DamageInfo killingBlow)
        {
            GameObject source = killingBlow.Source;
            bool killedByPlayer = source != null && source.CompareTag(GameTags.Player);
            EventBus<EnemyKilledEvent>.Raise(new EnemyKilledEvent(position, data.Id, data.MoneyReward, data.XpReward, killedByPlayer));
        }
    }
}
