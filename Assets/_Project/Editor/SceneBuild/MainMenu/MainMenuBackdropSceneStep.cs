using LastHaven.Core;
using LastHaven.GameFlow;
using LastHaven.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ana menünün arka planı (14.3; [Gameplay]): gece ışığında (gece <see cref="LightingProfile"/>'i: güneş, ambiyans, sis, post-process)
    /// yer tutucu bir üs — zemin, üs dairesi, ışıklı jeneratör küpü, birkaç barikat — ve etrafında üsse dönük yer tutucu zombiler
    /// (Yürüyen / Koşucu / Tank kapsülleri, GDD §18 renk ve ölçekleri). Hepsi sadece görsel: mantık bileşeni ve collider yok. Kamera
    /// (<see cref="MenuCameraOrbit"/>, <see cref="MenuConfig"/>) merkezin etrafında yavaşça döner; sahnenin tek
    /// <see cref="AudioListener"/>'ı kamerada. Arka plan sis rengi: ufuk sise karışır.
    /// </summary>
    public sealed class MainMenuBackdropSceneStep : IMainMenuSceneStep
    {
        public const string BackdropName = "Backdrop";
        public const string LightingName = "Lighting";
        public const string SunName = "Sun";
        public const string VolumeName = "NightVolume";
        public const string CameraName = "MenuCamera";
        public const string GeneratorName = "Generator";
        public const string GeneratorLightName = "GeneratorLight";

        // Zemin: kenarı kameradan uzakta sise karışsın.
        private const float GroundSize = 160f;
        private const float PlanePrimitiveSize = 10f;
        private const float CylinderPrimitiveHeight = 2f;
        private const float AreaDiskThickness = 0.02f;

        // GDD §18: jeneratör 2×2×2 küp + nokta ışık; barikat 3×1×1.5.
        private const float GeneratorSize = 2f;
        private const float GeneratorLightHeight = 3f;
        private const float GeneratorLightRange = 24f;
        private const float GeneratorLightIntensity = 6f;
        private static readonly Color GeneratorLightColor = new Color(1f, 0.82f, 0.5f);
        private static readonly Vector3 BarricadeSize = new Vector3(3f, 1.5f, 1f);
        private const float BarricadeRadius = 6.5f;
        private static readonly float[] BarricadeAngles = { 25f, 145f, 265f };

        /// <summary>Zombiler: (tip, merkeze uzaklık m, açı derece; 0 = güney). Barikatların hemen dışında, jeneratör ışığında; öne eğik.</summary>
        private static readonly (EnemyDefinition Type, float Radius, float Angle)[] Zombies =
        {
            (EnemyDefinitions.Walker, 8.5f, 10f),
            (EnemyDefinitions.Walker, 9.5f, 55f),
            (EnemyDefinitions.Runner, 8f, 95f),
            (EnemyDefinitions.Walker, 8.5f, 160f),
            (EnemyDefinitions.Walker, 10f, 205f),
            (EnemyDefinitions.Runner, 9f, 245f),
            (EnemyDefinitions.Tank, 10.5f, 315f),
        };

        private const float ZombieLean = 8f;
        private const float CapsulePrimitiveHeight = 2f;

        public int Order => 100;

        public void Build(SceneBuildContext context)
        {
            Transform backdrop = context.CreateObject(BackdropName, context.Gameplay).transform;
            BuildGround(context, backdrop);
            BuildBase(context, backdrop);
            BuildZombies(context, backdrop);
            LightingProfile night = LightingGenerator.LoadOrCreateNight();
            BuildLighting(context, night);
            BuildCamera(context, night);
        }

        private static void BuildGround(SceneBuildContext context, Transform parent)
        {
            GameObject ground = context.CreatePrimitive(PrimitiveType.Plane, "Ground", parent, PlaceholderMaterialId.Ground, withCollider: false);
            float scale = GroundSize / PlanePrimitiveSize;
            ground.transform.localScale = new Vector3(scale, 1f, scale);

            float diameter = BaseGenerator.LoadOrCreateAreaConfig().Radius * 2f;
            GameObject disk = context.CreatePrimitive(
                PrimitiveType.Cylinder, "AreaDisk", parent, PlaceholderMaterialId.BaseArea, withCollider: false);
            disk.transform.localPosition = new Vector3(0f, AreaDiskThickness * 0.5f, 0f);
            disk.transform.localScale = new Vector3(diameter, AreaDiskThickness / CylinderPrimitiveHeight, diameter);
            disk.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void BuildBase(SceneBuildContext context, Transform parent)
        {
            GameObject generator = context.CreatePrimitive(
                PrimitiveType.Cube, GeneratorName, parent, PlaceholderMaterialId.Generator, withCollider: false);
            generator.transform.localPosition = Vector3.up * (GeneratorSize * 0.5f);
            generator.transform.localScale = Vector3.one * GeneratorSize;

            GameObject lightObject = context.CreateObject(GeneratorLightName, parent);
            lightObject.transform.localPosition = Vector3.up * GeneratorLightHeight;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = GeneratorLightRange;
            light.intensity = GeneratorLightIntensity;
            light.color = GeneratorLightColor;
            light.shadows = LightShadows.None; // Nokta ışık gölgesi 6 harita: URP gölge atlasını küçültüp uyarı basıyordu.

            for (int i = 0; i < BarricadeAngles.Length; i++)
            {
                GameObject barricade = context.CreatePrimitive(
                    PrimitiveType.Cube, "Barricade_" + (i + 1), parent, PlaceholderMaterialId.BarricadeWood, withCollider: false);
                Vector3 position = RingPoint(BarricadeRadius, BarricadeAngles[i]);
                barricade.transform.localPosition = position + Vector3.up * (BarricadeSize.y * 0.5f);
                barricade.transform.localRotation = Quaternion.LookRotation(-position.normalized, Vector3.up);
                barricade.transform.localScale = BarricadeSize;
            }
        }

        private static void BuildZombies(SceneBuildContext context, Transform parent)
        {
            Transform group = context.CreateObject("Zombies", parent).transform;
            for (int i = 0; i < Zombies.Length; i++)
            {
                (EnemyDefinition type, float radius, float angle) = Zombies[i];
                GameObject zombie = context.CreatePrimitive(
                    type.BodyShape, type.AssetName + "_" + (i + 1), group, type.Material, withCollider: false);
                Vector3 position = RingPoint(radius, angle);
                float height = CapsulePrimitiveHeight * type.BodyScale.y;
                zombie.transform.localPosition = position + Vector3.up * (height * 0.5f);
                zombie.transform.localRotation = Quaternion.LookRotation(-position.normalized, Vector3.up) * Quaternion.Euler(ZombieLean, 0f, 0f);
                zombie.transform.localScale = type.BodyScale;
            }
        }

        private static void BuildLighting(SceneBuildContext context, LightingProfile night)
        {
            Transform lighting = context.CreateObject(LightingName, context.Gameplay).transform;
            GameObject sunObject = context.CreateObject(SunName, lighting);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            night.Values.ApplyTo(sun); // Güneş + RenderSettings (ambiyans, sis) sahneye kaydedilir; runtime bileşeni yok.
            RenderSettings.sun = sun;

            GameObject volumeObject = context.CreateObject(VolumeName, lighting);
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = night.VolumeProfile;
        }

        private static void BuildCamera(SceneBuildContext context, LightingProfile night)
        {
            MenuConfig config = UIGenerator.LoadOrCreateMenuConfig();
            GameObject cameraObject = context.CreateObject(CameraName, context.Gameplay);
            cameraObject.tag = GameTags.MainCamera;
            OrbitPose.Evaluate(config.OrbitCenter, config.OrbitRadius, config.OrbitHeight, config.LookHeight, config.StartAngle,
                config.LookYawOffset, out Vector3 position, out Quaternion rotation);
            cameraObject.transform.SetPositionAndRotation(position, rotation);

            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = config.FieldOfView;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = night.Values.FogColor;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            cameraObject.AddComponent<AudioListener>();

            var orbit = cameraObject.AddComponent<MenuCameraOrbit>();
            var serialized = new SerializedObject(orbit);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // 0° = güney (-Z), açı arttıkça saat yönünün tersine (OrbitPose ile aynı).
        private static Vector3 RingPoint(float radius, float angleDegrees)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians) * radius, 0f, -Mathf.Cos(radians) * radius);
        }
    }
}
