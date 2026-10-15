using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// İnşa üretimi (8.1, 8.2): yapı tanımları (barikat GDD §11/§18), kutu yapı prefab'ı (kök/Visual, collider ve NavMeshObstacle
    /// ayak izi boyunda, Health + Structure), üretilen config, yuvalar, seviye materyalleri ve önizleme materyali.
    /// Hiyerarşi testleri asset oluşturmaz.
    /// </summary>
    public class BuildingGenerationTests
    {
        private const float Tolerance = 0.001f;

        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }
        }

        [Test]
        public void Definitions_HaveUniqueIdsAndSlotsAndValidLevels()
        {
            var ids = new HashSet<string>();
            var slots = new HashSet<int>();
            foreach (BuildableDefinitions.Definition definition in BuildableDefinitions.All)
            {
                Assert.IsTrue(ids.Add(definition.Id), $"Tekrar eden id: {definition.Id}");
                Assert.IsTrue(slots.Add(definition.SlotIndex), $"Tekrar eden yuva: {definition.SlotIndex}");
                Assert.That(definition.SlotIndex, Is.InRange(0, 4), "1–5 tuşları.");
                Assert.GreaterOrEqual(definition.Footprint.x, 1);
                Assert.GreaterOrEqual(definition.Footprint.y, 1);
                Assert.Greater(definition.Levels.Length, 0);
                foreach (BuildableDefinitions.LevelDefinition level in definition.Levels)
                {
                    Assert.Greater(level.Cost, 0, $"{definition.Id} {level.DisplayName} fiyatı.");
                    Assert.Greater(level.MaxHealth, 0f);
                }
            }
        }

        [Test]
        public void Barricade_MatchesGdd()
        {
            BuildableDefinitions.Definition barricade = Barricade();

            Assert.AreEqual(new Vector2Int(3, 1), barricade.Footprint, "GDD §11: 3×1.");
            Assert.AreEqual(1.5f, barricade.Height, Tolerance, "GDD §18: 3×1×1.5 küp.");
            Assert.IsTrue(barricade.CarvesNavMesh, "NavMesh'i keser.");
            Assert.AreEqual(TargetType.Barricade, barricade.TargetType, "Tank barikatı tercih eder.");
            Assert.AreEqual(0, barricade.SlotIndex, "B → 1.");

            BuildableDefinitions.LevelDefinition[] levels = barricade.Levels;
            Assert.AreEqual(3, levels.Length);
            AssertLevel(levels[0], "Ahşap", 50, 300f, PlaceholderMaterialId.BarricadeWood);
            AssertLevel(levels[1], "Metal", 120, 800f, PlaceholderMaterialId.BarricadeMetal);
            AssertLevel(levels[2], "Beton", 300, 2000f, PlaceholderMaterialId.BarricadeConcrete);
            Assert.AreEqual(PlaceholderMaterialId.BarricadeWood, barricade.Material, "Gövde 1. seviye renginde.");
        }

        [Test]
        public void BuildRoot_RootIsStructureWithFootprintSizedColliderAndObstacle()
        {
            BuildableDefinitions.Definition definition = Barricade();
            GameObject root = Build(definition);
            var expected = new Vector3(definition.Footprint.x, definition.Height, definition.Footprint.y);

            Assert.AreEqual(GameLayers.Structure, root.layer);
            Assert.IsTrue(root.CompareTag(GameTags.Structure));
            Assert.IsNull(root.GetComponent<Renderer>(), "Kökte mesh olmamalı.");

            var collider = root.GetComponent<BoxCollider>();
            Assert.IsNotNull(collider);
            AssertVector(expected, collider.size);
            Assert.AreEqual(definition.Height * 0.5f, collider.center.y, Tolerance, "Pivot yerde.");

            var obstacle = root.GetComponent<NavMeshObstacle>();
            Assert.IsNotNull(obstacle);
            Assert.AreEqual(NavMeshObstacleShape.Box, obstacle.shape);
            AssertVector(expected, obstacle.size);
            Assert.AreEqual(definition.CarvesNavMesh, obstacle.carving);
        }

        [Test]
        public void BuildRoot_HasHealthAndStructureOnRoot()
        {
            BuildableDefinitions.Definition definition = Barricade();
            GameObject root = Build(definition);

            var health = root.GetComponent<Health>();
            Assert.IsNotNull(health);
            Assert.AreEqual(definition.Levels[0].MaxHealth, health.Max, Tolerance, "1. seviye canı.");
            Assert.IsNotNull(root.GetComponent<Structure>());
        }

        [Test]
        public void BuildRoot_VisualHasMeshOnlyAndNoCollider()
        {
            GameObject root = Build(Barricade());

            Transform visual = VisualRoot.Find(root.transform);
            Assert.IsNotNull(visual);
            Assert.IsNotNull(visual.Find(BuildablePrefabs.BodyName).GetComponent<MeshRenderer>());
            Assert.AreEqual(0, visual.GetComponentsInChildren<Collider>(true).Length, "Görselde collider olmamalı.");
            Assert.AreEqual(0, visual.GetComponentsInChildren<MonoBehaviour>(true).Length, "Mantık kökte.");
        }

        [Test]
        public void BuildRoot_UsesCellSize()
        {
            BuildableDefinitions.Definition definition = Barricade();
            _root = BuildablePrefabs.BuildRoot(definition, 0.5f, null);

            Assert.AreEqual(definition.Footprint.x * 0.5f, _root.GetComponent<BoxCollider>().size.x, Tolerance);
        }

        [Test]
        public void Assets_ConfigSlotsHaveBuildablesWithPrefabs()
        {
            BuildConfig config = LoadConfigOrInconclusive();

            foreach (BuildableDefinitions.Definition definition in BuildableDefinitions.All)
            {
                var data = AssetDatabase.LoadAssetAtPath<BuildableData>(BuildableDefinitions.GetDataPath(definition));
                Assert.IsNotNull(data, definition.AssetName);
                Assert.AreEqual(definition.Id, data.Id);
                Assert.AreEqual(definition.TargetType, data.TargetType);
                Assert.IsNotNull(data.Prefab, $"{definition.AssetName} prefab'ı bağlı değil.");
                Assert.IsNotNull(VisualRoot.Find(data.Prefab.transform), "Önizleme Visual'ı kopyalar.");
                Assert.IsNotNull(data.Prefab.GetComponent<Structure>(), "Yerleştirme Structure bileşeniyle yapılır.");
                Assert.That(config.Slots, Has.Member(data), $"{definition.AssetName} yuvalarda değil.");
            }
        }

        [Test]
        public void Assets_BarricadeLevelsUsePlaceholderMaterialsAndBodyStartsWithFirst()
        {
            LoadConfigOrInconclusive();
            BuildableDefinitions.Definition definition = Barricade();
            var data = AssetDatabase.LoadAssetAtPath<BuildableData>(BuildableDefinitions.GetDataPath(definition));
            Assert.IsNotNull(data);

            for (int i = 0; i < definition.Levels.Length; i++)
            {
                Assert.AreSame(PlaceholderMaterials.GetOrCreate(definition.Levels[i].Material), data.GetLevel(i).Material, $"{i + 1}. seviye.");
            }

            var body = VisualRoot.Find(data.Prefab.transform).Find(BuildablePrefabs.BodyName).GetComponent<Renderer>();
            Assert.AreSame(data.GetLevel(0).Material, body.sharedMaterial, "Yükseltmede değişecek yuva 1. seviye materyalini kullanmalı.");
        }

        [Test]
        public void Assets_GhostMaterialIsTransparentAndLinked()
        {
            BuildConfig config = LoadConfigOrInconclusive();

            Material material = config.GhostMaterial;
            Assert.IsNotNull(material);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<Material>(BuildGhostMaterial.Path), material);
            Assert.GreaterOrEqual(material.renderQueue, (int)RenderQueue.Transparent);
            Assert.IsTrue(material.HasProperty("_BaseColor"), "Renk MaterialPropertyBlock ile _BaseColor'a yazılır.");
        }

        private static BuildableDefinitions.Definition Barricade()
        {
            foreach (BuildableDefinitions.Definition definition in BuildableDefinitions.All)
            {
                if (definition.Id == "barricade")
                {
                    return definition;
                }
            }

            Assert.Fail("Barikat tanımı yok.");
            return null;
        }

        private static BuildConfig LoadConfigOrInconclusive()
        {
            var config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildingGenerator.ConfigPath);
            if (config == null)
            {
                Assert.Inconclusive("BuildConfig yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            return config;
        }

        private static void AssertLevel(BuildableDefinitions.LevelDefinition level, string name, int cost, float maxHealth, PlaceholderMaterialId material)
        {
            Assert.AreEqual(name, level.DisplayName);
            Assert.AreEqual(cost, level.Cost, $"{name} fiyatı.");
            Assert.AreEqual(maxHealth, level.MaxHealth, Tolerance, $"{name} canı.");
            Assert.AreEqual(material, level.Material, $"{name} rengi (GDD §18).");
        }

        private GameObject Build(BuildableDefinitions.Definition definition)
        {
            _root = BuildablePrefabs.BuildRoot(definition, 1f, null);
            return _root;
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
            Assert.AreEqual(expected.z, actual.z, Tolerance, "z");
        }
    }
}
