using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastHaven.Editor
{
    /// <summary>
    /// URP post-process profilleri (<see cref="VolumeProfile"/>): Vignette, Color Adjustments, Bloom. Sadece tanımdaki parametreler
    /// override edilir; diğerleri URP varsayılanında kalır. Var olan profile dokunulmaz.
    /// </summary>
    public static class PostProcessProfiles
    {
        /// <summary>Profili yükler; yoksa tanımla oluşturur (bileşenler asset'in alt nesneleri olarak kaydedilir).</summary>
        public static VolumeProfile LoadOrCreate(string path, in PostProcessDefinition definition, out bool created)
        {
            PostProcessDefinition copy = definition;
            return LoadOrCreate(path, profile => Populate(profile, copy), out created);
        }

        /// <summary>
        /// Profili yükler; yoksa boş oluşturup <paramref name="populate"/> ile bileşenlerini ekler ve bileşenleri asset'in alt nesneleri
        /// olarak kaydeder. Var olan profile dokunulmaz.
        /// </summary>
        public static VolumeProfile LoadOrCreate(string path, System.Action<VolumeProfile> populate, out bool created)
        {
            var profile = EditorAssetUtility.LoadOrCreate<VolumeProfile>(path, out created);
            if (!created)
            {
                return profile;
            }

            populate(profile);
            foreach (VolumeComponent component in profile.components)
            {
                // URP'nin profil editörü bileşenleri böyle saklar: asset içinde, Project penceresinde gizli.
                component.name = component.GetType().Name;
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        /// <summary>Boş profile üç bileşeni ekler (bellekte; asset işlemi yapmaz). Testler de kullanır.</summary>
        public static void Populate(VolumeProfile profile, in PostProcessDefinition definition)
        {
            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(definition.VignetteIntensity);
            vignette.smoothness.Override(definition.VignetteSmoothness);
            vignette.color.Override(definition.VignetteColor);

            var colorAdjustments = profile.Add<ColorAdjustments>();
            colorAdjustments.postExposure.Override(definition.PostExposure);
            colorAdjustments.contrast.Override(definition.Contrast);
            colorAdjustments.saturation.Override(definition.Saturation);
            colorAdjustments.colorFilter.Override(definition.ColorFilter);

            var bloom = profile.Add<Bloom>();
            bloom.threshold.Override(definition.BloomThreshold);
            bloom.intensity.Override(definition.BloomIntensity);
            bloom.scatter.Override(definition.BloomScatter);
        }
    }

    /// <summary>Bir post-process profilinin üretici değerleri.</summary>
    public readonly struct PostProcessDefinition
    {
        public PostProcessDefinition(
            float vignetteIntensity,
            float vignetteSmoothness,
            Color vignetteColor,
            float postExposure,
            float contrast,
            float saturation,
            Color colorFilter,
            float bloomThreshold,
            float bloomIntensity,
            float bloomScatter)
        {
            VignetteIntensity = vignetteIntensity;
            VignetteSmoothness = vignetteSmoothness;
            VignetteColor = vignetteColor;
            PostExposure = postExposure;
            Contrast = contrast;
            Saturation = saturation;
            ColorFilter = colorFilter;
            BloomThreshold = bloomThreshold;
            BloomIntensity = bloomIntensity;
            BloomScatter = bloomScatter;
        }

        public float VignetteIntensity { get; }

        public float VignetteSmoothness { get; }

        public Color VignetteColor { get; }

        /// <summary>EV.</summary>
        public float PostExposure { get; }

        /// <summary>−100…100.</summary>
        public float Contrast { get; }

        /// <summary>−100…100.</summary>
        public float Saturation { get; }

        public Color ColorFilter { get; }

        public float BloomThreshold { get; }

        public float BloomIntensity { get; }

        public float BloomScatter { get; }
    }
}
