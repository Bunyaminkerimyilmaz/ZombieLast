using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>Üreticilerin ortak asset yardımcıları.</summary>
    public static class EditorAssetUtility
    {
        /// <summary>"Assets/a/b/c" klasörünü, eksik ara klasörlerle birlikte oluşturur.</summary>
        public static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int slash = folderPath.LastIndexOf('/');
            string parent = folderPath.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderPath.Substring(slash + 1));
        }

        /// <summary>
        /// Asset'i yükler; yoksa varsayılan değerlerle oluşturur. Var olan asset'e dokunmaz.
        /// </summary>
        /// <param name="created">Bu çağrıda oluşturulduysa true.</param>
        public static T LoadOrCreate<T>(string assetPath, out bool created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                created = false;
                return asset;
            }

            EnsureFolder(assetPath.Substring(0, assetPath.LastIndexOf('/')));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            created = true;
            return asset;
        }

        /// <summary>
        /// Var olan prefab'ı düzenleme için açar ve <paramref name="edit"/>'i çalıştırır; dönen değişiklik sayısı 0'dan büyükse kaydeder.
        /// Eksik bileşen veya referans eklemek için; düzenleyici kullanıcının ayarlarını ezmemeli.
        /// </summary>
        /// <returns>Değişiklik sayısı.</returns>
        public static int EditPrefab(string prefabPath, Func<GameObject, int> edit)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                int changes = edit(contents);
                if (changes > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                }

                return changes;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        /// <summary>
        /// Private serialized referans alanı boşsa <paramref name="value"/>'yu yazar; doluysa dokunmaz.
        /// Alan yoksa hata loglar. Yazdıysa true döner.
        /// </summary>
        public static bool AssignIfMissing(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
            {
                Debug.LogError($"[Last Haven] {target.GetType().Name}: '{propertyName}' referans alanı bulunamadı.", target);
                return false;
            }

            if (property.objectReferenceValue != null)
            {
                return false;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}
