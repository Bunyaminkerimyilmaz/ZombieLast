using LastHaven.Core;
using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Editor
{
    /// <summary>
    /// Düşman prefab'ları (<c>Prefabs/Enemies/&lt;Tip&gt;.prefab</c>). Kök: NavMeshAgent + CapsuleCollider + Health + Enemy,
    /// Enemy layer ve tag. "Visual": GDD §18 renk, şekil (kapsül; Patlayan küre) ve ölçekte gövde (collider yok). Collider ve ajan ölçüleri
    /// gövdeye göre ayarlanır; EnemyData.Scale 1 kalır (ölçek iki kez uygulanmasın). Vuruş hissi (4.3): AnimationDriver, beyaz HitFlash
    /// (GDD §7: 0.08 sn), EnemyHitReaction ve başın üstünde can barı (Visual dışında). Durum efektleri (9.3): EnemyStatusEffects.
    /// Menzilli tiplerde (12.1) Visual dışında <see cref="MuzzleName"/> çıkış noktası; Patlayan'da yürüme nabzı; boss'ta (12.2)
    /// <see cref="BossBehaviour"/> (<see cref="BossSetup"/>).
    /// </summary>
    public static class EnemyPrefabs
    {
        public const string Folder = "Assets/_Project/Prefabs/Enemies";
        public const string BodyName = "Body";
        public const string MuzzleName = "Muzzle";

        private const string DataFieldName = "_data";
        private const string MuzzleFieldName = "_muzzle";
        private const string WalkPulsePath = "_procedural._walkPulseScale";

        // GDD'de yok: ağız yüksekliği (gövde boyunun %80'i), gövdenin ön yüzünde.
        private const float MuzzleHeightFraction = 0.8f;

        // GDD'de yok. Varsayılan 120°/sn ve 8 m/sn² köşelerde geniş dönüşe ve savrulmaya yol açıyordu.
        private const float AgentAngularSpeed = 540f;
        private const float AgentAcceleration = 20f;

        // GDD §7: vuruşta 0.08 sn beyaz flaş.
        private const float HitFlashDuration = 0.08f;

        public static string GetPrefabPath(EnemyDefinition definition)
        {
            return $"{Folder}/{definition.AssetName}.prefab";
        }

        /// <summary>
        /// Prefab'ı yükler; yoksa oluşturur. Varsa sadece eksikler tamamlanır (boş EnemyData referansı, vuruş hissi bileşenleri,
        /// can barı, menzilli tipte boş Muzzle referansı); diğer ayarlara dokunulmaz.
        /// </summary>
        public static Enemy LoadOrCreate(EnemyDefinition definition, EnemyData data)
        {
            string path = GetPrefabPath(definition);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return Create(definition, data, path);
            }

            int changes = EditorAssetUtility.EditPrefab(
                path,
                root => AssignData(root, data, path)
                        + EnsureFeedback(root, definition, HealthBarMaterials.GetOrCreateBackground(), HealthBarMaterials.GetOrCreateFill())
                        + EnsureMuzzle(root, definition)
                        + BossSetup.Ensure(root, definition, definition.IsBoss ? BossSetup.LoadOrCreateConfig() : null));
            if (changes > 0)
            {
                Debug.Log($"[Last Haven] Enemies: {path} içinde {changes} eksik bileşen/referans tamamlandı.");
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            return prefab.GetComponent<Enemy>();
        }

        /// <summary>
        /// Prefab hiyerarşisini sahnede (kaydetmeden) kurar. Çağıran yok etmekle sorumludur. Testler de kullanır
        /// (materyaller null olabilir; asset oluşturulmaz).
        /// </summary>
        public static GameObject BuildRoot(
            EnemyDefinition definition, Material material, Material barBackground = null, Material barFill = null)
        {
            var root = new GameObject(definition.AssetName) { layer = GameLayers.Enemy, tag = GameTags.Enemy };
            float height = definition.BodyHeight;
            float radius = definition.BodyRadius;

            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = radius;
            agent.height = height;
            agent.speed = definition.MoveSpeed; // Spawn'da EnemyData'dan yeniden yazılır.
            agent.angularSpeed = AgentAngularSpeed;
            agent.acceleration = AgentAcceleration;
            agent.stoppingDistance = 0f; // Durmayı durum makinesi saldırı menziline göre yapar.

            var collider = root.AddComponent<CapsuleCollider>();
            collider.direction = 1; // Y ekseni
            collider.radius = radius;
            collider.height = height;
            collider.center = new Vector3(0f, height * 0.5f, 0f);

            root.AddComponent<Health>();
            root.AddComponent<Enemy>();
            BuildVisual(root.transform, definition, material);
            EnsureFeedback(root, definition, barBackground, barFill);
            EnsureMuzzle(root, definition);
            SetWalkPulse(root, definition.WalkPulseScale);
            BossSetup.Ensure(root, definition, null); // Config asset'i Create bağlar (testlerde asset oluşturulmaz).
            return root;
        }

        private static Enemy Create(EnemyDefinition definition, EnemyData data, string path)
        {
            Material material = PlaceholderMaterials.GetOrCreate(definition.Material);
            GameObject root = BuildRoot(
                definition, material, HealthBarMaterials.GetOrCreateBackground(), HealthBarMaterials.GetOrCreateFill());
            try
            {
                EditorAssetUtility.AssignIfMissing(root.GetComponent<Enemy>(), DataFieldName, data);
                if (definition.IsBoss)
                {
                    BossSetup.Ensure(root, definition, BossSetup.LoadOrCreateConfig());
                }

                EditorAssetUtility.EnsureFolder(Folder);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[Last Haven] Enemies: {path} oluşturuldu.");
                return prefab.GetComponent<Enemy>();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildVisual(Transform root, EnemyDefinition definition, Material material)
        {
            var visual = new GameObject(VisualRoot.Name) { layer = GameLayers.Enemy };
            visual.transform.SetParent(root, false);

            GameObject body = PrimitiveFactory.Create(
                definition.BodyShape, BodyName, visual.transform, material, GameLayers.Enemy, withCollider: false);
            body.transform.localPosition = new Vector3(0f, definition.BodyHeight * 0.5f, 0f);
            body.transform.localScale = definition.BodyScale;
        }

        /// <summary>
        /// Menzilli tipte <see cref="Enemy"/>'nin boş mermi çıkış noktasını bağlar; kökte <see cref="MuzzleName"/> yoksa (Visual dışında,
        /// ağız yüksekliğinde, gövdenin önünde) oluşturur. Değişiklik sayısını döndürür.
        /// </summary>
        private static int EnsureMuzzle(GameObject root, EnemyDefinition definition)
        {
            if (definition.Attack != EnemyAttackKind.Ranged || !root.TryGetComponent(out Enemy enemy))
            {
                return 0;
            }

            var serialized = new SerializedObject(enemy);
            SerializedProperty property = serialized.FindProperty(MuzzleFieldName);
            if (property.objectReferenceValue != null)
            {
                return 0;
            }

            int changes = 1;
            Transform muzzle = root.transform.Find(MuzzleName);
            if (muzzle == null)
            {
                muzzle = new GameObject(MuzzleName) { layer = GameLayers.Enemy }.transform;
                muzzle.SetParent(root.transform, false);
                muzzle.localPosition = new Vector3(0f, definition.BodyHeight * MuzzleHeightFraction, definition.BodyRadius);
                changes++;
            }

            property.objectReferenceValue = muzzle;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return changes;
        }

        /// <summary>AnimationDriver'ın yürüme nabzı oranını yazar (sadece prefab ilk kurulurken; 0 ise dokunmaz).</summary>
        private static void SetWalkPulse(GameObject root, float scale)
        {
            if (scale <= 0f || !root.TryGetComponent(out AnimationDriver driver))
            {
                return;
            }

            var serialized = new SerializedObject(driver);
            serialized.FindProperty(WalkPulsePath).floatValue = scale;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Vuruş hissi bileşenlerini (AnimationDriver, HitFlash, EnemyHitReaction), can barını ve durum efektlerini
        /// (<see cref="EnemyStatusEffects"/>, 9.3; yavaşlatma tonu HitFlash'la) eksikse ekler. Visual'dan sonra çağrılmalı.
        /// Değişiklik sayısını döndürür.
        /// </summary>
        private static int EnsureFeedback(GameObject root, EnemyDefinition definition, Material barBackground, Material barFill)
        {
            return VisualFeedbackSetup.EnsureAnimationDriver(root, definition.BodyRadius)
                   + VisualFeedbackSetup.EnsureHitFlash(root, Color.white, HitFlashDuration)
                   + VisualFeedbackSetup.EnsureComponent<EnemyHitReaction>(root)
                   + EnemyHealthBarBuilder.Ensure(root, definition.BodyHeight, barBackground, barFill)
                   + VisualFeedbackSetup.EnsureComponent<EnemyStatusEffects>(root);
        }

        private static int AssignData(GameObject root, EnemyData data, string path)
        {
            if (!root.TryGetComponent(out Enemy enemy))
            {
                Debug.LogError($"[Last Haven] Enemies: {path} kökünde Enemy bileşeni yok; EnemyData bağlanamadı.", data);
                return 0;
            }

            return EditorAssetUtility.AssignIfMissing(enemy, DataFieldName, data) ? 1 : 0;
        }
    }
}
