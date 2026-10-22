using System;
using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Combat;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Enemies;
using LastHaven.Player;
using LastHaven.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Perk üretimi (13.2): tanımlar GDD §13 tablosuyla (nadirlik, maks. yığın, etkiler), modül perk config varsayılanları, üretilen
    /// katalog ve asset bağlantıları (taret config → yapı perk config'i, oyuncu prefab'ı → <see cref="PlayerPerkEffects"/>).
    /// </summary>
    public class PerkGenerationTests
    {
        private const float Tolerance = 0.0001f;

        private TestPerks _perks;

        [SetUp]
        public void SetUp()
        {
            _perks = new TestPerks();
        }

        [TearDown]
        public void TearDown()
        {
            _perks.DestroyAll();
        }

        [TestCase("quick_trigger", "Hızlı Tetik", PerkRarity.Common, 3)]
        [TestCase("heavy_rounds", "Ağır Mermi", PerkRarity.Common, 3)]
        [TestCase("thick_skin", "Kalın Deri", PerkRarity.Common, 3)]
        [TestCase("magnet", "Mıknatıs", PerkRarity.Common, 2)]
        [TestCase("sturdy_walls", "Sağlam Duvar", PerkRarity.Common, 2)]
        [TestCase("greedy", "Açgözlü", PerkRarity.Rare, 2)]
        [TestCase("adrenaline", "Adrenalin", PerkRarity.Rare, 1)]
        [TestCase("turret_master", "Taret Ustası", PerkRarity.Rare, 2)]
        [TestCase("vampire", "Vampir", PerkRarity.Rare, 1)]
        [TestCase("hunter", "Avcı", PerkRarity.Rare, 1)]
        [TestCase("ricochet", "Seken Mermi", PerkRarity.Epic, 1)]
        [TestCase("thorn_walls", "Dikenli Duvar", PerkRarity.Epic, 1)]
        [TestCase("shock_turret", "Şok Taret", PerkRarity.Epic, 1)]
        [TestCase("chain_explosion", "Zincirleme Son", PerkRarity.Epic, 1)]
        public void Definitions_MatchGddTable(string id, string displayName, PerkRarity rarity, int maxStacks)
        {
            PerkDefinitions.Definition definition = Find(id);

            Assert.AreEqual(displayName, definition.DisplayName);
            Assert.AreEqual(rarity, definition.Rarity);
            Assert.AreEqual(maxStacks, definition.MaxStacks);
            Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Description));
        }

        [Test]
        public void Definitions_AreFourteenWithUniqueIdsAndAssetNames()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var assetNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (PerkDefinitions.Definition definition in PerkDefinitions.All)
            {
                Assert.IsTrue(ids.Add(definition.Id), definition.Id);
                Assert.IsTrue(assetNames.Add(definition.AssetName), definition.AssetName);
            }

            Assert.AreEqual(14, PerkDefinitions.All.Count);
        }

        [Test]
        public void Definitions_CreateFreshEffectInstances()
        {
            PerkDefinitions.Definition definition = Find("quick_trigger");

            Assert.AreNotSame(definition.CreateEffects()[0], definition.CreateEffects()[0], "Asset'ler aynı managed reference'ı paylaşmasın.");
        }

        [Test]
        public void AllPerksOnce_ApplyGddEffects()
        {
            var modifiers = new RunModifiers();
            var heals = new List<float>();
            var inventory = new PerkInventory(modifiers, heals.Add);

            foreach (PerkDefinitions.Definition definition in PerkDefinitions.All)
            {
                Assert.IsTrue(inventory.TryAcquire(_perks.Create(definition)), definition.Id);
            }

            Assert.AreEqual(10f * 1.15f, modifiers.Get(StatType.PlayerFireRate, 10f), Tolerance, "Hızlı Tetik +%15");
            Assert.AreEqual(20f * 1.15f, modifiers.Get(StatType.PlayerDamage, 20f), Tolerance, "Ağır Mermi +%15");
            Assert.AreEqual(125f, modifiers.Get(StatType.PlayerMaxHealth, 100f), Tolerance, "Kalın Deri +25");
            CollectionAssert.AreEqual(new[] { 25f }, heals, "Kalın Deri o kadar iyileştirir");
            Assert.AreEqual(4.5f, modifiers.Get(StatType.PickupRadius, 3f), Tolerance, "Mıknatıs +%50");
            Assert.AreEqual(390f, modifiers.Get(StatType.BarricadeMaxHealth, 300f), Tolerance, "Sağlam Duvar +%30");
            Assert.AreEqual(12.5f, modifiers.Get(StatType.CoinValue, 10f), Tolerance, "Açgözlü +%25");
            Assert.AreEqual(1.2f, modifiers.Get(StatType.DashCooldown, 2f), Tolerance, "Adrenalin −%40");
            Assert.AreEqual(6.25f, modifiers.Get(StatType.TurretFireRate, 5f), Tolerance, "Taret Ustası +%25");
            Assert.AreEqual(8f, modifiers.Get(StatType.TurretDamage, 8f), Tolerance, "Taret hasarına perk yok");

            foreach (PerkFlag flag in Enum.GetValues(typeof(PerkFlag)))
            {
                Assert.IsTrue(modifiers.HasFlag(flag), flag.ToString());
            }

            Assert.AreEqual(14, inventory.TotalStacks);
        }

        [Test]
        public void ModulePerkConfigs_DefaultsMatchGdd()
        {
            var combat = ScriptableObject.CreateInstance<CombatPerkConfig>();
            var building = ScriptableObject.CreateInstance<BuildingPerkConfig>();
            var enemies = ScriptableObject.CreateInstance<EnemyPerkConfig>();
            var player = ScriptableObject.CreateInstance<PlayerData>();
            try
            {
                Assert.AreEqual(0.5f, combat.HunterDamageBonus, Tolerance, "Avcı +%50");
                Assert.AreEqual(0.5f, combat.RicochetDamageFraction, Tolerance, "Seken Mermi %50");
                Assert.AreEqual(GameLayers.EnemyMask, combat.RicochetTargetLayers.value);
                Assert.AreEqual(0.3f, building.ThornsReflectFraction, Tolerance, "Dikenli Duvar %30");
                Assert.AreEqual(0.25f, building.ShockSlowFraction, Tolerance, "Şok Taret %25");
                Assert.AreEqual(1f, building.ShockSlowDuration, Tolerance, "Şok Taret 1 sn");
                Assert.AreEqual(0.1f, enemies.ChainExplosionChance, Tolerance, "Zincirleme Son %10");
                Assert.AreEqual(3f, enemies.ChainExplosionRadius, Tolerance, "3 m");
                Assert.AreEqual(40f, enemies.ChainExplosionDamage, Tolerance, "40 hasar");
                Assert.AreEqual(GameLayers.EnemyMask, enemies.ChainExplosionLayers.value, "Oyuncuya ve yapılara vurmaz.");
                Assert.AreEqual(2f, player.VampireHealPerKill, Tolerance, "Vampir +2 can");
            }
            finally
            {
                Object.DestroyImmediate(combat);
                Object.DestroyImmediate(building);
                Object.DestroyImmediate(enemies);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void ProgressionConfig_OfferDefaults()
        {
            var config = ScriptableObject.CreateInstance<ProgressionConfig>();
            try
            {
                Assert.AreEqual(3, config.OfferSize, "GDD §13: 3 kart.");
                Assert.AreEqual(0, config.OfferSeed, "Varsayılan: her run farklı.");
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Catalog_Asset_ContainsEveryDefinition()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PerkCatalog>(ProgressionGenerator.CatalogPath);
            if (catalog == null)
            {
                Assert.Inconclusive($"{ProgressionGenerator.CatalogPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            foreach (PerkDefinitions.Definition definition in PerkDefinitions.All)
            {
                var perk = AssetDatabase.LoadAssetAtPath<PerkData>(ProgressionGenerator.GetPerkPath(definition.AssetName));
                Assert.IsNotNull(perk, definition.AssetName);
                Assert.That(catalog.Perks, Has.Member(perk), definition.Id);
                Assert.AreEqual(definition.Id, perk.Id);
                Assert.Greater(perk.Effects.Count, 0, definition.Id);
            }
        }

        [Test]
        public void TurretConfig_Asset_IsLinkedToBuildingPerkConfig()
        {
            var turret = AssetDatabase.LoadAssetAtPath<TurretConfig>(BuildingGenerator.TurretConfigPath);
            var perks = AssetDatabase.LoadAssetAtPath<BuildingPerkConfig>(BuildingPerksGenerator.ConfigPath);
            if (turret == null || perks == null)
            {
                Assert.Inconclusive("Taret veya yapı perk config'i yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.AreSame(perks, turret.Perks);
        }

        [Test]
        public void PlayerPrefab_HasPerkEffectsWithData()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerGenerator.PrefabPath);
            if (prefab == null)
            {
                Assert.Inconclusive($"{PlayerGenerator.PrefabPath} yok; Tools/Last Haven/Generate Prefabs çalıştır.");
            }

            var effects = prefab.GetComponent<PlayerPerkEffects>();
            Assert.IsNotNull(effects);
            Assert.IsNotNull(new SerializedObject(effects).FindProperty("_data").objectReferenceValue);
        }

        private static PerkDefinitions.Definition Find(string id)
        {
            foreach (PerkDefinitions.Definition definition in PerkDefinitions.All)
            {
                if (string.Equals(definition.Id, id, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            Assert.Fail($"Tanım yok: {id}");
            return null;
        }
    }
}
