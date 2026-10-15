using System;
using System.Collections.Generic;
using LastHaven.Enemies;
using LastHaven.Waves;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="NightDefinition"/> → <see cref="NightData"/>, "eksikse ekle" mantığıyla:
    /// <list type="bullet">
    /// <item>Tanımdaki tiplerden gecede grubu olmayanlar eklenir; var olan gruba (adet, aralık, yön…) dokunulmaz.</item>
    /// <item>Boss ve gece olayı alanı boşsa doldurulur; doluysa dokunulmaz.</item>
    /// <item>Projede karşılığı olmayan içerik (Gün 12'deki tipler, boss, olaylar) atlanır ve <see cref="NightFillResult.Missing"/>'e yazılır.</item>
    /// </list>
    /// </summary>
    public static class NightDataFiller
    {
        private const string NightNumberField = "_nightNumber";
        private const string GroupsField = "_groups";
        private const string ModifierField = "_modifier";
        private const string BossDataField = "_bossData";
        private const string BossStartDelayField = "_bossStartDelay";
        private const string BossDirectionField = "_bossDirection";
        private const string GroupEnemyField = "_enemy";
        private const string GroupCountField = "_count";
        private const string GroupIntervalField = "_spawnInterval";
        private const string GroupDelayField = "_startDelay";
        private const string GroupDirectionsField = "_directions";

        /// <summary>Sadece yeni oluşturulan asset'e: gece numarası, varsa boss gecikmesi ve yönü.</summary>
        public static void ApplyNew(NightData night, NightDefinition definition)
        {
            var serialized = new SerializedObject(night);
            serialized.FindProperty(NightNumberField).intValue = definition.Number;
            if (definition.BossId != null)
            {
                serialized.FindProperty(BossStartDelayField).floatValue = definition.BossStartDelay;
                serialized.FindProperty(BossDirectionField).enumValueIndex = (int)definition.BossDirection;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Eksik grupları, boş boss ve olay referanslarını doldurur.</summary>
        public static NightFillResult Fill(NightData night, NightDefinition definition, WaveContentLookup lookup)
        {
            var result = new NightFillResult();
            var serialized = new SerializedObject(night);
            SerializedProperty groups = serialized.FindProperty(GroupsField);

            foreach (NightGroupDefinition group in definition.Groups)
            {
                if (ContainsEnemy(groups, group.EnemyId))
                {
                    continue;
                }

                if (!lookup.TryGetEnemy(group.EnemyId, out EnemyData enemy))
                {
                    result.Missing.Add($"{group.EnemyId} × {group.Count}");
                    continue;
                }

                AppendGroup(groups, enemy, group);
                result.AddedGroups++;
            }

            if (definition.BossId != null)
            {
                SerializedProperty boss = serialized.FindProperty(BossDataField);
                if (boss.objectReferenceValue == null)
                {
                    if (lookup.TryGetEnemy(definition.BossId, out EnemyData bossData))
                    {
                        boss.objectReferenceValue = bossData;
                        result.BossAssigned = true;
                    }
                    else
                    {
                        result.Missing.Add($"boss '{definition.BossId}'");
                    }
                }
            }

            if (definition.ModifierId != null)
            {
                SerializedProperty modifier = serialized.FindProperty(ModifierField);
                if (modifier.objectReferenceValue == null)
                {
                    if (lookup.TryGetModifier(definition.ModifierId, out NightModifierData modifierData))
                    {
                        modifier.objectReferenceValue = modifierData;
                        result.ModifierAssigned = true;
                    }
                    else
                    {
                        result.Missing.Add($"olay '{definition.ModifierId}'");
                    }
                }
            }

            if (result.Changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return result;
        }

        private static bool ContainsEnemy(SerializedProperty groups, string enemyId)
        {
            for (int i = 0; i < groups.arraySize; i++)
            {
                var enemy = groups.GetArrayElementAtIndex(i).FindPropertyRelative(GroupEnemyField).objectReferenceValue as EnemyData;
                if (enemy != null && string.Equals(enemy.Id, enemyId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendGroup(SerializedProperty groups, EnemyData enemy, NightGroupDefinition definition)
        {
            // arraySize++ son elemanı kopyalar; bütün alanlar açıkça yazılır.
            groups.arraySize++;
            SerializedProperty element = groups.GetArrayElementAtIndex(groups.arraySize - 1);
            element.FindPropertyRelative(GroupEnemyField).objectReferenceValue = enemy;
            element.FindPropertyRelative(GroupCountField).intValue = definition.Count;
            element.FindPropertyRelative(GroupIntervalField).floatValue = definition.SpawnInterval;
            element.FindPropertyRelative(GroupDelayField).floatValue = definition.StartDelay;

            SerializedProperty directions = element.FindPropertyRelative(GroupDirectionsField);
            directions.arraySize = definition.Directions.Count;
            for (int i = 0; i < definition.Directions.Count; i++)
            {
                directions.GetArrayElementAtIndex(i).enumValueIndex = (int)definition.Directions[i];
            }
        }
    }

    /// <summary><see cref="NightDataFiller.Fill"/> sonucu.</summary>
    public sealed class NightFillResult
    {
        public int AddedGroups { get; set; }
        public bool BossAssigned { get; set; }
        public bool ModifierAssigned { get; set; }

        /// <summary>Projede karşılığı olmadığı için atlanan içerik (TODO logu için), ör. "spitter × 6", "boss 'brute'".</summary>
        public List<string> Missing { get; } = new List<string>();

        /// <summary>Asset'e bir şey yazıldıysa true.</summary>
        public bool Changed => AddedGroups > 0 || BossAssigned || ModifierAssigned;
    }
}
