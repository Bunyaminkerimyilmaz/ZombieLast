using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Testler için bellek içi <see cref="ShopItemData"/>. Oluşturulanlar <see cref="DestroyAll"/> ile silinir.</summary>
    internal sealed class TestShopItems
    {
        private readonly List<Object> _created = new List<Object>();

        public ShopItemData Create(
            string id,
            ShopEffectType effect,
            int[] prices,
            int maxLevel,
            string targetId = "",
            float value = 0f,
            string requiredItemId = "",
            WeaponUpgradeType upgradeType = WeaponUpgradeType.Damage)
        {
            var item = ScriptableObject.CreateInstance<ShopItemData>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            SerializedProperty priceList = serialized.FindProperty("_prices");
            priceList.arraySize = prices.Length;
            for (int i = 0; i < prices.Length; i++)
            {
                priceList.GetArrayElementAtIndex(i).intValue = prices[i];
            }

            serialized.FindProperty("_maxLevel").intValue = maxLevel;
            serialized.FindProperty("_effect").enumValueIndex = (int)effect;
            serialized.FindProperty("_targetId").stringValue = targetId;
            serialized.FindProperty("_value").floatValue = value;
            serialized.FindProperty("_requiredItemId").stringValue = requiredItemId;
            serialized.FindProperty("_upgradeType").enumValueIndex = (int)upgradeType;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            return item;
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

    /// <summary>Evresi elle ayarlanan oyun akışı.</summary>
    internal sealed class FakeGameState : IGameStateService
    {
        public GamePhase CurrentPhase { get; set; } = GamePhase.Day;

        public int NightIndex { get; set; }

        public float PhaseTimeRemaining { get; set; }
    }

    /// <summary>Canı elle ayarlanan oyuncu (sahnesiz).</summary>
    internal sealed class FakePlayer : IPlayer
    {
        public Transform Transform => null;

        public Vector3 Position => Vector3.zero;

        public Vector3 AimPoint => Vector3.zero;

        public Vector3 AimDirection => Vector3.forward;

        public bool IsAlive { get; set; } = true;

        public float CurrentHealth { get; set; } = 100f;

        public float MaxHealth { get; set; } = 100f;

        public float PickupRadius => 3f;
    }
}
