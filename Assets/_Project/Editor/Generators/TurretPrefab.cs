using LastHaven.Building;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Taret prefab'ının tarete özel parçaları (GDD §18: camgöbeği silindir gövde + küp kafa). <see cref="BuildablePrefabs"/>
    /// kökü, collider'ı, Health/Structure'ı ve onarımı kurar; bu sınıf:
    /// <list type="bullet">
    /// <item>"Visual/Base" (silindir) + "Visual/Head" (dönen pivot) → "HeadBody" (küp), "Barrel" (namlu), "Indicator" (güç göstergesi,
    /// <see cref="IndicatorMaterialPath"/>). Collider yok.</item>
    /// <item>Kökün çocuğu "Aim" (nişan pivotu, Visual dışında) → "Muzzle" (namlu ucu).</item>
    /// <item>Kökte <see cref="Turret"/> (+ <see cref="TurretConfig"/>) ve <see cref="TurretVisual"/> (kafa + gösterge).</item>
    /// </list>
    /// Var olan prefab'da sadece eksik parça/bileşen ve boş referanslar tamamlanır. Ölçüler GDD'de yok; namlu yüksekliği oyuncunun
    /// nişan yüksekliğiyle (1.2 m) aynı: yatay ışın her düşman tipinin gövdesine denk gelir.
    /// </summary>
    public static class TurretPrefab
    {
        public const string BaseName = "Base";
        public const string HeadName = "Head";
        public const string HeadBodyName = "HeadBody";
        public const string BarrelName = "Barrel";
        public const string IndicatorName = "Indicator";
        public const string AimName = "Aim";
        public const string MuzzleName = "Muzzle";

        public const string IndicatorMaterialPath = BuildGhostMaterial.Folder + "/M_TurretIndicator.mat";

        /// <summary>Kafa pivotunun ve namlunun yerden yüksekliği (m).</summary>
        public const float HeadHeight = 1.2f;

        private const string UnlitShaderName = "Universal Render Pipeline/Unlit";

        private static readonly Vector3 BaseScale = new Vector3(1.4f, 0.5f, 1.4f); // Unity silindiri 2 m: 1 m gövde.
        private static readonly Vector3 HeadBodyScale = new Vector3(0.8f, 0.5f, 0.9f);
        private static readonly Vector3 BarrelScale = new Vector3(0.18f, 0.18f, 0.7f);
        private static readonly Vector3 BarrelPosition = new Vector3(0f, 0f, 0.75f);
        private static readonly Vector3 IndicatorScale = new Vector3(0.22f, 0.08f, 0.22f);
        private static readonly Vector3 IndicatorPosition = new Vector3(0f, 0.29f, -0.2f);
        private static readonly Vector3 MuzzlePosition = new Vector3(0f, HeadHeight, 1.1f);
        private static readonly Color IndicatorColor = new Color(0.3f, 1f, 0.45f);
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        /// <summary>Tarete özel asset'ler. Testlerde null olabilir: parçalar kurulur, referans boş kalır.</summary>
        public readonly struct Assets
        {
            public Assets(TurretConfig config, Material indicator)
            {
                Config = config;
                Indicator = indicator;
            }

            public TurretConfig Config { get; }

            public Material Indicator { get; }
        }

        /// <summary>Gösterge materyali (URP/Unlit, yeşil; söndürme MaterialPropertyBlock ile). Asset varsa olduğu gibi döner.</summary>
        public static Material GetOrCreateIndicatorMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(IndicatorMaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(UnlitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[Last Haven] '{UnlitShaderName}' shader'ı bulunamadı; URP kurulu mu? Materyal oluşturulmadı: {IndicatorMaterialPath}");
                return null;
            }

            material = new Material(shader) { name = "M_TurretIndicator" };
            material.SetColor(BaseColorId, IndicatorColor);
            EditorAssetUtility.EnsureFolder(BuildGhostMaterial.Folder);
            AssetDatabase.CreateAsset(material, IndicatorMaterialPath);
            Debug.Log($"[Last Haven] Building: {IndicatorMaterialPath} oluşturuldu.");
            return material;
        }

        /// <summary>Görseli <paramref name="visual"/>'ın altına kurar (yeni prefab).</summary>
        public static void BuildVisual(Transform visual, Material body, Material indicator)
        {
            GameObject baseObject = CreatePart(PrimitiveType.Cylinder, BaseName, visual, body);
            baseObject.transform.localPosition = new Vector3(0f, BaseScale.y, 0f);
            baseObject.transform.localScale = BaseScale;

            var head = new GameObject(HeadName) { layer = GameLayers.Structure };
            head.transform.SetParent(visual, false);
            head.transform.localPosition = new Vector3(0f, HeadHeight, 0f);

            CreatePart(PrimitiveType.Cube, HeadBodyName, head.transform, body).transform.localScale = HeadBodyScale;

            GameObject barrel = CreatePart(PrimitiveType.Cube, BarrelName, head.transform, body);
            barrel.transform.localPosition = BarrelPosition;
            barrel.transform.localScale = BarrelScale;

            GameObject light = CreatePart(PrimitiveType.Cube, IndicatorName, head.transform, indicator);
            light.transform.localPosition = IndicatorPosition;
            light.transform.localScale = IndicatorScale;
        }

        /// <summary>
        /// Nişan pivotu + namlu, <see cref="Turret"/> ve <see cref="TurretVisual"/> yoksa ekler; boş referansları doldurur
        /// (config, pivot, namlu, görsel köprü, kafa, gösterge). Değişiklik sayısını döndürür.
        /// </summary>
        public static int EnsureParts(GameObject root, in Assets assets)
        {
            int changes = 0;
            Transform aim = root.transform.Find(AimName);
            if (aim == null)
            {
                aim = new GameObject(AimName) { layer = GameLayers.Structure }.transform;
                aim.SetParent(root.transform, false);
                changes++;
            }

            Transform muzzle = aim.Find(MuzzleName);
            if (muzzle == null)
            {
                muzzle = new GameObject(MuzzleName) { layer = GameLayers.Structure }.transform;
                muzzle.SetParent(aim, false);
                muzzle.localPosition = MuzzlePosition;
                changes++;
            }

            changes += VisualFeedbackSetup.EnsureComponent<TurretVisual>(root);
            changes += VisualFeedbackSetup.EnsureComponent<Turret>(root);
            var turretVisual = root.GetComponent<TurretVisual>();
            var turret = root.GetComponent<Turret>();

            Transform visual = VisualRoot.Find(root.transform);
            Transform head = visual != null ? visual.Find(HeadName) : null;
            Transform indicator = head != null ? head.Find(IndicatorName) : null;
            changes += Assign(turretVisual, "_head", head);
            changes += Assign(turretVisual, "_indicator", indicator != null ? indicator.GetComponent<Renderer>() : null);
            changes += Assign(turret, "_config", assets.Config);
            changes += Assign(turret, "_aimPivot", aim);
            changes += Assign(turret, "_muzzle", muzzle);
            changes += Assign(turret, "_visual", turretVisual);
            return changes;
        }

        private static int Assign(Object target, string fieldName, Object value)
        {
            return value != null && EditorAssetUtility.AssignIfMissing(target, fieldName, value) ? 1 : 0;
        }

        private static GameObject CreatePart(PrimitiveType type, string name, Transform parent, Material material)
        {
            return PrimitiveFactory.Create(type, name, parent, material, GameLayers.Structure, withCollider: false);
        }
    }
}
