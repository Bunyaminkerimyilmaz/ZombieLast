using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// GDD §8 gece tablosu: 10 gecenin yönleri, düşman adetleri, gece olayı ve boss'u. İçerik string id ile tanımlanır; projede o id'li
    /// EnemyData / NightModifierData yoksa üretici o satırı atlar ve TODO loglar. Gün 12'de asset'ler gelince bir sonraki
    /// Generate Default Data eksikleri kendiliğinden ekler (<see cref="NightDataFiller"/>).
    /// Zamanlama GDD'de tip başına yok; <see cref="Profiles"/>'tan hesaplanır.
    /// </summary>
    public static class NightDefinitions
    {
        // Gün 12'de üretilecek içeriğin id'leri. 12.1–12.3 asset'leri bu id'lerle oluşturmalı, yoksa geceler bağlanmaz.
        public const string SpitterId = "spitter";
        public const string ExploderId = "exploder";
        public const string BruteId = "brute";
        public const string BruteFinalId = "brute_final";
        public const string FogId = "fog";
        public const string BloodMoonId = "blood_moon";
        public const string StormId = "storm";

        private const CompassDirection N = CompassDirection.North;
        private const CompassDirection E = CompassDirection.East;
        private const CompassDirection S = CompassDirection.South;
        private const CompassDirection W = CompassDirection.West;

        private static readonly CompassDirection[] AllDirections = { N, E, S, W };

        private static readonly string Walker = EnemyDefinitions.Walker.Id;
        private static readonly string Runner = EnemyDefinitions.Runner.Id;
        private static readonly string Tank = EnemyDefinitions.Tank.Id;

        /// <summary>
        /// Tip başına spawn aralığı ve gecedeki yeri. GDD §8: aralık 0.6–1.2 sn (hafif tip sık, ağır tip seyrek);
        /// ağır/özel tipler (Tank, Tükürücü, Patlayan) gecenin ortasında gelir.
        /// </summary>
        public static readonly IReadOnlyList<WaveEnemyProfile> Profiles = new[]
        {
            new WaveEnemyProfile(Walker, 0.7f, WavePlacement.Front),
            new WaveEnemyProfile(Runner, 0.9f, WavePlacement.Front),
            new WaveEnemyProfile(Tank, 1.2f, WavePlacement.Middle),
            new WaveEnemyProfile(SpitterId, 1f, WavePlacement.Middle),
            new WaveEnemyProfile(ExploderId, 1f, WavePlacement.Middle),
        };

        public static readonly IReadOnlyList<NightDefinition> All = new[]
        {
            Night(1, new[] { N }, null, null, (Walker, 20)),
            Night(2, new[] { N, E }, null, null, (Walker, 30), (Runner, 8)),
            Night(3, new[] { N, E, S }, null, null, (Walker, 35), (Runner, 15), (Tank, 2)),
            Night(4, new[] { E, W }, FogId, null, (Walker, 40), (Runner, 15), (Tank, 3), (SpitterId, 6)),
            Night(5, AllDirections, BloodMoonId, BruteId, (Walker, 40), (Runner, 10)),
            Night(6, new[] { N, S }, null, null, (Walker, 50), (Runner, 20), (Tank, 4), (ExploderId, 8)),
            Night(7, new[] { E, S, W }, StormId, null, (Walker, 55), (Runner, 25), (Tank, 5), (SpitterId, 10), (ExploderId, 8)),
            Night(8, AllDirections, null, null, (Walker, 70), (Runner, 30), (Tank, 6), (SpitterId, 12), (ExploderId, 12)),
            Night(9, AllDirections, FogId, null, (Walker, 80), (Runner, 35), (Tank, 8), (SpitterId, 15), (ExploderId, 15)),
            Night(10, AllDirections, BloodMoonId, BruteFinalId, (Walker, 60), (Runner, 30), (Tank, 6), (ExploderId, 10)),
        };

        /// <summary>Tipin profili; tabloda yoksa <see cref="ArgumentException"/>.</summary>
        public static WaveEnemyProfile GetProfile(string enemyId)
        {
            for (int i = 0; i < Profiles.Count; i++)
            {
                if (string.Equals(Profiles[i].EnemyId, enemyId, StringComparison.Ordinal))
                {
                    return Profiles[i];
                }
            }

            throw new ArgumentException($"'{enemyId}' için dalga profili yok.", nameof(enemyId));
        }

        /// <summary>Bir grubun ilk ve son spawn'ı arasındaki süre (sn).</summary>
        public static float SpawnWindow(int count, float spawnInterval)
        {
            return Mathf.Max(0, count - 1) * spawnInterval;
        }

        private static NightDefinition Night(int number, CompassDirection[] directions, string modifierId, string bossId,
            params (string EnemyId, int Count)[] content)
        {
            // Öndeki tiplerin en uzun penceresi gecenin ana akışıdır; ortadakiler (ve boss) bu pencerenin ortasına oturur.
            float mainWindow = 0f;
            foreach ((string enemyId, int count) in content)
            {
                WaveEnemyProfile profile = GetProfile(enemyId);
                if (profile.Placement == WavePlacement.Front)
                {
                    mainWindow = Mathf.Max(mainWindow, SpawnWindow(count, profile.SpawnInterval));
                }
            }

            var groups = new List<NightGroupDefinition>(content.Length);
            foreach ((string enemyId, int count) in content)
            {
                WaveEnemyProfile profile = GetProfile(enemyId);
                float delay = profile.Placement == WavePlacement.Middle
                    ? CenterIn(mainWindow, SpawnWindow(count, profile.SpawnInterval))
                    : 0f;
                groups.Add(new NightGroupDefinition(enemyId, count, profile.SpawnInterval, delay, directions));
            }

            return new NightDefinition(number, directions, groups, modifierId, bossId, CenterIn(mainWindow, 0f), directions[0]);
        }

        /// <summary>Kendi penceresi <paramref name="own"/> olan grubu <paramref name="window"/>'un ortasına oturtan gecikme (0.1 sn'ye yuvarlı).</summary>
        private static float CenterIn(float window, float own)
        {
            const float Step = 0.1f;
            return Mathf.Round(Mathf.Max(0f, (window - own) * 0.5f) / Step) * Step;
        }
    }

    /// <summary>Tipin gecedeki yeri: öndekiler gece başında, ortadakiler ana akışın ortasında başlar.</summary>
    public enum WavePlacement
    {
        Front,
        Middle,
    }

    /// <summary>Bir düşman tipinin dalga zamanlaması (<see cref="NightDefinitions.Profiles"/>).</summary>
    public readonly struct WaveEnemyProfile
    {
        public WaveEnemyProfile(string enemyId, float spawnInterval, WavePlacement placement)
        {
            EnemyId = enemyId;
            SpawnInterval = spawnInterval;
            Placement = placement;
        }

        public string EnemyId { get; }
        public float SpawnInterval { get; }
        public WavePlacement Placement { get; }
    }

    /// <summary>Gecenin bir satırı: düşman tipi (id), adet, aralık, gecikme ve yönler.</summary>
    public readonly struct NightGroupDefinition
    {
        public NightGroupDefinition(string enemyId, int count, float spawnInterval, float startDelay, IReadOnlyList<CompassDirection> directions)
        {
            EnemyId = enemyId;
            Count = count;
            SpawnInterval = spawnInterval;
            StartDelay = startDelay;
            Directions = directions;
        }

        public string EnemyId { get; }
        public int Count { get; }
        public float SpawnInterval { get; }
        public float StartDelay { get; }
        public IReadOnlyList<CompassDirection> Directions { get; }
    }

    /// <summary>Bir gecenin üretici tanımı (GDD §8 satırı).</summary>
    public sealed class NightDefinition
    {
        public NightDefinition(int number, IReadOnlyList<CompassDirection> directions, IReadOnlyList<NightGroupDefinition> groups,
            string modifierId, string bossId, float bossStartDelay, CompassDirection bossDirection)
        {
            Number = number;
            Directions = directions;
            Groups = groups;
            ModifierId = modifierId;
            BossId = bossId;
            BossStartDelay = bossStartDelay;
            BossDirection = bossDirection;
        }

        /// <summary>1 tabanlı gece numarası.</summary>
        public int Number { get; }

        public IReadOnlyList<CompassDirection> Directions { get; }
        public IReadOnlyList<NightGroupDefinition> Groups { get; }

        /// <summary>Gece olayının id'si; olay yoksa null.</summary>
        public string ModifierId { get; }

        /// <summary>Boss'un EnemyData id'si; boss yoksa null.</summary>
        public string BossId { get; }

        public float BossStartDelay { get; }
        public CompassDirection BossDirection { get; }

        /// <summary>Asset dosya adı: Night01 … Night10.</summary>
        public string AssetName => $"Night{Number:00}";

        /// <summary>Test adlarında ve loglarda geceyi ayırt etmek için.</summary>
        public override string ToString()
        {
            return AssetName;
        }
    }
}
