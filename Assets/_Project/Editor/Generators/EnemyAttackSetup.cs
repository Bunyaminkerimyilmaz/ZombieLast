using LastHaven.Enemies;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="EnemyData"/>'nın saldırı stratejisi (<c>[SerializeReference] _attack</c>): tanımdaki türün örneği, boşsa doldurma ve
    /// stratejinin boş prefab referanslarını (<see cref="EnemyAttackPrefabs"/>) bağlama. <see cref="EnemiesGenerator"/> kullanır.
    /// </summary>
    public static class EnemyAttackSetup
    {
        public const string AttackFieldName = "_attack";

        private const string ProjectilePath = AttackFieldName + "._projectile";
        private const string ExplosionVfxPath = AttackFieldName + "._explosionVfx";
        private const string WarningVfxPath = AttackFieldName + "._warningVfx";
        private const string ImpactVfxPath = AttackFieldName + "._impactVfx";

        /// <summary>Türün yeni örneği (ayarlar sınıfın varsayılanları; mermi/efekt prefab'ları <see cref="LinkAssets"/> ile bağlanır).</summary>
        public static IEnemyAttack Create(EnemyAttackKind kind)
        {
            return kind switch
            {
                EnemyAttackKind.Ranged => new RangedAttack(),
                EnemyAttackKind.SuicideExplode => new SuicideExplodeAttack(),
                EnemyAttackKind.GroundSlam => new GroundSlamAttack(),
                _ => new MeleeAttack(),
            };
        }

        /// <summary>Tanımdaki türü yazar (sadece ilk oluşturmada; <see cref="SerializedObject"/> çağıran uygular).</summary>
        public static void Write(SerializedObject serialized, EnemyAttackKind kind)
        {
            serialized.FindProperty(AttackFieldName).managedReferenceValue = Create(kind);
        }

        /// <summary>Saldırı boşsa tanımdaki türü yazar (12.1 öncesi asset'ler). Yazdıysa true; çağıran kaydeder.</summary>
        public static bool EnsureAssigned(EnemyData data, EnemyAttackKind kind)
        {
            var serialized = new SerializedObject(data);
            SerializedProperty attack = serialized.FindProperty(AttackFieldName);
            if (attack.managedReferenceValue != null)
            {
                return false;
            }

            attack.managedReferenceValue = Create(kind);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        /// <summary>
        /// Stratejinin boş prefab referanslarını doldurur: menzilli → asit mermisi, patlama → patlama efekti, yere vurma → uyarı dairesi
        /// ve darbe efekti (yarıçap stratejiden). Tasarımcı türü değiştirdiyse yeni türün alanına bakılır. Bağladıysa true; çağıran kaydeder.
        /// </summary>
        public static bool LinkAssets(EnemyData data)
        {
            switch (data.Attack)
            {
                case RangedAttack ranged when ranged.Projectile == null:
                    ArcProjectile projectile = EnemyAttackPrefabs.LoadOrCreateProjectile();
                    return projectile != null && EditorAssetUtility.AssignIfMissing(data, ProjectilePath, projectile);
                case SuicideExplodeAttack explode when explode.ExplosionVfx == null:
                    ExplosionFlash vfx = EnemyAttackPrefabs.LoadOrCreateExplosion(explode.Radius);
                    return vfx != null && EditorAssetUtility.AssignIfMissing(data, ExplosionVfxPath, vfx.gameObject);
                case GroundSlamAttack slam:
                    return LinkSlam(data, slam);
                default:
                    return false;
            }
        }

        private static bool LinkSlam(EnemyData data, GroundSlamAttack slam)
        {
            bool linked = false;
            if (slam.WarningVfx == null)
            {
                GroundWarning warning = EnemyAttackPrefabs.LoadOrCreateWarning();
                linked |= warning != null && EditorAssetUtility.AssignIfMissing(data, WarningVfxPath, warning);
            }

            if (slam.ImpactVfx == null)
            {
                ExplosionFlash impact = EnemyAttackPrefabs.LoadOrCreateSlamImpact(slam.Radius);
                linked |= impact != null && EditorAssetUtility.AssignIfMissing(data, ImpactVfxPath, impact.gameObject);
            }

            return linked;
        }
    }
}
