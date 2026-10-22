using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;

namespace LastHaven.Editor
{
    /// <summary>
    /// GDD §13 perk tablosu (14 perk). <see cref="ProgressionGenerator"/> sadece ilk oluşturmada yazar. Bayrak perk'lerinin değerleri
    /// bayrağı soran modülün config'inde (Combat/Building/Enemies <c>*PerkConfig</c>, Vampir <c>PlayerData</c>).
    /// </summary>
    public static class PerkDefinitions
    {
        public const string DataFolder = "Assets/_Project/Data/Progression";
        public const string PerksFolder = DataFolder + "/Perks";

        /// <summary>GDD tablosunun sırasıyla.</summary>
        public static IReadOnlyList<Definition> All { get; } = new[]
        {
            new Definition("QuickTrigger", "quick_trigger", "Hızlı Tetik", "Atış hızı +%15.", PerkRarity.Common, 3,
                () => new PerkEffect[] { Percent(StatType.PlayerFireRate, 0.15f) }),
            new Definition("HeavyRounds", "heavy_rounds", "Ağır Mermi", "Silah hasarı +%15.", PerkRarity.Common, 3,
                () => new PerkEffect[] { Percent(StatType.PlayerDamage, 0.15f) }),
            new Definition("ThickSkin", "thick_skin", "Kalın Deri", "Maks. can +25 ve 25 can yeniler.", PerkRarity.Common, 3,
                () => new PerkEffect[] { new StatModifierEffect(StatType.PlayerMaxHealth, StatModifierType.Flat, 25f), new HealEffect(25f) }),
            new Definition("Magnet", "magnet", "Mıknatıs", "Jeton toplama yarıçapı +%50.", PerkRarity.Common, 2,
                () => new PerkEffect[] { Percent(StatType.PickupRadius, 0.5f) }),
            new Definition("SturdyWalls", "sturdy_walls", "Sağlam Duvar", "Barikatların maks. canı +%30.", PerkRarity.Common, 2,
                () => new PerkEffect[] { Percent(StatType.BarricadeMaxHealth, 0.3f) }),
            new Definition("Greedy", "greedy", "Açgözlü", "Jeton değeri +%25.", PerkRarity.Rare, 2,
                () => new PerkEffect[] { Percent(StatType.CoinValue, 0.25f) }),
            new Definition("Adrenaline", "adrenaline", "Adrenalin", "Atılma bekleme süresi −%40.", PerkRarity.Rare, 1,
                () => new PerkEffect[] { Percent(StatType.DashCooldown, -0.4f) }),
            new Definition("TurretMaster", "turret_master", "Taret Ustası", "Taret atış hızı +%25.", PerkRarity.Rare, 2,
                () => new PerkEffect[] { Percent(StatType.TurretFireRate, 0.25f) }),
            new Definition("Vampire", "vampire", "Vampir", "Her öldürme 2 can yeniler.", PerkRarity.Rare, 1,
                () => new PerkEffect[] { new FlagEffect(PerkFlag.Vampire) }),
            new Definition("Hunter", "hunter", "Avcı", "Yavaşlamış düşmanlara +%50 hasar.", PerkRarity.Rare, 1,
                () => new PerkEffect[] { new FlagEffect(PerkFlag.Hunter) }),
            new Definition("Ricochet", "ricochet", "Seken Mermi", "Mermi vurduğu düşmandan en yakın ikinci düşmana seker (%50 hasar).",
                PerkRarity.Epic, 1, () => new PerkEffect[] { new FlagEffect(PerkFlag.Ricochet) }),
            new Definition("ThornWalls", "thorn_walls", "Dikenli Duvar", "Barikatlar aldıkları yakın dövüş hasarının %30'unu saldırana yansıtır.",
                PerkRarity.Epic, 1, () => new PerkEffect[] { new FlagEffect(PerkFlag.ThornWalls) }),
            new Definition("ShockTurret", "shock_turret", "Şok Taret", "Taret vuruşları düşmanı 1 sn %25 yavaşlatır.", PerkRarity.Epic, 1,
                () => new PerkEffect[] { new FlagEffect(PerkFlag.ShockTurret) }),
            new Definition("ChainExplosion", "chain_explosion", "Zincirleme Son", "Öldürdüğün düşman %10 şansla patlar (3 m, 40 hasar).",
                PerkRarity.Epic, 1, () => new PerkEffect[] { new FlagEffect(PerkFlag.ChainExplosion) }),
        };

        private static StatModifierEffect Percent(StatType stat, float value)
        {
            return new StatModifierEffect(stat, StatModifierType.PercentAdd, value);
        }

        /// <summary>Bir perk'in varsayılanları. Etkiler her çağrıda yeni nesnelerdir (asset'e managed reference olarak yazılır).</summary>
        public sealed class Definition
        {
            private readonly Func<PerkEffect[]> _createEffects;

            public Definition(string assetName, string id, string displayName, string description, PerkRarity rarity, int maxStacks,
                Func<PerkEffect[]> createEffects)
            {
                AssetName = assetName;
                Id = id;
                DisplayName = displayName;
                Description = description;
                Rarity = rarity;
                MaxStacks = maxStacks;
                _createEffects = createEffects;
            }

            public string AssetName { get; }

            public string Id { get; }

            public string DisplayName { get; }

            public string Description { get; }

            public PerkRarity Rarity { get; }

            public int MaxStacks { get; }

            public PerkEffect[] CreateEffects()
            {
                return _createEffects();
            }
        }
    }
}
