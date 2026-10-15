using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// <see cref="NightData"/> → <see cref="WaveRunner"/> girdisi ve istihbarat özeti (<see cref="NightInfo"/>).
    /// Geçersiz gruplar (boş düşman, adet 0, yön yok) atlanır; boss tek düşmanlık bir grup olarak sona eklenir.
    /// </summary>
    public static class WavePlan
    {
        /// <summary>
        /// Gecenin geçerli gruplarını (ve varsa boss'u) <paramref name="results"/>'a ekler. Atlanan her grup için uyarı loglar.
        /// </summary>
        public static void BuildGroups(NightData night, List<WaveGroup> results)
        {
            IReadOnlyList<SpawnGroup> groups = night.Groups;
            for (int i = 0; i < groups.Count; i++)
            {
                SpawnGroup group = groups[i];
                if (TryGetProblem(group, out string problem))
                {
                    Debug.LogWarning($"[Waves] {night.name}: {i}. grup atlandı ({problem}).", night);
                    continue;
                }

                results.Add(new WaveGroup(group.Enemy, group.Count, group.SpawnInterval, group.StartDelay, group.Directions));
            }

            if (night.BossData != null)
            {
                results.Add(new WaveGroup(night.BossData, 1, 0f, night.BossStartDelay,
                    new[] { night.BossDirection }, isBoss: true));
            }
        }

        /// <summary>
        /// İstihbarat özeti: yönlerin birleşimi (enum sırasıyla), tip başına toplam adet (ilk görünme sırasıyla; boss ayrı satır)
        /// ve olay adı. Geçersiz gruplar sayılmaz, uyarı loglanmaz.
        /// </summary>
        public static NightInfo BuildInfo(int nightIndex, NightData night)
        {
            var hasDirection = new bool[CompassDirections.All.Count];
            var enemyTypes = new List<EnemyData>();
            var counts = new List<int>();

            IReadOnlyList<SpawnGroup> groups = night.Groups;
            for (int i = 0; i < groups.Count; i++)
            {
                SpawnGroup group = groups[i];
                if (TryGetProblem(group, out _))
                {
                    continue;
                }

                MarkDirections(group.Directions, hasDirection);
                int typeIndex = enemyTypes.IndexOf(group.Enemy);
                if (typeIndex < 0)
                {
                    enemyTypes.Add(group.Enemy);
                    counts.Add(group.Count);
                }
                else
                {
                    counts[typeIndex] += group.Count;
                }
            }

            var enemies = new List<NightEnemyInfo>(enemyTypes.Count + 1);
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                enemies.Add(new NightEnemyInfo(enemyTypes[i].Id, enemyTypes[i].DisplayName, counts[i], isBoss: false));
            }

            if (night.BossData != null)
            {
                MarkDirection(night.BossDirection, hasDirection);
                enemies.Add(new NightEnemyInfo(night.BossData.Id, night.BossData.DisplayName, 1, isBoss: true));
            }

            var directions = new List<CompassDirection>(CompassDirections.All.Count);
            for (int i = 0; i < CompassDirections.All.Count; i++)
            {
                if (hasDirection[i])
                {
                    directions.Add(CompassDirections.All[i]);
                }
            }

            string eventName = night.Modifier != null ? night.Modifier.DisplayName : string.Empty;
            return new NightInfo(nightIndex, night.NightNumber, directions, enemies, eventName);
        }

        private static bool TryGetProblem(SpawnGroup group, out string problem)
        {
            SpawnGroupProblem result = SpawnGroupValidation.GetProblem(group);
            problem = result != SpawnGroupProblem.None ? SpawnGroupValidation.Describe(result) : null;
            return problem != null;
        }

        private static void MarkDirections(IReadOnlyList<CompassDirection> directions, bool[] hasDirection)
        {
            for (int i = 0; i < directions.Count; i++)
            {
                MarkDirection(directions[i], hasDirection);
            }
        }

        private static void MarkDirection(CompassDirection direction, bool[] hasDirection)
        {
            int index = (int)direction;
            if (index >= 0 && index < hasDirection.Length)
            {
                hasDirection[index] = true;
            }
        }
    }
}
