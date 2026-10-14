using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Tüm yer tutucu materyalleri eksikse oluşturur. Diğer adımlardan önce çalışır.</summary>
    public sealed class PlaceholderMaterialsGenerator : IGenerationStep
    {
        public int Order => 0;

        public string Name => "Placeholder Materials";

        public void GenerateData()
        {
            int created = PlaceholderMaterials.EnsureAll();
            Debug.Log(created > 0
                ? $"[Last Haven] {Name}: {created} materyal oluşturuldu ({PlaceholderMaterials.Folder})."
                : $"[Last Haven] {Name}: tüm materyaller zaten var.");
        }

        public void GeneratePrefabs()
        {
        }
    }
}
