using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Efekt materyalleri (<c>Materials/Fx/</c>): URP'nin varsayılan parçacık materyalinin (Particles/Unlit, yumuşak daire dokusu)
    /// kopyaları. Additive: mermi izi ve kıvılcım (parlar). Alpha: kan (koyu kalır). Asset varsa olduğu gibi döner.
    /// </summary>
    public static class FxMaterials
    {
        public const string Folder = "Assets/_Project/Materials/Fx";
        public const string AdditivePath = Folder + "/M_FxAdditive.mat";
        public const string AlphaPath = Folder + "/M_FxAlpha.mat";

        private const string TemplatePath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat";
        private const string FallbackShaderName = "Universal Render Pipeline/Particles/Unlit";

        // URP BaseShaderGUI değerleri: _Surface 1 = Transparent; _Blend 0 = Alpha, 2 = Additive.
        private const float TransparentSurface = 1f;
        private const float AlphaBlend = 0f;
        private const float AdditiveBlend = 2f;

        private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
        private static readonly int BlendId = Shader.PropertyToID("_Blend");

        public static Material GetOrCreateAdditive()
        {
            return GetOrCreate(AdditivePath, AdditiveBlend);
        }

        public static Material GetOrCreateAlpha()
        {
            return GetOrCreate(AlphaPath, AlphaBlend);
        }

        /// <summary>İki materyali de eksikse oluşturur.</summary>
        public static void EnsureAll()
        {
            GetOrCreateAdditive();
            GetOrCreateAlpha();
        }

        private static Material GetOrCreate(string path, float blend)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var template = AssetDatabase.LoadAssetAtPath<Material>(TemplatePath);
            if (template != null)
            {
                material = new Material(template);
            }
            else
            {
                Shader shader = Shader.Find(FallbackShaderName);
                if (shader == null)
                {
                    Debug.LogError($"[Last Haven] '{FallbackShaderName}' shader'ı bulunamadı; URP kurulu mu? Materyal oluşturulmadı: {path}");
                    return null;
                }

                material = new Material(shader);
            }

            material.name = path.Substring(path.LastIndexOf('/') + 1).Replace(".mat", string.Empty);
            material.SetFloat(SurfaceId, TransparentSurface);
            material.SetFloat(BlendId, blend);
            UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(material);

            EditorAssetUtility.EnsureFolder(Folder);
            AssetDatabase.CreateAsset(material, path);
            Debug.Log($"[Last Haven] Feedback: {path} oluşturuldu.");
            return material;
        }
    }
}
