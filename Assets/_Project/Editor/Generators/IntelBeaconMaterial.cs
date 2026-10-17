using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// Spawn bölgelerinin istihbarat ışık sütunu (11.2, GDD §9): materyal (<c>Materials/Waves/M_IntelBeacon.mat</c>; URP/Unlit,
    /// saydam kırmızı, ışıktan etkilenmez) ve sütunun kendisi (gölgesiz, collider'sız silindir). Asset varsa olduğu gibi döner.
    /// </summary>
    public static class IntelBeaconMaterial
    {
        public const string Folder = "Assets/_Project/Materials/Waves";
        public const string Path = Folder + "/M_IntelBeacon.mat";
        public const string BeaconName = "IntelBeacon";

        /// <summary>Sütunun yüksekliği ve çapı (m): kenardaki bölge ekran dışındayken de tepesi görünsün.</summary>
        public const float BeaconHeight = 30f;
        public const float BeaconDiameter = 2f;

        private const string UnlitShaderName = "Universal Render Pipeline/Unlit";

        // URP BaseShaderGUI değerleri: _Surface 1 = Transparent; _Blend 0 = Alpha.
        private const float TransparentSurface = 1f;
        private const float AlphaBlend = 0f;

        // Unity silindiri 2 m yüksek, 1 m çaplı.
        private const float CylinderHeight = 2f;

        private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
        private static readonly int BlendId = Shader.PropertyToID("_Blend");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color BeaconColor = new Color(1f, 0.12f, 0.08f, 0.38f);

        public static Material GetOrCreate()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(UnlitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[Last Haven] '{UnlitShaderName}' shader'ı bulunamadı; URP kurulu mu? Materyal oluşturulmadı: {Path}");
                return null;
            }

            material = new Material(shader) { name = "M_IntelBeacon" };
            material.SetFloat(SurfaceId, TransparentSurface);
            material.SetFloat(BlendId, AlphaBlend);
            material.SetColor(BaseColorId, BeaconColor);
            UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(material);

            EditorAssetUtility.EnsureFolder(Folder);
            AssetDatabase.CreateAsset(material, Path);
            Debug.Log($"[Last Haven] Waves: {Path} oluşturuldu.");
            return material;
        }

        /// <summary>
        /// <paramref name="parent"/>'ın altına (tabanı yerde) kapalı ışık sütunu koyar: collider'sız, gölge düşürmez/almaz.
        /// </summary>
        public static GameObject CreateBeacon(Transform parent)
        {
            GameObject beacon = PrimitiveFactory.Create(PrimitiveType.Cylinder, BeaconName, parent, GetOrCreate(), withCollider: false);
            beacon.transform.localPosition = Vector3.up * (BeaconHeight * 0.5f);
            beacon.transform.localScale = new Vector3(BeaconDiameter, BeaconHeight / CylinderHeight, BeaconDiameter);

            var renderer = beacon.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            beacon.SetActive(false);
            return beacon;
        }
    }
}
