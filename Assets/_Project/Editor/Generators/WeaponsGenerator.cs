using System.Collections.Generic;
using LastHaven.Combat;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// GDD §6 silahlarının <see cref="WeaponData"/> asset'leri (<c>Data/Weapons/Pistol|Shotgun|Rifle.asset</c>) ve
    /// <see cref="WeaponUpgradeConfig"/> ve <see cref="CombatPerkConfig"/> (9.3). Varsa dokunulmaz (inspector'daki denge ayarları
    /// korunur); yoksa GDD değerleriyle oluşturulur.
    /// </summary>
    public sealed class WeaponsGenerator : IGenerationStep
    {
        public const string Folder = "Assets/_Project/Data/Weapons";
        public const string UpgradeConfigPath = Folder + "/WeaponUpgradeConfig.asset";
        public const string PerkConfigPath = Folder + "/CombatPerkConfig.asset";

        // GDD §6. Mermi izi renkleri GDD'de yok; silahları ayırt etmek için seçildi.
        private static readonly WeaponDefinition[] Definitions =
        {
            new WeaponDefinition("Pistol", "pistol", "Tabanca", damage: 20f, pellets: 1, fireRate: 4f,
                magSize: 12, reserve: WeaponData.InfiniteReserve, reloadTime: 1.2f, spread: 2f, range: 25f, tracer: "#FFE08A"),
            new WeaponDefinition("Shotgun", "shotgun", "Pompalı", damage: 12f, pellets: 8, fireRate: 1.1f,
                magSize: 6, reserve: 36, reloadTime: 2f, spread: 18f, range: 12f, tracer: "#FF9F43"),
            new WeaponDefinition("Rifle", "rifle", "Tüfek", damage: 18f, pellets: 1, fireRate: 10f,
                magSize: 30, reserve: 180, reloadTime: 1.8f, spread: 4f, range: 35f, tracer: "#8FE3FF"),
        };

        public int Order => 400;

        public string Name => "Weapons";

        public static string GetAssetPath(string assetName)
        {
            return $"{Folder}/{assetName}.asset";
        }

        /// <summary>Tüm silah asset'lerini yükler; eksik olanları oluşturur. Diğer üreticiler (ör. oyuncu silahları) buradan alır.</summary>
        public static List<WeaponData> LoadOrCreateAll()
        {
            var weapons = new List<WeaponData>(Definitions.Length);
            for (int i = 0; i < Definitions.Length; i++)
            {
                weapons.Add(LoadOrCreate(Definitions[i]));
            }

            return weapons;
        }

        /// <summary>Yükseltme verisini yükler; yoksa GDD §6 varsayılanlarıyla oluşturur.</summary>
        public static WeaponUpgradeConfig LoadOrCreateUpgradeConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<WeaponUpgradeConfig>(UpgradeConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Weapons: {UpgradeConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>Perk'lerin silah etkilerini (GDD §13 Avcı; 9.3) yükler; yoksa varsayılanlarla oluşturur.</summary>
        public static CombatPerkConfig LoadOrCreatePerkConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<CombatPerkConfig>(PerkConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Weapons: {PerkConfigPath} oluşturuldu.");
            }

            return config;
        }

        public void GenerateData()
        {
            LoadOrCreateAll();
            LoadOrCreateUpgradeConfig();
            LoadOrCreatePerkConfig();
        }

        public void GeneratePrefabs()
        {
        }

        private static WeaponData LoadOrCreate(WeaponDefinition definition)
        {
            string path = GetAssetPath(definition.AssetName);
            var weapon = EditorAssetUtility.LoadOrCreate<WeaponData>(path, out bool created);
            if (created)
            {
                Apply(weapon, definition);
                Debug.Log($"[Last Haven] Weapons: {path} oluşturuldu.");
            }

            return weapon;
        }

        private static void Apply(WeaponData weapon, WeaponDefinition definition)
        {
            var serialized = new SerializedObject(weapon);
            serialized.FindProperty("_id").stringValue = definition.Id;
            serialized.FindProperty("_displayName").stringValue = definition.DisplayName;
            serialized.FindProperty("_damage").floatValue = definition.Damage;
            serialized.FindProperty("_pelletCount").intValue = definition.Pellets;
            serialized.FindProperty("_fireRate").floatValue = definition.FireRate;
            serialized.FindProperty("_spreadAngle").floatValue = definition.Spread;
            serialized.FindProperty("_range").floatValue = definition.Range;
            serialized.FindProperty("_magSize").intValue = definition.MagSize;
            serialized.FindProperty("_reserveAmmo").intValue = definition.Reserve;
            serialized.FindProperty("_reloadTime").floatValue = definition.ReloadTime;
            serialized.FindProperty("_tracerColor").colorValue = definition.TracerColor;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private readonly struct WeaponDefinition
        {
            public WeaponDefinition(
                string assetName,
                string id,
                string displayName,
                float damage,
                int pellets,
                float fireRate,
                int magSize,
                int reserve,
                float reloadTime,
                float spread,
                float range,
                string tracer)
            {
                AssetName = assetName;
                Id = id;
                DisplayName = displayName;
                Damage = damage;
                Pellets = pellets;
                FireRate = fireRate;
                MagSize = magSize;
                Reserve = reserve;
                ReloadTime = reloadTime;
                Spread = spread;
                Range = range;
                ColorUtility.TryParseHtmlString(tracer, out Color color);
                TracerColor = color;
            }

            public string AssetName { get; }
            public string Id { get; }
            public string DisplayName { get; }
            public float Damage { get; }
            public int Pellets { get; }
            public float FireRate { get; }
            public int MagSize { get; }
            public int Reserve { get; }
            public float ReloadTime { get; }
            public float Spread { get; }
            public float Range { get; }
            public Color TracerColor { get; }
        }
    }
}
