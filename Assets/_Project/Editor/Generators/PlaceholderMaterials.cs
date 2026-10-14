using System;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// GDD §18 renkleriyle URP/Lit yer tutucu materyalleri. Diğer üreticiler materyali buradan ister.
    /// Asset varsa olduğu gibi döner (kullanıcının değişiklikleri korunur), yoksa oluşturulur.
    /// </summary>
    public sealed class PlaceholderMaterials : IMaterialProvider
    {
        public const string Folder = "Assets/_Project/Materials/Placeholder";

        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public Material Get(PlaceholderMaterialId id)
        {
            return GetOrCreate(id);
        }

        /// <summary>Materyali yükler; yoksa oluşturur.</summary>
        public static Material GetOrCreate(PlaceholderMaterialId id)
        {
            return GetOrCreate(id, out _);
        }

        /// <summary>Tüm yer tutucu materyalleri eksikse oluşturur ve oluşturulan sayısını döndürür.</summary>
        public static int EnsureAll()
        {
            int createdCount = 0;
            foreach (PlaceholderMaterialId id in Enum.GetValues(typeof(PlaceholderMaterialId)))
            {
                GetOrCreate(id, out bool created);
                if (created)
                {
                    createdCount++;
                }
            }

            return createdCount;
        }

        public static string GetAssetPath(PlaceholderMaterialId id)
        {
            return $"{Folder}/M_{id}.mat";
        }

        /// <summary>GDD §18 rengi (sRGB).</summary>
        public static Color GetColor(PlaceholderMaterialId id)
        {
            return id switch
            {
                PlaceholderMaterialId.Player => Hex("#2F6FE0"),            // Mavi
                PlaceholderMaterialId.Walker => Hex("#3FA34D"),            // Yeşil
                PlaceholderMaterialId.Runner => Hex("#A8D63A"),            // Sarı-yeşil
                PlaceholderMaterialId.Tank => Hex("#8B1A1A"),              // Koyu kırmızı
                PlaceholderMaterialId.Spitter => Hex("#8E44AD"),           // Mor
                PlaceholderMaterialId.Exploder => Hex("#F28C28"),          // Turuncu
                PlaceholderMaterialId.Brute => Hex("#3A0D0D"),             // Siyah-kırmızı
                PlaceholderMaterialId.Generator => Hex("#F5D42A"),         // Sarı
                PlaceholderMaterialId.BarricadeWood => Hex("#8B5A2B"),     // Kahve
                PlaceholderMaterialId.BarricadeMetal => Hex("#8C8C8C"),    // Gri
                PlaceholderMaterialId.BarricadeConcrete => Hex("#EDEDED"), // Beyaz
                PlaceholderMaterialId.Turret => Hex("#2EC4D6"),            // Camgöbeği
                PlaceholderMaterialId.SpikeTrap => Hex("#3A3A3A"),         // Koyu gri
                PlaceholderMaterialId.Coin => Hex("#FFC72C"),              // Altın
                PlaceholderMaterialId.LootChest => Hex("#A0612A"),         // Turuncu-kahve
                PlaceholderMaterialId.Ground => Hex("#363636"),            // Koyu gri
                PlaceholderMaterialId.BaseArea => Hex("#555555"),          // Biraz açık gri
                PlaceholderMaterialId.Obstacle => Hex("#6E6257"),          // Kahve-gri
                PlaceholderMaterialId.TestDummy => Hex("#9A9A9A"),         // Gri (GDD'de yok; silah testi kuklası)
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null),
            };
        }

        private static Material GetOrCreate(PlaceholderMaterialId id, out bool created)
        {
            string path = GetAssetPath(id);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                created = false;
                return material;
            }

            Shader shader = Shader.Find(LitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[Last Haven] '{LitShaderName}' shader'ı bulunamadı; URP kurulu mu? Materyal oluşturulmadı: {id}");
                created = false;
                return null;
            }

            EditorAssetUtility.EnsureFolder(Folder);
            material = new Material(shader) { name = $"M_{id}" };
            material.SetColor(BaseColorId, GetColor(id));
            AssetDatabase.CreateAsset(material, path);
            created = true;
            return material;
        }

        private static Color Hex(string html)
        {
            ColorUtility.TryParseHtmlString(html, out Color color);
            return color;
        }
    }
}
