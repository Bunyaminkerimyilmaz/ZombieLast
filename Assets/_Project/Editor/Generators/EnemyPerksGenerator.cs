using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Düşman perk değerleri (GDD §13): <see cref="EnemyPerkConfig"/> (<c>Data/Enemies/EnemyPerkConfig.asset</c>; Zincirleme Son %10,
    /// 3 m, 40 hasar). Varsa dokunulmaz. Prefab adımında boş patlama efektine Patlayan'ın <c>ExplosionFx</c>'i bağlanır.
    /// Sahnedeki <see cref="ChainExplosionPerk"/>'ı <see cref="EnemySpawnerSceneStep"/> bağlar.
    /// </summary>
    public sealed class EnemyPerksGenerator : IGenerationStep
    {
        public const string ConfigPath = EnemiesGenerator.Folder + "/EnemyPerkConfig.asset";

        private const string VfxFieldName = "_explosionVfx";

        public int Order => 750; // EnemiesGenerator'dan (700; patlama efekti) sonra.

        public string Name => "Enemy Perks";

        /// <summary>Config'i yükler/oluşturur. Sahne adımı buradan alır.</summary>
        public static EnemyPerkConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<EnemyPerkConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Enemy Perks: {ConfigPath} oluşturuldu.");
            }

            return config;
        }

        public void GenerateData()
        {
            LoadOrCreateConfig();
        }

        public void GeneratePrefabs()
        {
            EnemyPerkConfig config = LoadOrCreateConfig();
            if (config.ExplosionVfx != null)
            {
                return;
            }

            ExplosionFlash vfx = EnemyAttackPrefabs.LoadOrCreateExplosion(config.ChainExplosionRadius);
            if (vfx != null && EditorAssetUtility.AssignIfMissing(config, VfxFieldName, vfx.gameObject))
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Enemy Perks: {ConfigPath} patlama efekti bağlandı ({EnemyAttackPrefabs.ExplosionPath}).");
            }
        }
    }
}
