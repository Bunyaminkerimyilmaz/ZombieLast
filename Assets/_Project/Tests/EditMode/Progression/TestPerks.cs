using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Testler için bellek içi <see cref="PerkData"/>. Oluşturulanlar <see cref="DestroyAll"/> ile silinir.</summary>
    internal sealed class TestPerks
    {
        private readonly List<Object> _created = new List<Object>();

        public PerkData Create(string id, PerkRarity rarity = PerkRarity.Common, int maxStacks = 1, params PerkEffect[] effects)
        {
            var perk = ScriptableObject.CreateInstance<PerkData>();
            perk.name = id;
            var serialized = new SerializedObject(perk);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("_maxStacks").intValue = maxStacks;
            SerializedProperty list = serialized.FindProperty("_effects");
            list.arraySize = effects.Length;
            for (int i = 0; i < effects.Length; i++)
            {
                list.GetArrayElementAtIndex(i).managedReferenceValue = effects[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(perk);
            return perk;
        }

        /// <summary>Üreticinin tanımından (GDD §13 değerleri).</summary>
        public PerkData Create(LastHaven.Editor.PerkDefinitions.Definition definition)
        {
            var perk = ScriptableObject.CreateInstance<PerkData>();
            perk.name = definition.AssetName;
            LastHaven.Editor.ProgressionGenerator.Apply(perk, definition);
            _created.Add(perk);
            return perk;
        }

        public void DestroyAll()
        {
            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }
    }
}
