using LastHaven.GameFlow;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Gameplay]/Lighting: gün/gece ışığı (GDD §18). Kökte <see cref="DayNightLighting"/>; altında Sun (directional light),
    /// DayVolume ve NightVolume (Global Volume, post-process profilleri <see cref="LightingProfile"/>'lerden). Sahne gündüz ışığıyla
    /// kaydedilir (güneş, düz ambiyans, sis; DayVolume weight 1, NightVolume 0); oyunda geçişleri bileşen yapar.
    /// </summary>
    public sealed class LightingSceneStep : ISceneBuildStep
    {
        public const string LightingGroupName = "Lighting";
        public const string SunName = "Sun";
        public const string DayVolumeName = "DayVolume";
        public const string NightVolumeName = "NightVolume";

        // Eşit öncelikte uygulama sırası belirsiz; gece volume'ü gündüzden sonra harmanlansın.
        private const float DayVolumePriority = 0f;
        private const float NightVolumePriority = 1f;

        public int Order => 200;

        public void Build(SceneBuildContext context)
        {
            LightingProfile day = LightingGenerator.LoadOrCreateDay();
            LightingProfile night = LightingGenerator.LoadOrCreateNight();
            GameConfig config = GameFlowGenerator.LoadOrCreateConfig();

            Transform lighting = context.GetOrCreateGroup(context.Gameplay, LightingGroupName);
            Light sun = BuildSun(context, lighting);
            Volume dayVolume = BuildVolume(context, lighting, DayVolumeName, day.VolumeProfile, DayVolumePriority, 1f);
            Volume nightVolume = BuildVolume(context, lighting, NightVolumeName, night.VolumeProfile, NightVolumePriority, 0f);

            // Editörde de gündüz görünsün; oyunda DayNightLighting Awake'te aynısını uygular.
            day.Values.ApplyTo(sun);
            RenderSettings.sun = sun;

            var controller = lighting.gameObject.AddComponent<DayNightLighting>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_dayProfile").objectReferenceValue = day;
            serialized.FindProperty("_nightProfile").objectReferenceValue = night;
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_eventLighting").objectReferenceValue = LightingGenerator.LoadOrCreateNightEvents();
            serialized.FindProperty("_sun").objectReferenceValue = sun;
            serialized.FindProperty("_dayVolume").objectReferenceValue = dayVolume;
            serialized.FindProperty("_nightVolume").objectReferenceValue = nightVolume;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Light BuildSun(SceneBuildContext context, Transform parent)
        {
            GameObject sun = context.CreateObject(SunName, parent);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            return light;
        }

        private static Volume BuildVolume(
            SceneBuildContext context, Transform parent, string name, VolumeProfile profile, float priority, float weight)
        {
            GameObject volumeObject = context.CreateObject(name, parent);
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = priority;
            volume.weight = weight;
            volume.sharedProfile = profile;
            if (profile == null)
            {
                Debug.LogWarning($"[Last Haven] Lighting: {name} için post-process profili yok; ışık profiline VolumeProfile ata.", volume);
            }

            return volume;
        }
    }
}
