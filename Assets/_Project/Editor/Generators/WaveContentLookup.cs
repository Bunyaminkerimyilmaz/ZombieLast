using System;
using System.Collections.Generic;
using LastHaven.Enemies;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Gece üreticisinin içerik kaynağı: id → <see cref="EnemyData"/> / <see cref="NightModifierData"/> (ordinal).
    /// Projedeki asset'lerden kurulur; böylece Gün 12'de eklenen tipler ve olaylar bu dosyaya dokunmadan bulunur.
    /// Boş id'li asset atlanır; aynı id iki kez varsa ilki geçerlidir (uyarıyla).
    /// </summary>
    public sealed class WaveContentLookup
    {
        private readonly Dictionary<string, EnemyData> _enemies = new Dictionary<string, EnemyData>(StringComparer.Ordinal);
        private readonly Dictionary<string, NightModifierData> _modifiers = new Dictionary<string, NightModifierData>(StringComparer.Ordinal);

        public WaveContentLookup(IEnumerable<EnemyData> enemies, IEnumerable<NightModifierData> modifiers)
        {
            if (enemies != null)
            {
                foreach (EnemyData enemy in enemies)
                {
                    Add(_enemies, enemy, enemy != null ? enemy.Id : null);
                }
            }

            if (modifiers != null)
            {
                foreach (NightModifierData modifier in modifiers)
                {
                    Add(_modifiers, modifier, modifier != null ? modifier.Id : null);
                }
            }
        }

        /// <summary><paramref name="folders"/> altındaki tüm EnemyData ve NightModifierData asset'lerinden kurar.</summary>
        public static WaveContentLookup FromProject(params string[] folders)
        {
            return new WaveContentLookup(LoadAll<EnemyData>(folders), LoadAll<NightModifierData>(folders));
        }

        public bool TryGetEnemy(string id, out EnemyData enemy)
        {
            enemy = null;
            return !string.IsNullOrEmpty(id) && _enemies.TryGetValue(id, out enemy);
        }

        public bool TryGetModifier(string id, out NightModifierData modifier)
        {
            modifier = null;
            return !string.IsNullOrEmpty(id) && _modifiers.TryGetValue(id, out modifier);
        }

        private static void Add<T>(Dictionary<string, T> map, T asset, string id) where T : ScriptableObject
        {
            if (asset == null || string.IsNullOrEmpty(id))
            {
                return;
            }

            if (map.TryGetValue(id, out T existing))
            {
                Debug.LogWarning($"[Last Haven] Waves: '{id}' id'si hem '{existing.name}' hem '{asset.name}' asset'inde; '{existing.name}' kullanılıyor.", asset);
                return;
            }

            map.Add(id, asset);
        }

        private static List<T> LoadAll<T>(string[] folders) where T : ScriptableObject
        {
            var assets = new List<T>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, folders))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            // FindAssets sırası garanti değil; yol sırası her makinede aynı sonucu verir.
            assets.Sort((a, b) => string.CompareOrdinal(AssetDatabase.GetAssetPath(a), AssetDatabase.GetAssetPath(b)));
            return assets;
        }
    }
}
