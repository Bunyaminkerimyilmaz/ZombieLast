using System;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// Düşman saldırılarının havuzlanan yardımcıları (<c>Prefabs/Enemies/Attacks/</c>): Tükürücü'nün asit mermisi
    /// (<see cref="ArcProjectile"/>, EnemyProjectile layer, collider'sız; Visual altında asit yeşili küre), Patlayan'ın patlama efekti
    /// (<see cref="ExplosionFlash"/>: Visual altında genişleyip solan saydam turuncu küre + nokta ışık, <see cref="PooledLifetime"/>),
    /// Brute'un yerdeki uyarı dairesi (<see cref="GroundWarning"/>: iki ince disk) ve toz renkli darbe efekti. Efekt materyali
    /// <c>Materials/Fx/M_FxExplosion.mat</c> (URP/Unlit, saydam; renk property block'tan). Varsa dokunulmaz.
    /// </summary>
    public static class EnemyAttackPrefabs
    {
        public const string Folder = EnemyPrefabs.Folder + "/Attacks";
        public const string ProjectilePath = Folder + "/AcidProjectile.prefab";
        public const string ExplosionPath = Folder + "/ExplosionFx.prefab";
        public const string WarningPath = Folder + "/SlamWarning.prefab";
        public const string SlamImpactPath = Folder + "/SlamFx.prefab";
        public const string ExplosionMaterialPath = FxMaterials.Folder + "/M_FxExplosion.mat";
        public const string ShellName = "Shell";
        public const string WarningAreaName = "Area";
        public const string WarningFillName = "Fill";

        // GDD'de yok: okunur ama oyuncu mermisinden (hitscan) ayrı görünen küçük küre.
        private const float ProjectileDiameter = 0.35f;

        // GDD'de yok: kısa, okunur bir parlama.
        private const float ExplosionLifetime = 0.45f;
        private const float ExplosionLightRangePerRadius = 2.5f;
        private const float ExplosionLightIntensity = 8f;
        private static readonly Color ExplosionLightColor = new Color(1f, 0.6f, 0.25f);
        private static readonly Color ExplosionColor = new Color(1f, 0.55f, 0.1f, 0.6f);

        // Yere vurma: toz rengi, sönük ışık. Uyarı diskleri zeminle çakışmasın diye biraz yukarıda (iç disk alanın üstünde).
        private static readonly Color SlamColor = new Color(0.55f, 0.42f, 0.3f, 0.55f);
        private const float SlamLightIntensity = 3f;
        private const float WarningAreaHeight = 0.03f;
        private const float WarningFillHeight = 0.045f;

        private const string UnlitShaderName = "Universal Render Pipeline/Unlit";
        private const float TransparentSurface = 1f; // URP BaseShaderGUI: _Surface 1 = Transparent
        private const float AlphaBlend = 0f;         // _Blend 0 = Alpha
        private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
        private static readonly int BlendId = Shader.PropertyToID("_Blend");

        public static ArcProjectile LoadOrCreateProjectile()
        {
            return LoadOrCreate<ArcProjectile>(ProjectilePath, () => BuildProjectile(PlaceholderMaterials.GetOrCreate(PlaceholderMaterialId.AcidProjectile)));
        }

        /// <param name="radius">Patlama yarıçapı (m); efektin küresi bu yarıçapa genişler (sadece ilk oluşturmada yazılır).</param>
        public static ExplosionFlash LoadOrCreateExplosion(float radius)
        {
            return LoadOrCreate<ExplosionFlash>(ExplosionPath, () => BuildExplosion(radius, GetOrCreateExplosionMaterial()));
        }

        /// <param name="radius">Darbe yarıçapı (m; sadece ilk oluşturmada yazılır).</param>
        public static ExplosionFlash LoadOrCreateSlamImpact(float radius)
        {
            return LoadOrCreate<ExplosionFlash>(
                SlamImpactPath, () => BuildExplosion("SlamFx", radius, GetOrCreateExplosionMaterial(), SlamColor, SlamLightIntensity));
        }

        public static GroundWarning LoadOrCreateWarning()
        {
            return LoadOrCreate<GroundWarning>(WarningPath, () => BuildWarning(GetOrCreateExplosionMaterial()));
        }

        /// <summary>Uyarı dairesini sahnede (kaydetmeden) kurar; çağıran yok eder. Testler de kullanır (materyal null olabilir).</summary>
        public static GameObject BuildWarning(Material material)
        {
            var root = new GameObject("SlamWarning");
            Transform visual = CreateVisual(root);
            Renderer area = CreateDisc(WarningAreaName, visual, material, WarningAreaHeight);
            Renderer fill = CreateDisc(WarningFillName, visual, material, WarningFillHeight);

            var warning = root.AddComponent<GroundWarning>();
            var serialized = new SerializedObject(warning);
            serialized.FindProperty("_area").objectReferenceValue = area;
            serialized.FindProperty("_fill").objectReferenceValue = fill;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static Renderer CreateDisc(string name, Transform visual, Material material, float height)
        {
            GameObject disc = PrimitiveFactory.Create(PrimitiveType.Cylinder, name, visual, material, withCollider: false);
            disc.transform.localPosition = Vector3.up * height;
            var renderer = disc.GetComponent<Renderer>();
            FeedbackPrefabs.DisableShadowsAndProbes(renderer);
            return renderer;
        }

        /// <summary>Mermi hiyerarşisini sahnede (kaydetmeden) kurar; çağıran yok eder. Testler de kullanır (materyal null olabilir).</summary>
        public static GameObject BuildProjectile(Material material)
        {
            var root = new GameObject("AcidProjectile") { layer = GameLayers.EnemyProjectile };
            root.AddComponent<ArcProjectile>();

            Transform visual = CreateVisual(root);
            GameObject body = PrimitiveFactory.Create(
                PrimitiveType.Sphere, EnemyPrefabs.BodyName, visual, material, GameLayers.EnemyProjectile, withCollider: false);
            body.transform.localScale = Vector3.one * ProjectileDiameter;
            FeedbackPrefabs.DisableShadowsAndProbes(body.GetComponent<Renderer>());
            return root;
        }

        /// <summary>Patlama efektini sahnede (kaydetmeden) kurar; çağıran yok eder. Testler de kullanır (materyal null olabilir).</summary>
        public static GameObject BuildExplosion(float radius, Material material)
        {
            return BuildExplosion("ExplosionFx", radius, material, ExplosionColor, ExplosionLightIntensity);
        }

        private static GameObject BuildExplosion(string name, float radius, Material material, Color color, float lightIntensity)
        {
            var root = new GameObject(name);
            FeedbackPrefabs.SetLifetime(root.AddComponent<PooledLifetime>(), ExplosionLifetime);

            var light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = radius * ExplosionLightRangePerRadius;
            light.color = ExplosionLightColor;
            light.intensity = 0f; // Efekt yazar.
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Realtime;

            Transform visual = CreateVisual(root);
            GameObject shell = PrimitiveFactory.Create(PrimitiveType.Sphere, ShellName, visual, material, withCollider: false);
            FeedbackPrefabs.DisableShadowsAndProbes(shell.GetComponent<Renderer>());

            var flash = root.AddComponent<ExplosionFlash>();
            var serialized = new SerializedObject(flash);
            serialized.FindProperty("_shell").objectReferenceValue = shell.GetComponent<Renderer>();
            serialized.FindProperty("_light").objectReferenceValue = light;
            serialized.FindProperty("_radius").floatValue = radius;
            serialized.FindProperty("_color").colorValue = color;
            serialized.FindProperty("_lightIntensity").floatValue = lightIntensity;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static Transform CreateVisual(GameObject root)
        {
            var visual = new GameObject(VisualRoot.Name) { layer = root.layer };
            visual.transform.SetParent(root.transform, false);
            return visual.transform;
        }

        private static Material GetOrCreateExplosionMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(ExplosionMaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(UnlitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[Last Haven] '{UnlitShaderName}' shader'ı bulunamadı; URP kurulu mu? Materyal oluşturulmadı: {ExplosionMaterialPath}");
                return null;
            }

            material = new Material(shader) { name = "M_FxExplosion" };
            material.SetFloat(SurfaceId, TransparentSurface);
            material.SetFloat(BlendId, AlphaBlend);
            UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(material);

            EditorAssetUtility.EnsureFolder(FxMaterials.Folder);
            AssetDatabase.CreateAsset(material, ExplosionMaterialPath);
            Debug.Log($"[Last Haven] Enemies: {ExplosionMaterialPath} oluşturuldu.");
            return material;
        }

        private static T LoadOrCreate<T>(string path, Func<GameObject> build) where T : Component
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing.GetComponent<T>();
            }

            GameObject root = build();
            try
            {
                EditorAssetUtility.EnsureFolder(Folder);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[Last Haven] Enemies: {path} oluşturuldu.");
                return prefab.GetComponent<T>();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
