using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Üreticinin yapı tanımları (GDD §11 tablosu + §18 görünüm): data + prefab ölçüleri + yuva. 8.2: Barikat, 9.1: Taret,
    /// 9.3: Diken tuzağı. Data <c>Data/Building/&lt;Ad&gt;.asset</c>, prefab <c>Prefabs/Building/&lt;Ad&gt;.prefab</c>.
    /// </summary>
    /// <remarks>
    /// Taret seviyeleri GDD §11'den mutlak değer: 2. seviye hasar 8 × 1.6 = 12.8; 3. seviye menzil 12 + 3 = 15, atış hızı
    /// 5 × 1.3 = 6.5 (hasar 2. seviyeden kalır). GDD sadece 1. seviyenin canını (250) verir; yükseltme canı artırmaz.
    /// Tuzağın tek seviyesi var; dayanıklılığı (40) canıdır, statları hasar/sn (15), yavaşlatma (0.4), tik aralığı (0.5).
    /// </remarks>
    public static class BuildableDefinitions
    {
        public const string DataFolder = "Assets/_Project/Data/Building";

        public static readonly IReadOnlyList<Definition> All = new[]
        {
            new Definition(
                id: "barricade",
                assetName: "Barricade",
                displayName: "Barikat",
                description: "Zombilerin yolunu keser; etrafından dolaşırlar. U ile metale ve betona yükseltilir.",
                footprint: new Vector2Int(3, 1),
                height: 1.5f,
                carvesNavMesh: true,
                targetType: TargetType.Barricade,
                slotIndex: 0,
                levels: new[]
                {
                    new LevelDefinition("Ahşap", 50, 300f, PlaceholderMaterialId.BarricadeWood),
                    new LevelDefinition("Metal", 120, 800f, PlaceholderMaterialId.BarricadeMetal),
                    new LevelDefinition("Beton", 300, 2000f, PlaceholderMaterialId.BarricadeConcrete),
                }),
            new Definition(
                id: "turret",
                assetName: "Turret",
                displayName: "Taret",
                description: "Menzildeki en yakın zombiye döner ve ateş eder; güç ister. U ile önce hasarı, sonra menzili ve atış hızı artar.",
                footprint: new Vector2Int(2, 2),
                height: 1.6f,
                carvesNavMesh: true,
                targetType: TargetType.Turret,
                slotIndex: 1,
                levels: new[]
                {
                    new LevelDefinition("Standart", 250, 250f, PlaceholderMaterialId.Turret, TurretLevelStats(8f, 5f, 12f)),
                    new LevelDefinition("Ağır Namlu", 300, 250f, PlaceholderMaterialId.Turret, TurretLevelStats(12.8f, 5f, 12f)),
                    new LevelDefinition("Gelişmiş", 500, 250f, PlaceholderMaterialId.Turret, TurretLevelStats(12.8f, 6.5f, 15f)),
                },
                shape: BuildableShape.Turret),
            new Definition(
                id: "spike_trap",
                assetName: "SpikeTrap",
                displayName: "Diken Tuzağı",
                description: "Üstünden geçen zombilere hasar verir ve onları %40 yavaşlatır. Her hasar tikinde aşınır, dayanıklılığı bitince kırılır.",
                footprint: new Vector2Int(2, 2),
                height: SpikeTrapPrefab.VisualHeight,
                carvesNavMesh: false,
                targetType: TargetType.Trap,
                slotIndex: 2,
                levels: new[]
                {
                    // Dayanıklılık 40 = can (GDD §11 tablosunda can sütununda).
                    new LevelDefinition("Standart", 80, 40f, PlaceholderMaterialId.SpikeTrap, SpikeTrapLevelStats(15f, 0.4f, 0.5f)),
                },
                shape: BuildableShape.SpikeTrap),
        };

        public static string GetDataPath(Definition definition)
        {
            return $"{DataFolder}/{definition.AssetName}.asset";
        }

        /// <summary>Tanımı yeni oluşturulmuş data'ya yazar (prefab hariç; seviye materyalleri dahil). Var olan asset'e uygulanmaz.</summary>
        public static void Apply(BuildableData data, Definition definition)
        {
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = definition.Id;
            serialized.FindProperty("_displayName").stringValue = definition.DisplayName;
            serialized.FindProperty("_description").stringValue = definition.Description;
            serialized.FindProperty("_footprint").vector2IntValue = definition.Footprint;
            serialized.FindProperty("_carvesNavMesh").boolValue = definition.CarvesNavMesh;
            serialized.FindProperty("_targetType").intValue = (int)definition.TargetType;

            SerializedProperty levels = serialized.FindProperty("_levels");
            levels.arraySize = definition.Levels.Length;
            for (int i = 0; i < definition.Levels.Length; i++)
            {
                LevelDefinition level = definition.Levels[i];
                levels.GetArrayElementAtIndex(i).boxedValue = new BuildableLevel(
                    level.DisplayName, level.Cost, level.MaxHealth, PlaceholderMaterials.GetOrCreate(level.Material), level.Stats);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static BuildableStatValue[] TurretLevelStats(float damage, float fireRate, float range)
        {
            return new[]
            {
                new BuildableStatValue(BuildableStat.Damage, damage),
                new BuildableStatValue(BuildableStat.FireRate, fireRate),
                new BuildableStatValue(BuildableStat.Range, range),
            };
        }

        private static BuildableStatValue[] SpikeTrapLevelStats(float damagePerSecond, float slowFraction, float tickInterval)
        {
            return new[]
            {
                new BuildableStatValue(BuildableStat.Damage, damagePerSecond),
                new BuildableStatValue(BuildableStat.SlowFraction, slowFraction),
                new BuildableStatValue(BuildableStat.TickInterval, tickInterval),
            };
        }

        /// <summary>Bir yapı tipinin üretim tanımı.</summary>
        public sealed class Definition
        {
            public Definition(
                string id,
                string assetName,
                string displayName,
                string description,
                Vector2Int footprint,
                float height,
                bool carvesNavMesh,
                TargetType targetType,
                int slotIndex,
                LevelDefinition[] levels,
                BuildableShape shape = BuildableShape.Box)
            {
                Id = id;
                AssetName = assetName;
                DisplayName = displayName;
                Description = description;
                Footprint = footprint;
                Height = height;
                CarvesNavMesh = carvesNavMesh;
                TargetType = targetType;
                SlotIndex = slotIndex;
                Levels = levels;
                Shape = shape;
            }

            public string Id { get; }

            /// <summary>Data ve prefab asset'inin adı.</summary>
            public string AssetName { get; }

            public string DisplayName { get; }

            public string Description { get; }

            /// <summary>Hücre cinsinden ayak izi; prefab'ın collider/gövde ölçüsü bu × hücre boyu.</summary>
            public Vector2Int Footprint { get; }

            /// <summary>Gövde yüksekliği (m).</summary>
            public float Height { get; }

            /// <summary>Prefab gövdesinin materyali: 1. seviyeninki (yükseltmede bu yuvalar değişir).</summary>
            public PlaceholderMaterialId Material => Levels[0].Material;

            public bool CarvesNavMesh { get; }

            public TargetType TargetType { get; }

            /// <summary>BuildConfig'teki 0 tabanlı yuva (boşsa buraya yazılır).</summary>
            public int SlotIndex { get; }

            public LevelDefinition[] Levels { get; }

            /// <summary>Prefab'ın yer tutucu görünümü.</summary>
            public BuildableShape Shape { get; }
        }

        /// <summary>Bir seviyenin üretim tanımı: ad, fiyat (1. seviyede inşa, sonra yükseltme), maks. can, görünüm, tipe özel statlar.</summary>
        public sealed class LevelDefinition
        {
            public LevelDefinition(string displayName, int cost, float maxHealth, PlaceholderMaterialId material, params BuildableStatValue[] stats)
            {
                DisplayName = displayName;
                Cost = cost;
                MaxHealth = maxHealth;
                Material = material;
                Stats = stats ?? System.Array.Empty<BuildableStatValue>();
            }

            /// <summary>Seviyenin mutlak statları (taret: hasar, atış hızı, menzil). Boş olabilir.</summary>
            public BuildableStatValue[] Stats { get; }

            public string DisplayName { get; }

            public int Cost { get; }

            public float MaxHealth { get; }

            public PlaceholderMaterialId Material { get; }
        }
    }
}
