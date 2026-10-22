using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Combat;
using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Enemies;
using LastHaven.GameFlow;
using LastHaven.Progression;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Denge tablosunun verisi: <c>Data/</c> altındaki SO'lar bir kez yüklenir, gece toplamları (<see cref="NightTotals"/>) bir kez
    /// hesaplanır; bölümler (<see cref="IBalanceSection"/>) buradan okur. Bulunamayan tekil asset null'dır (bölüm "bulunamadı" yazar).
    /// </summary>
    public sealed class BalanceSheetContext
    {
        public const string DataFolder = "Assets/_Project/Data";

        private readonly List<NightRow> _nights = new List<NightRow>();
        private readonly Dictionary<EnemyData, int> _firstNight = new Dictionary<EnemyData, int>();

        public BalanceSheetContext()
        {
            Waves = LoadFirst<WaveConfig>();
            Enemies = LoadEnemies();
            Weapons = LoadAll<WeaponData>();
            Buildables = LoadAll<BuildableData>();
            Shop = LoadFirst<ShopCatalog>();
            Economy = LoadFirst<EconomyConfig>();
            Game = LoadFirst<GameConfig>();
            Loot = LoadFirst<LootConfig>();
            Wanderers = LoadFirst<DayWanderersConfig>();
            Generator = LoadFirst<GeneratorData>();
            Progression = LoadFirst<ProgressionConfig>();
            Perks = LoadFirst<PerkCatalog>();
            BuildNights();
        }

        public WaveConfig Waves { get; }

        /// <summary>EnemySpawnerConfig sırasıyla (yoksa yol sırasıyla) tüm düşman verileri.</summary>
        public IReadOnlyList<EnemyData> Enemies { get; }

        public IReadOnlyList<WeaponData> Weapons { get; }

        public IReadOnlyList<BuildableData> Buildables { get; }

        public ShopCatalog Shop { get; }

        public EconomyConfig Economy { get; }

        public GameConfig Game { get; }

        public LootConfig Loot { get; }

        public DayWanderersConfig Wanderers { get; }

        public GeneratorData Generator { get; }

        public ProgressionConfig Progression { get; }

        public PerkCatalog Perks { get; }

        /// <summary>WaveConfig sırasıyla gecelerin toplamları (boş gece elemanı atlanır).</summary>
        public IReadOnlyList<NightRow> Nights => _nights;

        /// <summary>Düşmanın ilk göründüğü gece numarası (boss dahil); hiç yoksa 0.</summary>
        public int FirstNightOf(EnemyData enemy)
        {
            return enemy != null && _firstNight.TryGetValue(enemy, out int number) ? number : 0;
        }

        /// <summary><see cref="DataFolder"/> altındaki <typeparamref name="T"/> asset'leri, yol sırasıyla.</summary>
        public static List<T> LoadAll<T>() where T : Object
        {
            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { DataFolder }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            paths.Sort(System.StringComparer.Ordinal);
            var assets = new List<T>(paths.Count);
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

        private static T LoadFirst<T>() where T : Object
        {
            List<T> assets = LoadAll<T>();
            return assets.Count > 0 ? assets[0] : null;
        }

        private static List<EnemyData> LoadEnemies()
        {
            var ordered = new List<EnemyData>();
            var spawner = LoadFirst<EnemySpawnerConfig>();
            if (spawner != null)
            {
                foreach (EnemyPoolEntry entry in spawner.Enemies)
                {
                    if (entry.Data != null && !ordered.Contains(entry.Data))
                    {
                        ordered.Add(entry.Data);
                    }
                }
            }

            foreach (EnemyData enemy in LoadAll<EnemyData>())
            {
                if (!ordered.Contains(enemy))
                {
                    ordered.Add(enemy);
                }
            }

            return ordered;
        }

        private void BuildNights()
        {
            if (Waves == null)
            {
                return;
            }

            var groups = new List<WaveGroup>();
            for (int i = 0; i < Waves.Nights.Count; i++)
            {
                NightData night = Waves.Nights[i];
                if (night == null)
                {
                    continue;
                }

                groups.Clear();
                WavePlan.BuildGroups(night, groups);
                var totals = new NightTotals(night.Modifier != null ? night.Modifier.CoinMultiplier : 1f);
                foreach (WaveGroup group in groups)
                {
                    EnemyData enemy = group.Enemy;
                    totals.AddGroup(enemy.DisplayName, group.Count, enemy.MaxHealth, enemy.MoneyReward, enemy.XpReward,
                        group.StartDelay, group.SpawnInterval, group.IsBoss);
                    if (!_firstNight.ContainsKey(enemy))
                    {
                        _firstNight.Add(enemy, night.NightNumber);
                    }
                }

                _nights.Add(new NightRow(night, WavePlan.BuildInfo(i, night), totals));
            }
        }

        /// <summary>Bir gecenin verisi, istihbarat özeti ve toplamları.</summary>
        public sealed class NightRow
        {
            public NightRow(NightData data, NightInfo info, NightTotals totals)
            {
                Data = data;
                Info = info;
                Totals = totals;
            }

            public NightData Data { get; }

            public NightInfo Info { get; }

            public NightTotals Totals { get; }

            public int Number => Data.NightNumber;
        }
    }
}
