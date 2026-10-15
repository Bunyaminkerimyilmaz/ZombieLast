using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Üreticinin yapı tanımları (GDD §11 tablosu + §18 görünüm): data + prefab ölçüleri + yuva. 8.2: Barikat. Taret ve diken
    /// tuzağı Gün 9'da eklenir. Data <c>Data/Building/&lt;Ad&gt;.asset</c>, prefab <c>Prefabs/Building/&lt;Ad&gt;.prefab</c>.
    /// </summary>
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
                    level.DisplayName, level.Cost, level.MaxHealth, PlaceholderMaterials.GetOrCreate(level.Material));
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
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
                LevelDefinition[] levels)
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
        }

        /// <summary>Bir seviyenin üretim tanımı: ad, fiyat (1. seviyede inşa, sonra yükseltme), maks. can, görünüm.</summary>
        public sealed class LevelDefinition
        {
            public LevelDefinition(string displayName, int cost, float maxHealth, PlaceholderMaterialId material)
            {
                DisplayName = displayName;
                Cost = cost;
                MaxHealth = maxHealth;
                Material = material;
            }

            public string DisplayName { get; }

            public int Cost { get; }

            public float MaxHealth { get; }

            public PlaceholderMaterialId Material { get; }
        }
    }
}
