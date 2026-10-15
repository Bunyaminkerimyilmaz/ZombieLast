using System.Collections.Generic;
using LastHaven.Waves;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>
    /// Gece verisi kuralları (GDD §8). Validate Project ve EditMode testleri aynı kuralları kullanır.
    /// Grup kuralı runtime'la ortaktır (<see cref="SpawnGroupValidation"/>): runtime geçersiz grubu uyarıyla atlar, burada hata sayılır.
    /// </summary>
    public static class NightDataValidator
    {
        /// <summary>
        /// Hatalar: gecede hiç grup (ve boss) yok; grup boş, düşman verisi yok, adet ≤ 0 veya yön yok.
        /// </summary>
        public static void ValidateNight(NightData night, ValidationReport report)
        {
            IReadOnlyList<SpawnGroup> groups = night.Groups;
            if (groups.Count == 0 && night.BossData == null)
            {
                report.Error($"{night.name}: hiç grup yok (gece başlar başlamaz biter).", night);
            }

            for (int i = 0; i < groups.Count; i++)
            {
                SpawnGroupProblem problem = SpawnGroupValidation.GetProblem(groups[i]);
                if (problem != SpawnGroupProblem.None)
                {
                    report.Error($"{night.name}: grup [{i}] {SpawnGroupValidation.Describe(problem)}.", night);
                }
            }
        }

        /// <summary>
        /// Hatalar: gece listesi boş, boş satır. Uyarılar: gece numarası listedeki sırayla uyuşmuyor, aynı gece iki kez listede.
        /// </summary>
        public static void ValidateConfig(WaveConfig config, ValidationReport report)
        {
            IReadOnlyList<NightData> nights = config.Nights;
            if (nights.Count == 0)
            {
                report.Error($"{config.name}: gece listesi boş.", config);
                return;
            }

            for (int i = 0; i < nights.Count; i++)
            {
                NightData night = nights[i];
                if (night == null)
                {
                    report.Error($"{config.name}: gece [{i}] boş (NightData atanmamış).", config);
                    continue;
                }

                if (night.NightNumber != i + 1)
                {
                    report.Warning($"{config.name}: gece [{i}] '{night.name}' numarası {night.NightNumber}, sırası {i + 1}.", config);
                }

                for (int j = 0; j < i; j++)
                {
                    if (nights[j] == night)
                    {
                        report.Warning($"{config.name}: '{night.name}' listede iki kez ([{j}] ve [{i}]).", config);
                        break;
                    }
                }
            }
        }

        /// <summary>Projedeki tüm NightData asset'lerinin yolları, sıralı.</summary>
        public static List<string> FindAllNightPaths()
        {
            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(NightData), new[] { "Assets" }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            paths.Sort(string.CompareOrdinal);
            return paths;
        }
    }
}
