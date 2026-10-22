using System;
using LastHaven.Audio;
using LastHaven.Core;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="AudioEventMap"/>'in boş yerlerini <see cref="AudioDefinitions"/>'taki seslerle doldurur ("eksikse ekle"): boş tekil alan
    /// yazılır, listede olmayan silah/evre/müzik satırı sona eklenir, satırın boş sesi doldurulur. Dolu alan ve satıra dokunulmaz
    /// (kullanıcının bağladığı ses korunur). Değişiklikler <see cref="SerializedObject"/>'te birikir; çağıran uygular.
    /// </summary>
    public static class AudioEventMapFiller
    {
        private const string WeaponFiredField = "_weaponFired";
        private const string PhaseChangedField = "_phaseChanged";
        private const string MusicField = "_music";
        private const string SoundField = "_sound";
        private const string WeaponIdField = "_weaponId";
        private const string PhaseField = "_phase";
        private const string CueField = "_cue";
        private const string LoopField = "_loop";

        /// <summary>Tanımın sesini haritadaki yerine yazar (boşsa). Yazdıysa true.</summary>
        public static bool Fill(SerializedObject map, AudioDefinitions.Definition definition, SoundData sound)
        {
            switch (definition.Slot)
            {
                case AudioDefinitions.Slot.WeaponFired:
                    return FillRow(map.FindProperty(WeaponFiredField), sound, row => row.FindPropertyRelative(WeaponIdField).stringValue,
                        definition.WeaponId, row => row.FindPropertyRelative(WeaponIdField).stringValue = definition.WeaponId);
                case AudioDefinitions.Slot.PhaseChanged:
                    return FillRow(map.FindProperty(PhaseChangedField), sound, row => Key(row.FindPropertyRelative(PhaseField).intValue),
                        Key((int)definition.Phase), row => row.FindPropertyRelative(PhaseField).intValue = (int)definition.Phase);
                case AudioDefinitions.Slot.Music:
                    return FillRow(map.FindProperty(MusicField), sound, row => Key(row.FindPropertyRelative(CueField).intValue),
                        Key((int)definition.Cue), row =>
                        {
                            row.FindPropertyRelative(CueField).intValue = (int)definition.Cue;
                            row.FindPropertyRelative(LoopField).boolValue = definition.Loop;
                        });
                default:
                    return FillField(map.FindProperty(GetFieldName(definition.Slot)), sound);
            }
        }

        /// <summary>Tekil yerlerin serileştirilmiş alan adı.</summary>
        public static string GetFieldName(AudioDefinitions.Slot slot)
        {
            return slot switch
            {
                AudioDefinitions.Slot.WeaponFiredFallback => "_weaponFiredFallback",
                AudioDefinitions.Slot.ShotHitFlesh => "_shotHitFlesh",
                AudioDefinitions.Slot.ShotHitSurface => "_shotHitSurface",
                AudioDefinitions.Slot.EnemyKilled => "_enemyKilled",
                AudioDefinitions.Slot.BossSpawned => "_bossSpawned",
                AudioDefinitions.Slot.CoinCollected => "_coinCollected",
                AudioDefinitions.Slot.PurchaseCompleted => "_purchaseCompleted",
                AudioDefinitions.Slot.StructurePlaced => "_structurePlaced",
                AudioDefinitions.Slot.StructureDestroyed => "_structureDestroyed",
                AudioDefinitions.Slot.PowerLost => "_powerLost",
                AudioDefinitions.Slot.DayEndingSoon => "_dayEndingSoon",
                AudioDefinitions.Slot.LevelUp => "_levelUp",
                AudioDefinitions.Slot.UiClick => "_uiClick",
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Liste yeri; tekil alan adı yok."),
            };
        }

        private static string Key(int value)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static bool FillField(SerializedProperty field, SoundData sound)
        {
            if (field.objectReferenceValue != null)
            {
                return false;
            }

            field.objectReferenceValue = sound;
            return true;
        }

        // Anahtarı eşleşen satır yoksa sona ekler; varsa sadece boş sesini doldurur.
        private static bool FillRow(
            SerializedProperty list,
            SoundData sound,
            Func<SerializedProperty, string> readKey,
            string key,
            Action<SerializedProperty> writeKey)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty row = list.GetArrayElementAtIndex(i);
                if (string.Equals(readKey(row), key, StringComparison.Ordinal))
                {
                    return FillField(row.FindPropertyRelative(SoundField), sound);
                }
            }

            list.arraySize++;
            SerializedProperty added = list.GetArrayElementAtIndex(list.arraySize - 1);
            writeKey(added);
            added.FindPropertyRelative(SoundField).objectReferenceValue = sound;
            return true;
        }
    }
}
