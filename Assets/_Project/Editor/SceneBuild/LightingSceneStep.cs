using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Gameplay]/Lighting/Sun: gündüz için sıcak beyaz directional light (GDD §18).
    /// Gün/gece ışık geçişi ileride GameFlow'a gelecek; o zaman değerler oradaki config'e taşınır.
    /// </summary>
    public sealed class LightingSceneStep : ISceneBuildStep
    {
        public const string LightingGroupName = "Lighting";
        public const string SunName = "Sun";

        private const float SunIntensity = 1.2f;
        private static readonly Vector3 SunEulerAngles = new Vector3(50f, -30f, 0f);
        private static readonly Color SunColor = new Color(1f, 0.96f, 0.88f);

        public int Order => 200;

        public void Build(SceneBuildContext context)
        {
            Transform lighting = context.GetOrCreateGroup(context.Gameplay, LightingGroupName);
            GameObject sun = context.CreateObject(SunName, lighting);
            sun.transform.localRotation = Quaternion.Euler(SunEulerAngles);

            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = SunColor;
            light.intensity = SunIntensity;
            light.shadows = LightShadows.Soft;
            RenderSettings.sun = light;
        }
    }
}
