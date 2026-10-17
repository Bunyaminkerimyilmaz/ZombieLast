using System;
using System.Collections.Generic;
using LastHaven.Enemies;
using LastHaven.Waves;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>
    /// Validate Project bölümü: projedeki tüm EnemyData'lar, prefab'ları ve <c>EnemySpawnerConfig</c>
    /// (<see cref="EnemyLinksValidator"/>). Kullanım bilgisi gece verisinden (gruplar, boss) ve gündüz gezginlerinden.
    /// </summary>
    public sealed class EnemyLinksValidationStep : IValidationStep
    {
        public int Order => 700;

        public string Name => "Düşman verisi ve prefab bağlantıları";

        public void Validate(ValidationReport report)
        {
            List<EnemyData> enemies = LoadAll<EnemyData>();
            var config = AssetDatabase.LoadAssetAtPath<EnemySpawnerConfig>(EnemiesGenerator.ConfigPath);
            Dictionary<EnemyData, string> usedBy = CollectUsage();

            EnemyLinksValidator.Validate(enemies, config, usedBy, report);
            report.Note($"{enemies.Count} EnemyData, prefab'ları ve {EnemiesGenerator.ConfigPath} kontrol edildi; " +
                        $"{usedBy.Count} tip gece/gezgin verisinde kullanılıyor.");
        }

        private static Dictionary<EnemyData, string> CollectUsage()
        {
            var usage = new Dictionary<EnemyData, List<string>>();
            foreach (NightData night in LoadAll<NightData>())
            {
                foreach (SpawnGroup group in night.Groups)
                {
                    AddUse(usage, group?.Enemy, night.name);
                }

                AddUse(usage, night.BossData, night.name);
            }

            var wanderers = AssetDatabase.LoadAssetAtPath<DayWanderersConfig>(DayWanderersGenerator.ConfigPath);
            if (wanderers != null)
            {
                AddUse(usage, wanderers.Enemy, wanderers.name);
            }

            var usedBy = new Dictionary<EnemyData, string>();
            foreach (KeyValuePair<EnemyData, List<string>> pair in usage)
            {
                usedBy.Add(pair.Key, string.Join(", ", pair.Value));
            }

            return usedBy;
        }

        private static void AddUse(Dictionary<EnemyData, List<string>> usage, EnemyData data, string user)
        {
            if (data == null)
            {
                return;
            }

            if (!usage.TryGetValue(data, out List<string> users))
            {
                users = new List<string>();
                usage.Add(data, users);
            }

            if (!users.Contains(user))
            {
                users.Add(user);
            }
        }

        private static List<T> LoadAll<T>() where T : UnityEngine.Object
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets" }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            var assets = new List<T>();
            foreach (string path in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }
}
