using System.Collections.Generic;
using LastHaven.Combat;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Testler için bellek içi WeaponData / WeaponUpgradeConfig. Oluşturulanlar <see cref="DestroyAll"/> ile silinir.</summary>
    internal sealed class TestWeapons
    {
        private readonly List<Object> _created = new List<Object>();

        public WeaponData Create(
            float damage = 20f,
            int pellets = 1,
            float fireRate = 4f,
            int magSize = 12,
            int reserve = WeaponData.InfiniteReserve,
            float reloadTime = 1.2f,
            string id = "test_weapon")
        {
            var weapon = ScriptableObject.CreateInstance<WeaponData>();
            var serialized = new SerializedObject(weapon);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_damage").floatValue = damage;
            serialized.FindProperty("_pelletCount").intValue = pellets;
            serialized.FindProperty("_fireRate").floatValue = fireRate;
            serialized.FindProperty("_magSize").intValue = magSize;
            serialized.FindProperty("_reserveAmmo").intValue = reserve;
            serialized.FindProperty("_reloadTime").floatValue = reloadTime;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(weapon);
            return weapon;
        }

        /// <summary>GDD §6 varsayılanlarıyla (+%10 / +%8 / +%25; 5 / 3 / 3 seviye).</summary>
        public WeaponUpgradeConfig CreateUpgrades()
        {
            var upgrades = ScriptableObject.CreateInstance<WeaponUpgradeConfig>();
            _created.Add(upgrades);
            return upgrades;
        }

        public void DestroyAll()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                Object.DestroyImmediate(_created[i]);
            }

            _created.Clear();
        }
    }
}
