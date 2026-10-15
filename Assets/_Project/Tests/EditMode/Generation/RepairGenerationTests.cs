using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Enemies;
using LastHaven.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Onarım ve tıkalı yol üretimi (8.3): yapı hiyerarşisinde StructureRepair + onarım barı (Visual dışında), üretilen
    /// RepairConfig (GDD §11), barikat prefab'ında bağlı referanslar, oyuncuda Interactor, Tank'ın barikat tercihi (GDD §7).
    /// </summary>
    public class RepairGenerationTests
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
        public void BuildRoot_HasRepairAndBarOutsideVisual()
        {
            _root = BuildablePrefabs.BuildRoot(BuildableDefinitions.All[0], 1f, null);

            Assert.IsNotNull(_root.GetComponent<StructureRepair>());
            var bar = _root.GetComponentInChildren<StructureRepairBar>(true);
            Assert.IsNotNull(bar);
            Assert.AreSame(_root.transform, bar.transform.parent, "Bar kökün çocuğu (önizleme Visual'ı kopyalar).");
            Assert.Greater(bar.transform.localPosition.y, BuildableDefinitions.All[0].Height, "Bar yapının üstünde.");
            Assert.AreEqual(0, bar.GetComponentsInChildren<Collider>(true).Length, "Bar ışın ve hover'a karışmasın.");
            Assert.IsNull(VisualRoot.Find(_root.transform).GetComponentInChildren<StructureRepairBar>(true));

            var serialized = new SerializedObject(_root.GetComponent<StructureRepair>());
            Assert.AreSame(bar, serialized.FindProperty("_bar").objectReferenceValue);
        }

        [Test]
        public void Assets_RepairConfigMatchesGdd()
        {
            var config = AssetDatabase.LoadAssetAtPath<RepairConfig>(BuildingGenerator.RepairConfigPath);
            if (config == null)
            {
                Assert.Inconclusive("RepairConfig yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.AreEqual(2f, config.Range, Tolerance);
            Assert.AreEqual(50f, config.Settings.HealthPerSecond, Tolerance);
            Assert.AreEqual(5f, config.Settings.HealthPerMoney, Tolerance);
            Assert.AreEqual("Onar (3)", config.FormatPrompt(3));
        }

        [Test]
        public void Assets_BuildablePrefabsHaveLinkedRepair()
        {
            var config = AssetDatabase.LoadAssetAtPath<RepairConfig>(BuildingGenerator.RepairConfigPath);
            if (config == null)
            {
                Assert.Inconclusive("RepairConfig yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            foreach (BuildableDefinitions.Definition definition in BuildableDefinitions.All)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildablePrefabs.GetPrefabPath(definition));
                Assert.IsNotNull(prefab, definition.AssetName);
                var repair = prefab.GetComponent<StructureRepair>();
                Assert.IsNotNull(repair, $"{definition.AssetName}: StructureRepair yok.");
                Assert.AreSame(config, repair.Config);

                var serialized = new SerializedObject(repair);
                var bar = serialized.FindProperty("_bar").objectReferenceValue as StructureRepairBar;
                Assert.IsNotNull(bar, $"{definition.AssetName}: onarım barı bağlı değil.");
                foreach (Renderer renderer in bar.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.IsNotNull(renderer.sharedMaterial, $"{definition.AssetName}: {renderer.name} materyalsiz.");
                }
            }
        }

        [Test]
        public void Assets_PlayerHasInteractor()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerGenerator.PrefabPath);
            if (prefab == null)
            {
                Assert.Inconclusive("Player prefab'ı yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.IsNotNull(prefab.GetComponent<Interactor>());
        }

        [Test]
        public void Assets_TankPrefersBarricadesWithinFifteenMeters()
        {
            var tank = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemiesGenerator.GetDataPath(EnemyDefinitions.Tank));
            if (tank == null)
            {
                Assert.Inconclusive("Tank verisi yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.AreEqual(TargetType.Barricade, tank.PreferredTarget, "GDD §7: Tank önce barikatı hedefler.");
            Assert.AreEqual(15f, tank.PreferredTargetRadius, Tolerance);
            Assert.AreEqual(3f, tank.AggroRadius, Tolerance);

            var barricade = AssetDatabase.LoadAssetAtPath<BuildableData>(BuildableDefinitions.GetDataPath(BuildableDefinitions.All[0]));
            Assert.IsNotNull(barricade);
            Assert.AreEqual(TargetType.Barricade, barricade.TargetType, "Barikat hedef kaydına Barricade olarak girer.");
        }

        [Test]
        public void Assets_EnemiesHaveAvoidancePriorityRange()
        {
            foreach (EnemyDefinition definition in EnemyDefinitions.All)
            {
                var data = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemiesGenerator.GetDataPath(definition));
                if (data == null)
                {
                    Assert.Inconclusive("Düşman verisi yok; Tools/Last Haven/Rebuild All çalıştır.");
                }

                Assert.Less(data.MinAvoidancePriority, data.MaxAvoidancePriority, definition.AssetName);
            }
        }
    }
}
