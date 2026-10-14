using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Player;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Oyuncunun silah yuvaları: <c>Data/Player/WeaponLoadout.asset</c>. İlk oluşturmada Tabanca (açık), Pompalı, Tüfek
    /// sırasıyla ve yükseltme verisiyle doldurulur. Varsa sadece boş yükseltme referansı doldurulur.
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
            if (created)
            {
                Fill(loadout, WeaponsGenerator.LoadOrCreateAll(), upgrades);
                Debug.Log($"[Last Haven] Player Weapons: {LoadoutPath} oluşturuldu.");
            }
            else if (EditorAssetUtility.AssignIfMissing(loadout, "_upgrades", upgrades))
            {
                AssetDatabase.SaveAssetIfDirty(loadout);
                Debug.Log($"[Last Haven] Player Weapons: {LoadoutPath} içinde boş yükseltme referansı dolduruldu.");
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

        private static void Fill(WeaponLoadoutData loadout, List<WeaponData> weapons, WeaponUpgradeConfig upgrades)
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
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(loadout);
        }
    }
}
