using System;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="GameLayers"/>'taki layer'ları TagManager'a sabit indekslerle, <see cref="GameTags.Custom"/>'ı tag olarak yazar; sonra
    /// Player Settings'i <see cref="PlayerSettingsSetup"/> ile uygular (15.3).
    /// Tekrar çalıştırmak güvenlidir; sadece eksik veya farklı olanı değiştirir ve her değişikliği loglar.
    /// </summary>
    public static class ProjectSetup
    {
        private const string TagManagerPath = "ProjectSettings/TagManager.asset";
        private const string LogPrefix = "[Last Haven] Setup Project:";

        public static void Run()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(TagManagerPath);
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError($"{LogPrefix} {TagManagerPath} yüklenemedi.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            int layerChanges = SetupLayers(tagManager.FindProperty("layers"));
            int tagChanges = SetupTags(tagManager.FindProperty("tags"));

            if (layerChanges + tagChanges > 0)
            {
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"{LogPrefix} {layerChanges} layer ve {tagChanges} tag değişti " +
                      $"({GameLayers.All.Count} layer, {GameTags.Custom.Count} özel tag kontrol edildi).");

            // 15.3: ürün adı, sürüm, pencere modu ve sis modları Data/Build/PlayerBuildConfig.asset'ten.
            PlayerSettingsSetup.Run();
        }

        private static int SetupLayers(SerializedProperty layers)
        {
            int changes = 0;
            foreach ((int index, string name) in GameLayers.All)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(index);
                string existing = slot.stringValue;
                if (string.Equals(existing, name, StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(existing))
                {
                    Debug.Log($"{LogPrefix} Layer {index} = '{name}' eklendi.");
                }
                else
                {
                    Debug.LogWarning($"{LogPrefix} Layer {index} '{existing}' idi, '{name}' ile değiştirildi. " +
                                     "Bu layer'daki eski objeleri kontrol et.");
                }

                slot.stringValue = name;
                changes++;
                WarnIfDuplicate(layers, index, name);
            }

            return changes;
        }

        private static void WarnIfDuplicate(SerializedProperty layers, int expectedIndex, string name)
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (i != expectedIndex &&
                    string.Equals(layers.GetArrayElementAtIndex(i).stringValue, name, StringComparison.Ordinal))
                {
                    Debug.LogWarning($"{LogPrefix} '{name}' adı layer {i}'de de var. Kod sadece {expectedIndex}'i kullanır; " +
                                     $"layer {i}'yi elle temizle.");
                }
            }
        }

        private static int SetupTags(SerializedProperty tags)
        {
            int changes = 0;
            foreach (string tag in GameTags.Custom)
            {
                if (ContainsTag(tags, tag))
                {
                    continue;
                }

                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
                Debug.Log($"{LogPrefix} Tag '{tag}' eklendi.");
                changes++;
            }

            return changes;
        }

        private static bool ContainsTag(SerializedProperty tags, string tag)
        {
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (string.Equals(tags.GetArrayElementAtIndex(i).stringValue, tag, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
