using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Player;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Oyuncunun silah yuvaları: <c>Data/Player/WeaponLoadout.asset</c>. İlk oluşturmada Tabanca (açık), Pompalı, Tüfek
    /// sırasıyla, yükseltme verisi ve perk etkileriyle (<see cref="CombatPerkConfig"/>) doldurulur. Varsa sadece boş yükseltme ve
    /// perk referansları doldurulur.
    /// Player prefab'ına bağlanması <see cref="PlayerGenerator"/>'da.
    /// </summary>
    public sealed class PlayerWeaponsGenerator : IGenerationStep
    {
        public const string LoadoutPath = "Assets/_Project/Data/Player/WeaponLoadout.asset";

        // WeaponsGenerator.LoadOrCreateAll sırası: Pistol, Shotgun, Rifle. Başta sadece ilk yuva (Tabanca) açık.
        private const int StartingUnlockedSlots = 1;

        public int Order => 450;

        public string Name => "Player Weapons";

        /// <summary>Loadout'u yükler; yoksa oluşturur. Player prefab üreticisi buradan alır.</summary>
        public static WeaponLoadoutData LoadOrCreateLoadout()
        {
            var loadout = EditorAssetUtility.LoadOrCreate<WeaponLoadoutData>(LoadoutPath, out bool created);
            WeaponUpgradeConfig upgrades = WeaponsGenerator.LoadOrCreateUpgradeConfig();
            CombatPerkConfig perks = WeaponsGenerator.LoadOrCreatePerkConfig();
            if (created)
            {
                Fill(loadout, WeaponsGenerator.LoadOrCreateAll(), upgrades, perks);
                Debug.Log($"[Last Haven] Player Weapons: {LoadoutPath} oluşturuldu.");
                return loadout;
            }

            if (EditorAssetUtility.AssignIfMissing(loadout, "_upgrades", upgrades))
            {
                AssetDatabase.SaveAssetIfDirty(loadout);
                Debug.Log($"[Last Haven] Player Weapons: {LoadoutPath} içinde boş yükseltme referansı dolduruldu.");
            }

            if (EditorAssetUtility.AssignIfMissing(loadout, "_perks", perks))
            {
                AssetDatabase.SaveAssetIfDirty(loadout);
                Debug.Log($"[Last Haven] Player Weapons: {LoadoutPath} içinde boş perk referansı dolduruldu.");
            }

            return loadout;
        }

        public void GenerateData()
        {
            LoadOrCreateLoadout();
        }

        public void GeneratePrefabs()
        {
        }

        private static void Fill(WeaponLoadoutData loadout, List<WeaponData> weapons, WeaponUpgradeConfig upgrades, CombatPerkConfig perks)
        {
            var serialized = new SerializedObject(loadout);
            SerializedProperty slots = serialized.FindProperty("_slots");
            slots.arraySize = weapons.Count;
            for (int i = 0; i < weapons.Count; i++)
            {
                SerializedProperty slot = slots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("_weapon").objectReferenceValue = weapons[i];
                slot.FindPropertyRelative("_unlockedAtStart").boolValue = i < StartingUnlockedSlots;
            }

            serialized.FindProperty("_upgrades").objectReferenceValue = upgrades;
            serialized.FindProperty("_perks").objectReferenceValue = perks;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(loadout);
        }
    }
}
