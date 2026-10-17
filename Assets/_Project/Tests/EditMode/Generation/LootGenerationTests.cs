using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Ganimet prefab'ları ve asset'leri (11.1): sandık Interactable layer'ında katı kutu (pivot zeminde, mermi maskesinde değil),
    /// bidon/kutu Pickup layer'ında trigger + ödül türü; kök/Visual ayrımı; üretilen asset'ler ve config bağlantıları.
    /// Hiyerarşi testleri asset oluşturmaz (sahnede kurulup silinir).
    /// </summary>
    public class LootGenerationTests
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
        public void BuildCrate_RootIsSolidInteractableBoxOnGround()
        {
            _root = LootPrefabs.BuildCrate(null);

            Assert.AreEqual(GameLayers.Interactable, _root.layer);
            Assert.IsNotNull(_root.GetComponent<LootCrate>());
            Assert.IsNull(_root.GetComponent<Renderer>(), "Kökte mesh olmamalı.");

            var collider = _root.GetComponent<BoxCollider>();
            Assert.IsNotNull(collider);
            Assert.IsFalse(collider.isTrigger, "Oyuncu sandığın içinden geçmemeli.");
            Assert.AreEqual(collider.size.y * 0.5f, collider.center.y, Tolerance, "Pivot zeminde.");
        }

        [Test]
        public void BuildCrate_VisualCubeWithoutCollider()
        {
            _root = LootPrefabs.BuildCrate(null);

            Transform visual = VisualRoot.Find(_root.transform);
            Assert.IsNotNull(visual);
            Transform body = visual.Find(LootPrefabs.BodyName);
            Assert.IsNotNull(body);
            Assert.IsNotNull(body.GetComponent<MeshRenderer>());
            Assert.IsNull(body.GetComponent<Collider>(), "Görselde collider olmamalı.");
        }

        [TestCase(LootRewardType.Fuel)]
        [TestCase(LootRewardType.AmmoPack)]
        public void BuildPickup_RootIsPickupTriggerWithRewardType(LootRewardType type)
        {
            _root = LootPrefabs.BuildPickup("Pickup", type, Vector3.one * 0.3f, null);

            Assert.AreEqual(GameLayers.Pickup, _root.layer);
            Assert.IsTrue(_root.CompareTag(GameTags.Pickup));
            Assert.AreEqual(type, _root.GetComponent<LootPickup>().RewardType);
            Assert.IsTrue(_root.GetComponent<SphereCollider>().isTrigger, "CharacterController takılmamalı.");
            Assert.IsNotNull(VisualRoot.Find(_root.transform).Find(LootPrefabs.BodyName));
        }

        [Test]
        public void CrateLayer_NotInPlayerWeaponHitMask()
        {
            var loadout = ScriptableObject.CreateInstance<LastHaven.Player.WeaponLoadoutData>();
            try
            {
                Assert.IsFalse(GameLayers.Contains(loadout.HitMask, GameLayers.Interactable), "Mermiler sandıktan geçer.");
            }
            finally
            {
                Object.DestroyImmediate(loadout);
            }
        }

        [Test]
        public void Assets_ExistAndAreLinked()
        {
            var config = AssetDatabase.LoadAssetAtPath<LootConfig>(LootGenerator.ConfigPath);
            var table = AssetDatabase.LoadAssetAtPath<LootTableData>(LootGenerator.TablePath);
            if (config == null || table == null)
            {
                Assert.Inconclusive("Ganimet asset'leri yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.AreSame(table, config.LootTable);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<LootCrate>(LootPrefabs.CratePath), config.CratePrefab);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<LootPickup>(LootPrefabs.FuelPickupPath), config.FuelPickupPrefab);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<LootPickup>(LootPrefabs.AmmoPickupPath), config.AmmoPickupPrefab);
            Assert.AreEqual(LootRewardType.Fuel, config.FuelPickupPrefab.RewardType);
            Assert.AreEqual(LootRewardType.AmmoPack, config.AmmoPickupPrefab.RewardType);
            Assert.AreSame(config.FuelPickupPrefab, config.GetPickupPrefab(LootRewardType.Fuel));
            Assert.IsNull(config.GetPickupPrefab(LootRewardType.Money), "Para jeton olarak saçılır.");
            Assert.GreaterOrEqual(config.CratePrewarm, config.CratesPerDay, "Gündüz başında Instantiate olmasın.");
        }
    }
}
