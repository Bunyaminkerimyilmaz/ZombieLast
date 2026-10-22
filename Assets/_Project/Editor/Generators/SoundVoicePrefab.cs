using LastHaven.Audio;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Havuzlanan ses kaynağı prefab'ı (<c>Prefabs/Audio/SoundVoice.prefab</c>): AudioSource + <see cref="SoundVoice"/>. Varlık değil,
    /// görseli yok (Visual kuralı dışında). Varsa dokunulmaz; sadece eksik <see cref="SoundVoice"/> ve kaynak referansı tamamlanır.
    /// Azalma doğrusal: dinleyici oyuncunun üstünde, 3D sesler min mesafeye kadar tam, max mesafede sıfır.
    /// </summary>
    public static class SoundVoicePrefab
    {
        public const string Folder = "Assets/_Project/Prefabs/Audio";
        public const string Path = Folder + "/SoundVoice.prefab";

        private const string SourceField = "_source";

        public static SoundVoice LoadOrCreate()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (prefab == null)
            {
                GameObject root = Build();
                try
                {
                    EditorAssetUtility.EnsureFolder(Folder);
                    prefab = PrefabUtility.SaveAsPrefabAsset(root, Path);
                    Debug.Log($"[Last Haven] Audio: {Path} oluşturuldu.");
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }
            else
            {
                int changes = EditorAssetUtility.EditPrefab(Path, CompleteMissing);
                if (changes > 0)
                {
                    Debug.Log($"[Last Haven] Audio: {Path} eksikleri tamamlandı ({changes}).");
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
                }
            }

            return prefab.GetComponent<SoundVoice>();
        }

        private static GameObject Build()
        {
            var root = new GameObject("SoundVoice");
            var source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.dopplerLevel = 0f;
            CompleteMissing(root);
            return root;
        }

        private static int CompleteMissing(GameObject root)
        {
            int changes = 0;
            if (!root.TryGetComponent(out AudioSource source))
            {
                source = root.AddComponent<AudioSource>();
                source.playOnAwake = false;
                changes++;
            }

            if (!root.TryGetComponent(out SoundVoice voice))
            {
                voice = root.AddComponent<SoundVoice>();
                changes++;
            }

            if (EditorAssetUtility.AssignIfMissing(voice, SourceField, source))
            {
                changes++;
            }

            return changes;
        }
    }
}
