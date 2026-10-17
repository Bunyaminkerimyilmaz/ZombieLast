using LastHaven.GameFlow;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// Gün/gece ışığı (6.2): <c>Data/Lighting/</c> altında Day/Night <see cref="LightingProfile"/> ve post-process profilleri
    /// (<see cref="PostProcessProfiles"/>; <see cref="LightingDefinitions"/> değerleriyle). Varsa dokunulmaz; ışık profilinin
    /// post-process referansı boşsa varsayılan profil (yoksa oluşturularak) bağlanır.
    /// </summary>
    public sealed class LightingGenerator : IGenerationStep
    {
        public const string Folder = "Assets/_Project/Data/Lighting";
        public const string DayProfilePath = Folder + "/DayLighting.asset";
        public const string NightProfilePath = Folder + "/NightLighting.asset";
        public const string DayPostProcessPath = Folder + "/DayPostProcess.asset";
        public const string NightPostProcessPath = Folder + "/NightPostProcess.asset";
        public const string NightEventLightingPath = Folder + "/NightEventLighting.asset";

        private const string VolumeProfileFieldName = "_volumeProfile";

        public int Order => 1100;

        public string Name => "Lighting";

        /// <summary>Sahne adımı buradan alır; Generate Default Data çalışmamış olsa da asset'ler oluşur.</summary>
        public static LightingProfile LoadOrCreateDay()
        {
            return LoadOrCreate(DayProfilePath, LightingDefinitions.Day, DayPostProcessPath, LightingDefinitions.DayPostProcess);
        }

        public static LightingProfile LoadOrCreateNight()
        {
            return LoadOrCreate(NightProfilePath, LightingDefinitions.Night, NightPostProcessPath, LightingDefinitions.NightPostProcess);
        }

        /// <summary>Gece olayının ışık geçişi ve şimşek ayarları (varsayılanlar sınıfta). Varsa dokunulmaz.</summary>
        public static NightEventLighting LoadOrCreateNightEvents()
        {
            var asset = EditorAssetUtility.LoadOrCreate<NightEventLighting>(NightEventLightingPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Lighting: {NightEventLightingPath} oluşturuldu.");
            }

            return asset;
        }

        public void GenerateData()
        {
            LoadOrCreateDay();
            LoadOrCreateNight();
            LoadOrCreateNightEvents();
        }

        public void GeneratePrefabs()
        {
        }

        private static LightingProfile LoadOrCreate(
            string path, in LightingDefinition definition, string postProcessPath, in PostProcessDefinition postProcess)
        {
            var profile = EditorAssetUtility.LoadOrCreate<LightingProfile>(path, out bool created);
            if (created)
            {
                definition.WriteTo(profile);
                Debug.Log($"[Last Haven] Lighting: {path} oluşturuldu.");
            }

            if (profile.VolumeProfile != null)
            {
                return profile;
            }

            VolumeProfile volumeProfile = PostProcessProfiles.LoadOrCreate(postProcessPath, postProcess, out bool volumeCreated);
            if (volumeCreated)
            {
                Debug.Log($"[Last Haven] Lighting: {postProcessPath} oluşturuldu (Vignette, Color Adjustments, Bloom).");
            }

            EditorAssetUtility.AssignIfMissing(profile, VolumeProfileFieldName, volumeProfile);
            return profile;
        }
    }
}
