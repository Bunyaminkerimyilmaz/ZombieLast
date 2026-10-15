using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// İnşa önizlemesinin materyali (<c>Materials/Building/M_BuildGhost.mat</c>): URP/Unlit, saydam (alfa karışımı),
    /// gölgesiz. Rengi (yeşil/kırmızı) runtime'da MaterialPropertyBlock ile <c>_BaseColor</c>'a yazılır. Asset varsa olduğu gibi döner.
    /// </summary>
    public static class BuildGhostMaterial
    {
        public const string Folder = "Assets/_Project/Materials/Building";
        public const string Path = Folder + "/M_BuildGhost.mat";

        private const string UnlitShaderName = "Universal Render Pipeline/Unlit";

        // URP BaseShaderGUI değerleri: _Surface 1 = Transparent; _Blend 0 = Alpha.
        private const float TransparentSurface = 1f;
        private const float AlphaBlend = 0f;

        private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
        private static readonly int BlendId = Shader.PropertyToID("_Blend");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color DefaultColor = new Color(1f, 1f, 1f, 0.45f);

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

            material = new Material(shader) { name = "M_BuildGhost" };
            material.SetFloat(SurfaceId, TransparentSurface);
            material.SetFloat(BlendId, AlphaBlend);
            material.SetColor(BaseColorId, DefaultColor);
            UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(material);

            EditorAssetUtility.EnsureFolder(Folder);
            AssetDatabase.CreateAsset(material, Path);
            Debug.Log($"[Last Haven] Building: {Path} oluşturuldu.");
            return material;
        }
    }
}
