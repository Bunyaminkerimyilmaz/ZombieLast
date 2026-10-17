using System.Collections.Generic;
using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>
    /// Market öğelerinin varsayılanları (GDD §6 silahlar ve yükseltmeler, §10 market tablosu). <see cref="ShopGenerator"/> sadece
    /// ilk oluşturmada yazar. Silahlara id ile başvurulur (<c>WeaponsGenerator</c>: pistol, shotgun, rifle).
    /// </summary>
    public static class ShopDefinitions
    {
        public const string DataFolder = "Assets/_Project/Data/Shop";
        public const string ItemsFolder = DataFolder + "/Items";

        public const string UnlockShotgunId = "unlock_shotgun";
        public const string UnlockRifleId = "unlock_rifle";

        // GDD §6 yükseltme fiyatları; seviye sayısı = fiyat sayısı (WeaponUpgradeConfig'teki en yüksek seviyelerle aynı olmalı).
        private static readonly int[] DamagePrices = { 150, 300, 500, 800, 1200 };
        private static readonly int[] FireRatePrices = { 200, 450, 800 };
        private static readonly int[] MagSizePrices = { 150, 350, 600 };

        /// <summary>Tüm öğeler, arayüz sırasıyla: silahlar, yükseltmeler (silah başına hasar/atış hızı/şarjör, mıknatıs), malzeme.</summary>
        public static IReadOnlyList<Definition> All { get; } = Build();

        private static Definition[] Build()
        {
            var all = new List<Definition>
            {
                Unlock("UnlockShotgun", UnlockShotgunId, "Pompalı", "Yakın mesafede yıkıcı: 8 saçma × 12 hasar, 12 m menzil.", "shotgun", 400),
                Unlock("UnlockRifle", UnlockRifleId, "Tüfek", "Seri atış: saniyede 10 mermi × 18 hasar, 35 m menzil.", "rifle", 900),
            };

            AddUpgrades(all, "Pistol", "pistol", "Tabanca", requiredItemId: null);
            AddUpgrades(all, "Shotgun", "shotgun", "Pompalı", UnlockShotgunId);
            AddUpgrades(all, "Rifle", "rifle", "Tüfek", UnlockRifleId);
            all.Add(new Definition("Magnet", "magnet", "Mıknatıs", "Jeton toplama yarıçapı her seviyede +1 m.", ShopCategory.Upgrades,
                new[] { 100, 200, 350 }, 3, ShopEffectType.PickupRadius, null, default, 1f, null));

            all.Add(Supply("AmmoShotgun", "ammo_shotgun", "Pompalı Mermisi", "12 yedek pompalı mermisi.", ShopEffectType.BuyAmmo,
                40, 12f, "shotgun", UnlockShotgunId));
            all.Add(Supply("AmmoRifle", "ammo_rifle", "Tüfek Mermisi", "60 yedek tüfek mermisi.", ShopEffectType.BuyAmmo,
                50, 60f, "rifle", UnlockRifleId));
            all.Add(Supply("HealthKit", "health_kit", "Can Paketi", "Anında 50 can yeniler.", ShopEffectType.HealthKit, 60, 50f));
            all.Add(Supply("Fuel", "fuel", "Yakıt", "Jeneratöre 10 birim yakıt.", ShopEffectType.BuyFuel, 15, 10f));
            return all.ToArray();
        }

        private static Definition Unlock(string assetName, string id, string displayName, string description, string weaponId, int price)
        {
            return new Definition(assetName, id, displayName, description, ShopCategory.Weapons, new[] { price }, 1,
                ShopEffectType.UnlockWeapon, weaponId, default, 0f, null);
        }

        private static void AddUpgrades(List<Definition> all, string assetPrefix, string weaponId, string weaponName, string requiredItemId)
        {
            all.Add(Upgrade(assetPrefix + "Damage", weaponId + "_damage", weaponName + " Hasarı", "Her seviye hasarı %10 artırır.",
                weaponId, WeaponUpgradeType.Damage, DamagePrices, requiredItemId));
            all.Add(Upgrade(assetPrefix + "FireRate", weaponId + "_fire_rate", weaponName + " Atış Hızı", "Her seviye atış hızını %8 artırır.",
                weaponId, WeaponUpgradeType.FireRate, FireRatePrices, requiredItemId));
            all.Add(Upgrade(assetPrefix + "MagSize", weaponId + "_mag_size", weaponName + " Şarjörü",
                "Her seviye şarjör kapasitesini %25 artırır.", weaponId, WeaponUpgradeType.MagSize, MagSizePrices, requiredItemId));
        }

        private static Definition Upgrade(
            string assetName,
            string id,
            string displayName,
            string description,
            string weaponId,
            WeaponUpgradeType type,
            int[] prices,
            string requiredItemId)
        {
            return new Definition(assetName, id, displayName, description, ShopCategory.Upgrades, prices, prices.Length,
                ShopEffectType.WeaponUpgrade, weaponId, type, 0f, requiredItemId);
        }

        private static Definition Supply(
            string assetName,
            string id,
            string displayName,
            string description,
            ShopEffectType effect,
            int price,
            float value,
            string targetId = null,
            string requiredItemId = null)
        {
            return new Definition(assetName, id, displayName, description, ShopCategory.Supplies, new[] { price }, 0, effect, targetId,
                default, value, requiredItemId);
        }

        /// <summary>Bir market öğesinin varsayılanları. <see cref="MaxLevel"/> 0 = sınırsız (sarf malzemesi).</summary>
        public readonly struct Definition
        {
            public Definition(
                string assetName,
                string id,
                string displayName,
                string description,
                ShopCategory category,
                int[] prices,
                int maxLevel,
                ShopEffectType effect,
                string targetId,
                WeaponUpgradeType upgradeType,
                float value,
                string requiredItemId)
            {
                AssetName = assetName;
                Id = id;
                DisplayName = displayName;
                Description = description;
                Category = category;
                Prices = prices;
                MaxLevel = maxLevel;
                Effect = effect;
                TargetId = targetId ?? string.Empty;
                UpgradeType = upgradeType;
                Value = value;
                RequiredItemId = requiredItemId ?? string.Empty;
            }

            public string AssetName { get; }
            public string Id { get; }
            public string DisplayName { get; }
            public string Description { get; }
            public ShopCategory Category { get; }
            public int[] Prices { get; }
            public int MaxLevel { get; }
            public ShopEffectType Effect { get; }
            public string TargetId { get; }
            public WeaponUpgradeType UpgradeType { get; }
            public float Value { get; }
            public string RequiredItemId { get; }
        }
    }
}
