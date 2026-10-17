using LastHaven.Building;
using LastHaven.Combat;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Enemies;
using LastHaven.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Diken tuzağı ve durum efektleri üretimi (9.3): tanım (GDD §11 değerleri, §18 görünüm), prefab (üstünden geçilen trigger alanı,
    /// NavMeshObstacle yok, yassı görsel, SpikeTrap bileşeni), üretilen data/config/yuva, düşman prefab'larında EnemyStatusEffects,
    /// oyuncu loadout'unda perk config'i (Avcı). Hiyerarşi testleri asset oluşturmaz.
    /// </summary>
    public class SpikeTrapGenerationTests
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
            BuildableDefinitions.Definition trap = SpikeTrap();

            Assert.AreEqual(new Vector2Int(2, 2), trap.Footprint, "GDD §11: 2×2.");
            Assert.IsFalse(trap.CarvesNavMesh, "Üzerinden geçilir, NavMesh'i kesmez.");
            Assert.AreEqual(TargetType.Trap, trap.TargetType);
            Assert.AreEqual(2, trap.SlotIndex, "B → 3.");
            Assert.AreEqual(BuildableShape.SpikeTrap, trap.Shape);
            Assert.AreEqual(PlaceholderMaterialId.SpikeTrap, trap.Material, "GDD §18: koyu gri.");
            Assert.Less(trap.Height, 0.5f, "Yassı.");

            Assert.AreEqual(1, trap.Levels.Length, "Yükseltme yok.");
            BuildableDefinitions.LevelDefinition level = trap.Levels[0];
            Assert.AreEqual(80, level.Cost);
            Assert.AreEqual(40f, level.MaxHealth, Tolerance, "Dayanıklılık 40 = can.");
            SpikeTrapStats stats = SpikeTrapStats.From(new BuildableLevel(level.DisplayName, level.Cost, level.MaxHealth, level.Stats));
            Assert.AreEqual(15f, stats.DamagePerSecond, Tolerance);
            Assert.AreEqual(0.4f, stats.SlowFraction, Tolerance);
            Assert.AreEqual(0.5f, stats.TickInterval, Tolerance);
        }

        [Test]
        public void BuildRoot_IsWalkableTriggerAreaWithoutObstacle()
        {
            GameObject root = Build();

            Assert.AreEqual(GameLayers.Structure, root.layer);
            Assert.IsTrue(root.CompareTag(GameTags.Structure));
            Assert.IsNull(root.GetComponent<NavMeshObstacle>(), "Kaçınmaya girmesin: ajanlar üstünden yürür.");

            var area = root.GetComponent<BoxCollider>();
            Assert.IsNotNull(area);
            Assert.IsTrue(area.isTrigger, "Mermiler, inşa çakışması ve oyuncu onu görmez.");
            Assert.AreEqual(2f, area.size.x, Tolerance);
            Assert.AreEqual(2f, area.size.z, Tolerance);
            Assert.AreEqual(SpikeTrapPrefab.AreaHeight, area.size.y, Tolerance);
            Assert.AreEqual(SpikeTrapPrefab.AreaHeight * 0.5f, area.center.y, Tolerance, "Alan yerden başlar.");
        }

        [Test]
        public void BuildRoot_HasStructureTrapAndRepairComponents()
        {
            GameObject root = Build();

            Assert.AreEqual(40f, root.GetComponent<Health>().Max, Tolerance);
            Assert.IsNotNull(root.GetComponent<Structure>());
            Assert.IsNotNull(root.GetComponent<SpikeTrap>());
            Assert.IsNotNull(root.GetComponent<StructureRepair>());
            StructureRepairBar bar = root.GetComponentInChildren<StructureRepairBar>(true);
            Assert.IsNotNull(bar);
            Assert.Greater(bar.transform.localPosition.y, SpikeTrapPrefab.VisualHeight);
        }

        [Test]
        public void BuildRoot_VisualIsFlatPlateWithSpikes_AndNoLogic()
        {
            GameObject root = Build();

            Transform visual = VisualRoot.Find(root.transform);
            Assert.IsNotNull(visual);
            Assert.IsNotNull(visual.Find(SpikeTrapPrefab.PlateName).GetComponent<MeshRenderer>());
            Assert.AreEqual(1 + SpikeTrapPrefab.SpikesPerRow * SpikeTrapPrefab.SpikesPerRow, visual.GetComponentsInChildren<MeshRenderer>(true).Length);
            Assert.AreEqual(0, visual.GetComponentsInChildren<Collider>(true).Length, "Görselde collider olmamalı.");
            Assert.AreEqual(0, visual.GetComponentsInChildren<MonoBehaviour>(true).Length, "Mantık kökte.");

            foreach (MeshRenderer renderer in visual.GetComponentsInChildren<MeshRenderer>(true))
            {
                Assert.LessOrEqual(renderer.bounds.max.y, SpikeTrapPrefab.VisualHeight + Tolerance, renderer.name);
                Assert.LessOrEqual(Mathf.Abs(renderer.bounds.max.x), 1f + Tolerance, "Ayak izinin içinde.");
            }
        }

        [Test]
        public void Assets_TrapDataPrefabConfigAndSlot()
        {
            var config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildingGenerator.ConfigPath);
            var trapConfig = AssetDatabase.LoadAssetAtPath<SpikeTrapConfig>(BuildingGenerator.SpikeTrapConfigPath);
            var data = AssetDatabase.LoadAssetAtPath<BuildableData>(BuildableDefinitions.GetDataPath(SpikeTrap()));
            if (config == null || trapConfig == null || data == null)
            {
                Assert.Inconclusive("Tuzak asset'leri yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.AreEqual(GameLayers.EnemyMask, trapConfig.TargetLayers.value);
            Assert.AreEqual(1f, trapConfig.WearPerTick, Tolerance, "GDD §11: her tik dayanıklılıktan 1 düşer.");
            Assert.Greater(trapConfig.SlowDuration, 0.5f, "Yavaşlatma tik aralığından uzun: tuzaktayken kesilmez.");
            Assert.Less(trapConfig.IdleScanInterval, 0.5f);

            Assert.AreEqual(TargetType.Trap, data.TargetType);
            Assert.IsFalse(data.CarvesNavMesh);
            Assert.AreEqual(80, data.BuildCost);
            Assert.AreEqual(40f, data.GetLevel(0).MaxHealth, Tolerance);
            Assert.AreEqual(7.5f, SpikeTrapStats.From(data.GetLevel(0)).DamagePerTick, Tolerance);

            Assert.IsNotNull(data.Prefab);
            var trap = data.Prefab.GetComponent<SpikeTrap>();
            Assert.IsNotNull(trap);
            Assert.AreSame(trapConfig, new SerializedObject(trap).FindProperty("_config").objectReferenceValue);
            Assert.IsTrue(data.Prefab.GetComponent<BoxCollider>().isTrigger);
            Assert.IsNull(data.Prefab.GetComponent<NavMeshObstacle>());

            Assert.That(config.Slots.Count, Is.GreaterThan(2));
            Assert.AreSame(data, config.Slots[2], "3. yuva.");
        }

        [Test]
        public void Assets_EnemyPrefabsHaveStatusEffectsAndFlash()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { EnemyPrefabs.Folder });
            if (guids.Length == 0)
            {
                Assert.Inconclusive("Düşman prefab'ları yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            foreach (string guid in guids)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (prefab.GetComponent<Enemy>() == null)
                {
                    continue;
                }

                Assert.IsNotNull(prefab.GetComponent<EnemyStatusEffects>(), $"{prefab.name}: yavaşlatılabilmeli.");
                Assert.IsNotNull(prefab.GetComponent<HitFlash>(), $"{prefab.name}: mavi ton HitFlash ile.");
            }
        }

        [Test]
        public void Assets_LoadoutHasCombatPerkConfig()
        {
            var loadout = AssetDatabase.LoadAssetAtPath<WeaponLoadoutData>(PlayerWeaponsGenerator.LoadoutPath);
            var perks = AssetDatabase.LoadAssetAtPath<CombatPerkConfig>(WeaponsGenerator.PerkConfigPath);
            if (loadout == null || perks == null)
            {
                Assert.Inconclusive("Silah asset'leri yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.AreSame(perks, loadout.Perks);
            Assert.AreEqual(0.5f, perks.HunterDamageBonus, Tolerance, "GDD §13 Avcı: +%50.");
        }

        private static BuildableDefinitions.Definition SpikeTrap()
        {
            foreach (BuildableDefinitions.Definition definition in BuildableDefinitions.All)
            {
                if (definition.Id == "spike_trap")
                {
                    return definition;
                }
            }

            Assert.Fail("Diken tuzağı tanımı yok.");
            return null;
        }

        private GameObject Build()
        {
            _root = BuildablePrefabs.BuildRoot(SpikeTrap(), 1f, null);
            return _root;
        }
    }
}
