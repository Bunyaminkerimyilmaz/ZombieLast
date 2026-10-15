using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Dünya can barlarının materyalleri (<c>Materials/HealthBar/</c>): URP/Unlit, ışıktan ve gölgeden etkilenmez.
    /// Renkler GDD'de yok: koyu arka plan, düşmanda kırmızı dolgu, yapı onarımında yeşil dolgu. Asset varsa olduğu gibi döner.
    /// </summary>
    public static class HealthBarMaterials
    {
        public const string Folder = "Assets/_Project/Materials/HealthBar";
        public const string BackgroundPath = Folder + "/M_HealthBarBackground.mat";
        public const string FillPath = Folder + "/M_HealthBarFill.mat";
        public const string RepairFillPath = Folder + "/M_RepairBarFill.mat";

        private const string UnlitShaderName = "Universal Render Pipeline/Unlit";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private static readonly Color BackgroundColor = new Color(0.08f, 0.08f, 0.08f);
        private static readonly Color FillColor = new Color(0.86f, 0.18f, 0.16f);
        private static readonly Color RepairFillColor = new Color(0.3f, 0.85f, 0.35f);

        public static Material GetOrCreateBackground()
        {
            return GetOrCreate(BackgroundPath, BackgroundColor);
        }

        public static Material GetOrCreateFill()
        {
            return GetOrCreate(FillPath, FillColor);
        }

        /// <summary>Yapının onarım barının (8.3) dolgusu.</summary>
        public static Material GetOrCreateRepairFill()
        {
            return GetOrCreate(RepairFillPath, RepairFillColor);
        }

        /// <summary>Düşman can barının iki materyalini eksikse oluşturur.</summary>
        public static void EnsureAll()
        {
            GetOrCreateBackground();
            GetOrCreateFill();
        }

        private static Material GetOrCreate(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(UnlitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[Last Haven] '{UnlitShaderName}' shader'ı bulunamadı; URP kurulu mu? Materyal oluşturulmadı: {path}");
                return null;
            }

            material = new Material(shader) { name = path.Substring(path.LastIndexOf('/') + 1).Replace(".mat", string.Empty) };
            material.SetColor(BaseColorId, color);
            EditorAssetUtility.EnsureFolder(Folder);
            AssetDatabase.CreateAsset(material, path);
            Debug.Log($"[Last Haven] HealthBar: {path} oluşturuldu.");
            return material;
        }
    }
}
