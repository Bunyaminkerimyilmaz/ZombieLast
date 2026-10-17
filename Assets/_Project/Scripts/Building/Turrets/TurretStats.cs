using System;
using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>
    /// Taretin o anki etkili değerleri (saf, GDD §11): seviyenin mutlak statı (<see cref="BuildableLevel"/>) × run çarpanları
    /// (<see cref="IRunModifiers"/>: <see cref="StatType.TurretDamage"/>, <see cref="StatType.TurretFireRate"/>,
    /// <see cref="StatType.TurretRange"/>). Seviyede yazılmayan stat 0 sayılır. Çarpan servisi yoksa taban değerler geçerlidir.
    /// </summary>
    public readonly struct TurretStats
    {
        /// <summary>Atış hızının alt sınırı (atış/sn): çarpanlarla 0'a inse de aralık sonsuz olmasın.</summary>
        public const float MinFireRate = 0.01f;

        public TurretStats(float damage, float fireRate, float range)
        {
            Damage = Math.Max(0f, damage);
            FireRate = Math.Max(MinFireRate, fireRate);
            Range = Math.Max(0f, range);
        }

        /// <summary>Atış başına hasar.</summary>
        public float Damage { get; }

        /// <summary>Atış/sn (en az <see cref="MinFireRate"/>).</summary>
        public float FireRate { get; }

        /// <summary>Menzil (m).</summary>
        public float Range { get; }

        /// <summary>İki atış arası süre (sn).</summary>
        public float FireInterval => 1f / FireRate;

        /// <param name="modifiers">Null olabilir (taban değerler).</param>
        public static TurretStats From(BuildableLevel level, IRunModifiers modifiers)
        {
            if (level == null)
            {
                return new TurretStats(0f, MinFireRate, 0f);
            }

            return new TurretStats(
                Apply(modifiers, StatType.TurretDamage, level.GetStat(BuildableStat.Damage, 0f)),
                Apply(modifiers, StatType.TurretFireRate, level.GetStat(BuildableStat.FireRate, 0f)),
                Apply(modifiers, StatType.TurretRange, level.GetStat(BuildableStat.Range, 0f)));
        }

        /// <summary>
        /// Seviyede menzil statı varsa etkili menzili verir (inşa modundaki menzil dairesi). Menzili olmayan yapılar (barikat,
        /// tuzak) false döner.
        /// </summary>
        public static bool TryGetRange(BuildableLevel level, IRunModifiers modifiers, out float range)
        {
            if (level == null || !level.TryGetStat(BuildableStat.Range, out float baseRange))
            {
                range = 0f;
                return false;
            }

            range = Math.Max(0f, Apply(modifiers, StatType.TurretRange, baseRange));
            return true;
        }

        private static float Apply(IRunModifiers modifiers, StatType stat, float baseValue)
        {
            return modifiers != null ? modifiers.Get(stat, baseValue) : baseValue;
        }
    }
}
