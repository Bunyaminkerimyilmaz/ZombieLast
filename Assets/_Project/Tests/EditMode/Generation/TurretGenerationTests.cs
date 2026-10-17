using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Taret üretimi (9.1): tanım (GDD §11 seviyeleri, §18 görünüm), prefab hiyerarşisi (kök/Visual ayrımı, dönen kafa, Visual
    /// dışındaki nişan pivotu ve namlu, bileşen referansları), üretilen data/config/yuva. Hiyerarşi testleri asset oluşturmaz.
    /// </summary>
    public class TurretGenerationTests
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
        public void Definition_MatchesGdd()
        {
            BuildableDefinitions.Definition turret = Turret();

            Assert.AreEqual(new Vector2Int(2, 2), turret.Footprint, "GDD §11: 2×2.");
            Assert.AreEqual(TargetType.Turret, turret.TargetType, "Tükürücü tareti tercih eder (GDD §7).");
            Assert.AreEqual(1, turret.SlotIndex, "B → 2.");
            Assert.AreEqual(BuildableShape.Turret, turret.Shape);
            Assert.AreEqual(PlaceholderMaterialId.Turret, turret.Material, "GDD §18: camgöbeği.");

            BuildableDefinitions.LevelDefinition[] levels = turret.Levels;
            Assert.AreEqual(3, levels.Length);
            AssertLevel(levels[0], 250, 250f, damage: 8f, fireRate: 5f, range: 12f);
            AssertLevel(levels[1], 300, 250f, damage: 8f * 1.6f, fireRate: 5f, range: 12f);
            AssertLevel(levels[2], 500, 250f, damage: 8f * 1.6f, fireRate: 5f * 1.3f, range: 15f);
        }

        [Test]
        public void BuildRoot_VisualHasRotatingHeadAndNoLogic()
        {
            GameObject root = Build();

            Transform visual = VisualRoot.Find(root.transform);
            Assert.IsNotNull(visual);
            Assert.IsNotNull(visual.Find(TurretPrefab.BaseName).GetComponent<MeshRenderer>(), "Silindir gövde.");
            Transform head = visual.Find(TurretPrefab.HeadName);
            Assert.IsNotNull(head, "Dönen kafa Visual içinde.");
            Assert.IsNull(head.GetComponent<Renderer>(), "Kafa pivotu boş obje (dönüş merkezi).");
            Assert.IsNotNull(head.Find(TurretPrefab.HeadBodyName).GetComponent<MeshRenderer>(), "Küp kafa.");
            Assert.IsNotNull(head.Find(TurretPrefab.IndicatorName).GetComponent<MeshRenderer>(), "Güç göstergesi.");
            Assert.AreEqual(TurretPrefab.HeadHeight, head.localPosition.y, Tolerance);
            Assert.AreEqual(0, visual.GetComponentsInChildren<Collider>(true).Length, "Görselde collider olmamalı.");
            Assert.AreEqual(0, visual.GetComponentsInChildren<MonoBehaviour>(true).Length, "Mantık kökte.");
        }

        [Test]
        public void BuildRoot_AimPivotAndMuzzleAreOutsideVisual()
        {
            GameObject root = Build();

            Transform aim = root.transform.Find(TurretPrefab.AimName);
            Assert.IsNotNull(aim, "Nişan pivotu kökün çocuğu.");
            Transform muzzle = aim.Find(TurretPrefab.MuzzleName);
            Assert.IsNotNull(muzzle);
            Assert.AreEqual(TurretPrefab.HeadHeight, muzzle.position.y, Tolerance, "Namlu kafa yüksekliğinde.");
            Assert.Greater(muzzle.localPosition.z, 0.5f, "Namlu ileride.");
        }

        [Test]
        public void BuildRoot_HasTurretComponentsWithReferences()
        {
            GameObject root = Build();

            var turret = root.GetComponent<Turret>();
            var visual = root.GetComponent<TurretVisual>();
            Assert.IsNotNull(root.GetComponent<Structure>());
            Assert.IsNotNull(root.GetComponent<StructureRepair>());
            Assert.IsNotNull(turret);
            Assert.IsNotNull(visual);

            var serializedTurret = new SerializedObject(turret);
            Assert.AreSame(root.transform.Find(TurretPrefab.AimName), serializedTurret.FindProperty("_aimPivot").objectReferenceValue);
            Assert.AreSame(root.transform.Find(TurretPrefab.AimName).Find(TurretPrefab.MuzzleName),
                serializedTurret.FindProperty("_muzzle").objectReferenceValue);
            Assert.AreSame(visual, serializedTurret.FindProperty("_visual").objectReferenceValue);

            Transform head = VisualRoot.Find(root.transform).Find(TurretPrefab.HeadName);
            var serializedVisual = new SerializedObject(visual);
            Assert.AreSame(head, serializedVisual.FindProperty("_head").objectReferenceValue);
            Assert.AreSame(head.Find(TurretPrefab.IndicatorName).GetComponent<Renderer>(),
                serializedVisual.FindProperty("_indicator").objectReferenceValue);
        }

        [Test]
        public void BuildRoot_FootprintSizedColliderAndBarAboveHead()
        {
            BuildableDefinitions.Definition definition = Turret();
            GameObject root = Build();

            var collider = root.GetComponent<BoxCollider>();
            Assert.AreEqual(2f, collider.size.x, Tolerance);
            Assert.AreEqual(2f, collider.size.z, Tolerance);
            Assert.AreEqual(definition.Height, collider.size.y, Tolerance);
            Assert.Greater(definition.Height, TurretPrefab.HeadHeight, "Kafa collider'ın içinde.");

            StructureRepairBar bar = root.GetComponentInChildren<StructureRepairBar>(true);
            Assert.IsNotNull(bar);
            Assert.Greater(bar.transform.localPosition.y, definition.Height);
        }

        [Test]
        public void Assets_TurretDataPrefabConfigAndSlot()
        {
            var config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildingGenerator.ConfigPath);
            var turretConfig = AssetDatabase.LoadAssetAtPath<TurretConfig>(BuildingGenerator.TurretConfigPath);
            BuildableDefinitions.Definition definition = Turret();
            var data = AssetDatabase.LoadAssetAtPath<BuildableData>(BuildableDefinitions.GetDataPath(definition));
            if (config == null || turretConfig == null || data == null)
            {
                Assert.Inconclusive("Taret asset'leri yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.AreEqual(0.2f, turretConfig.ScanInterval, Tolerance, "Görev: 0.2 sn'de bir tarama.");
            Assert.AreEqual(GameLayers.EnemyMask, turretConfig.TargetLayers.value);
            Assert.IsFalse(GameLayers.Contains(turretConfig.HitMask, GameLayers.Structure), "Barikatın üstünden ateş eder.");

            Assert.AreEqual(TargetType.Turret, data.TargetType);
            Assert.AreEqual(8f, data.GetLevel(0).GetStat(BuildableStat.Damage, 0f), Tolerance);
            Assert.AreEqual(15f, data.GetLevel(2).GetStat(BuildableStat.Range, 0f), Tolerance);
            Assert.AreEqual(250, data.BuildCost);

            Assert.IsNotNull(data.Prefab);
            var turret = data.Prefab.GetComponent<Turret>();
            Assert.IsNotNull(turret);
            Assert.AreSame(turretConfig, new SerializedObject(turret).FindProperty("_config").objectReferenceValue);

            Renderer indicator = VisualRoot.Find(data.Prefab.transform).Find(TurretPrefab.HeadName).Find(TurretPrefab.IndicatorName)
                .GetComponent<Renderer>();
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<Material>(TurretPrefab.IndicatorMaterialPath), indicator.sharedMaterial);

            Assert.That(config.Slots.Count, Is.GreaterThan(1));
            Assert.AreSame(data, config.Slots[1], "2. yuva.");
        }

        private static BuildableDefinitions.Definition Turret()
        {
            foreach (BuildableDefinitions.Definition definition in BuildableDefinitions.All)
            {
                if (definition.Id == "turret")
                {
                    return definition;
                }
            }

            Assert.Fail("Taret tanımı yok.");
            return null;
        }

        private GameObject Build()
        {
            _root = BuildablePrefabs.BuildRoot(Turret(), 1f, null);
            return _root;
        }

        private static void AssertLevel(BuildableDefinitions.LevelDefinition level, int cost, float maxHealth, float damage, float fireRate, float range)
        {
            Assert.AreEqual(cost, level.Cost, $"{level.DisplayName} fiyatı.");
            Assert.AreEqual(maxHealth, level.MaxHealth, Tolerance, $"{level.DisplayName} canı.");
            var data = new BuildableLevel(level.DisplayName, level.Cost, level.MaxHealth, level.Stats);
            Assert.AreEqual(damage, data.GetStat(BuildableStat.Damage, -1f), Tolerance, $"{level.DisplayName} hasarı.");
            Assert.AreEqual(fireRate, data.GetStat(BuildableStat.FireRate, -1f), Tolerance, $"{level.DisplayName} atış hızı.");
            Assert.AreEqual(range, data.GetStat(BuildableStat.Range, -1f), Tolerance, $"{level.DisplayName} menzili.");
        }
    }
}
