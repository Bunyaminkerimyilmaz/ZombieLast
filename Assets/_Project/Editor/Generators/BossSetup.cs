using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Boss ekleri (GDD §7 Brute): ortak <see cref="BossConfig"/> (<c>Data/Enemies/BossConfig.asset</c>; varsayılanlar sınıfta, GDD'den)
    /// ve boss prefab'larının kökündeki <see cref="BossBehaviour"/>. Varsa dokunulmaz; boş config referansı doldurulur.
    /// </summary>
    public static class BossSetup
    {
        public const string ConfigPath = EnemiesGenerator.Folder + "/BossConfig.asset";

        private const string ConfigFieldName = "_config";

        public static BossConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<BossConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Enemies: {ConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>Boss tanımında kökte <see cref="BossBehaviour"/> yoksa ekler, boş config'ini bağlar. Değişiklik sayısını döndürür.</summary>
        /// <param name="config">Null olabilir (testler; referans boş kalır).</param>
        public static int Ensure(GameObject root, EnemyDefinition definition, BossConfig config)
        {
            if (!definition.IsBoss)
            {
                return 0;
            }

            int changes = VisualFeedbackSetup.EnsureComponent<BossBehaviour>(root);
            if (config != null && EditorAssetUtility.AssignIfMissing(root.GetComponent<BossBehaviour>(), ConfigFieldName, config))
            {
                changes++;
            }

            return changes;
        }
    }
}
