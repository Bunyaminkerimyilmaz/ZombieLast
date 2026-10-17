using System;
using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Editor;
using LastHaven.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Market üretimi (10.1): tanımlar GDD §6/§10 tablolarıyla, yükseltme seviyeleri <see cref="WeaponUpgradeConfig"/>'le, hedef silah
    /// id'leri loadout'la uyumlu; üretilen katalog tüm öğeleri içeriyor; oyuncu prefab'ında <see cref="PlayerPurchaseEffects"/> var.
    /// </summary>
    public class ShopGenerationTests
    {
        private const float Tolerance = 0.0001f;

        private static readonly string[] WeaponIds = { "pistol", "shotgun", "rifle" };

        [Test]
        public void Definitions_MatchGddMarketTable()
        {
            AssertItem("unlock_shotgun", ShopCategory.Weapons, ShopEffectType.UnlockWeapon, new[] { 400 }, 1, "shotgun", 0f);
            AssertItem("unlock_rifle", ShopCategory.Weapons, ShopEffectType.UnlockWeapon, new[] { 900 }, 1, "rifle", 0f);
            AssertItem("ammo_shotgun", ShopCategory.Supplies, ShopEffectType.BuyAmmo, new[] { 40 }, 0, "shotgun", 12f);
            AssertItem("ammo_rifle", ShopCategory.Supplies, ShopEffectType.BuyAmmo, new[] { 50 }, 0, "rifle", 60f);
            AssertItem("health_kit", ShopCategory.Supplies, ShopEffectType.HealthKit, new[] { 60 }, 0, string.Empty, 50f);
            AssertItem("fuel", ShopCategory.Supplies, ShopEffectType.BuyFuel, new[] { 15 }, 0, string.Empty, 10f);
            AssertItem("magnet", ShopCategory.Upgrades, ShopEffectType.PickupRadius, new[] { 100, 200, 350 }, 3, string.Empty, 1f);
        }

        [Test]
        public void Definitions_EveryWeaponHasThreeUpgradesWithGddPrices()
        {
            foreach (string weapon in WeaponIds)
            {
                AssertUpgrade(weapon + "_damage", weapon, WeaponUpgradeType.Damage, new[] { 150, 300, 500, 800, 1200 });
                AssertUpgrade(weapon + "_fire_rate", weapon, WeaponUpgradeType.FireRate, new[] { 200, 450, 800 });
                AssertUpgrade(weapon + "_mag_size", weapon, WeaponUpgradeType.MagSize, new[] { 150, 350, 600 });
            }

            Assert.AreEqual(2 + 9 + 1 + 4, ShopDefinitions.All.Count, "2 silah, 9 silah yükseltmesi, mıknatıs, 4 malzeme.");
        }

        [Test]
        public void Definitions_UpgradeMaxLevelsMatchWeaponUpgradeConfig()
        {
            var config = ScriptableObject.CreateInstance<WeaponUpgradeConfig>();
            try
            {
                foreach (ShopDefinitions.Definition definition in ShopDefinitions.All)
                {
                    if (definition.Effect == ShopEffectType.WeaponUpgrade)
                    {
                        Assert.AreEqual(config.GetMaxLevel(definition.UpgradeType), definition.MaxLevel, definition.Id);
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Definitions_IdsUniqueAndReferencesValid()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var assetNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (ShopDefinitions.Definition definition in ShopDefinitions.All)
            {
                Assert.IsTrue(ids.Add(definition.Id), $"Tekrarlanan id: {definition.Id}");
                Assert.IsTrue(assetNames.Add(definition.AssetName), $"Tekrarlanan asset adı: {definition.AssetName}");
                Assert.IsFalse(string.IsNullOrEmpty(definition.DisplayName), definition.Id);
                Assert.IsFalse(string.IsNullOrEmpty(definition.Description), definition.Id);
                Assert.IsNotEmpty(definition.Prices, definition.Id);
                Assert.IsTrue(definition.MaxLevel == 0 || definition.Prices.Length == definition.MaxLevel,
                    $"{definition.Id}: seviyeli öğede fiyat sayısı = seviye sayısı.");
            }

            foreach (ShopDefinitions.Definition definition in ShopDefinitions.All)
            {
                if (definition.RequiredItemId.Length > 0)
                {
                    Assert.IsTrue(ids.Contains(definition.RequiredItemId), $"{definition.Id}: ön koşul {definition.RequiredItemId} yok.");
                }

                bool weaponEffect = definition.Effect == ShopEffectType.UnlockWeapon || definition.Effect == ShopEffectType.WeaponUpgrade
                                    || definition.Effect == ShopEffectType.BuyAmmo;
                if (weaponEffect)
                {
                    CollectionAssert.Contains(WeaponIds, definition.TargetId, definition.Id);
                }
            }
        }

        [Test]
        public void Definitions_LockedWeaponItemsRequireUnlock()
        {
            Assert.AreEqual(string.Empty, Find("pistol_damage").RequiredItemId, "Tabanca baştan açık.");
            Assert.AreEqual(ShopDefinitions.UnlockShotgunId, Find("shotgun_fire_rate").RequiredItemId);
            Assert.AreEqual(ShopDefinitions.UnlockShotgunId, Find("ammo_shotgun").RequiredItemId);
            Assert.AreEqual(ShopDefinitions.UnlockRifleId, Find("rifle_mag_size").RequiredItemId);
            Assert.AreEqual(ShopDefinitions.UnlockRifleId, Find("ammo_rifle").RequiredItemId);
        }

        [Test]
        public void Assets_CatalogContainsAllItems()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(ShopGenerator.CatalogPath);
            if (catalog == null)
            {
                Assert.Inconclusive("Market asset'leri yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (ShopItemData item in catalog.Items)
            {
                Assert.IsNotNull(item);
                ids.Add(item.Id);
            }

            foreach (ShopDefinitions.Definition definition in ShopDefinitions.All)
            {
                Assert.IsTrue(ids.Contains(definition.Id), $"Katalogda yok: {definition.Id}");
                var item = AssetDatabase.LoadAssetAtPath<ShopItemData>(ShopGenerator.GetItemPath(definition.AssetName));
                Assert.IsNotNull(item, definition.AssetName);
                Assert.AreEqual(definition.Id, item.Id);
            }

            Assert.DoesNotThrow(() => new Shop(catalog.Items, new Wallet(0, 0)), "Katalog id'leri benzersiz.");
        }

        [Test]
        public void Assets_WeaponTargetsExistInLoadout()
        {
            var loadout = AssetDatabase.LoadAssetAtPath<WeaponLoadoutData>(PlayerWeaponsGenerator.LoadoutPath);
            if (loadout == null)
            {
                Assert.Inconclusive("Loadout yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            var loadoutIds = new List<string>();
            foreach (WeaponSlotDefinition slot in loadout.Slots)
            {
                if (slot.Weapon != null)
                {
                    loadoutIds.Add(slot.Weapon.Id);
                }
            }

            foreach (ShopDefinitions.Definition definition in ShopDefinitions.All)
            {
                if (definition.TargetId.Length > 0)
                {
                    CollectionAssert.Contains(loadoutIds, definition.TargetId, definition.Id);
                }
            }
        }

        [Test]
        public void Assets_PlayerPrefabAppliesPurchases()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerGenerator.PrefabPath);
            if (prefab == null)
            {
                Assert.Inconclusive("Player prefab'ı yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.IsNotNull(prefab.GetComponent<PlayerPurchaseEffects>());
        }

        private static ShopDefinitions.Definition Find(string id)
        {
            foreach (ShopDefinitions.Definition definition in ShopDefinitions.All)
            {
                if (string.Equals(definition.Id, id, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            Assert.Fail($"Tanım yok: {id}");
            return default;
        }

        private static void AssertItem(
            string id,
            ShopCategory category,
            ShopEffectType effect,
            int[] prices,
            int maxLevel,
            string targetId,
            float value)
        {
            ShopDefinitions.Definition definition = Find(id);
            Assert.AreEqual(category, definition.Category, id);
            Assert.AreEqual(effect, definition.Effect, id);
            CollectionAssert.AreEqual(prices, definition.Prices, id);
            Assert.AreEqual(maxLevel, definition.MaxLevel, id);
            Assert.AreEqual(targetId, definition.TargetId, id);
            Assert.AreEqual(value, definition.Value, Tolerance, id);
        }

        private static void AssertUpgrade(string id, string weaponId, WeaponUpgradeType type, int[] prices)
        {
            ShopDefinitions.Definition definition = Find(id);
            Assert.AreEqual(ShopCategory.Upgrades, definition.Category, id);
            Assert.AreEqual(ShopEffectType.WeaponUpgrade, definition.Effect, id);
            Assert.AreEqual(weaponId, definition.TargetId, id);
            Assert.AreEqual(type, definition.UpgradeType, id);
            CollectionAssert.AreEqual(prices, definition.Prices, id);
            Assert.AreEqual(prices.Length, definition.MaxLevel, id);
        }
    }
}
