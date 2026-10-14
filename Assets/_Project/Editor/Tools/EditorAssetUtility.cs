using UnityEditor;
using UnityEngine;

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
    }
}
