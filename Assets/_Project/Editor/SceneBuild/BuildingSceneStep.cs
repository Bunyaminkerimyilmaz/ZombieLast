using LastHaven.Building;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/BuildMode: inşa ızgarası ve yerleştirme (<see cref="BuildSite"/>) + inşa modu (<see cref="BuildModeController"/>),
    /// <see cref="BuildConfig"/> ile. Altında kapalı "Ghost" (önizleme kökü) ve onun zemin plakası "Footprint" (yatırılmış quad),
    /// kapalı "Highlight" (üzerine gelinen yapının vurgu kabuğu), kapalı "RangeIndicator" (taret menzil halkası, LineRenderer,
    /// <c>M_BuildGhost</c>). Yapıların kopyaları runtime'da oyun başında Ghost ve Highlight'ın altına eklenir.
    /// </summary>
    public sealed class BuildingSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "BuildMode";
        public const string GhostName = "Ghost";
        public const string FootprintName = "Footprint";
        public const string HighlightName = "Highlight";
        public const string RangeName = "RangeIndicator";

        private const int IgnoreRaycastLayer = 2; // Unity'nin yerleşik layer'ı.

        // Plaka üs dairesinin (0.02 m) hemen üstünde: z-fighting olmasın. Menzil halkası plakanın da üstünde.
        private const float FootprintLift = 0.03f;
        private const float RangeLift = 0.05f;

        public int Order => 80;

        public void Build(SceneBuildContext context)
        {
            BuildConfig config = BuildingGenerator.LoadOrCreateAll();
            GameObject buildObject = context.CreateObject(ObjectName, context.Systems);

            var site = buildObject.AddComponent<BuildSite>();
            Assign(site, "_config", config);

            GameObject ghost = context.CreateObject(GhostName, buildObject.transform, IgnoreRaycastLayer);
            GameObject footprint = PrimitiveFactory.Create(
                PrimitiveType.Quad, FootprintName, ghost.transform, config.GhostMaterial, IgnoreRaycastLayer, withCollider: false);
            footprint.transform.SetLocalPositionAndRotation(new Vector3(0f, FootprintLift, 0f), Quaternion.Euler(90f, 0f, 0f));
            var footprintRenderer = footprint.GetComponent<Renderer>();
            footprintRenderer.shadowCastingMode = ShadowCastingMode.Off;
            footprintRenderer.receiveShadows = false;
            ghost.SetActive(false);

            GameObject highlight = context.CreateObject(HighlightName, buildObject.transform, IgnoreRaycastLayer);
            highlight.SetActive(false);

            LineRenderer range = BuildRangeIndicator(context, buildObject.transform, config.GhostMaterial);

            var controller = buildObject.AddComponent<BuildModeController>();
            Assign(controller, "_site", site);
            Assign(controller, "_ghostRoot", ghost.transform);
            Assign(controller, "_ghostFootprint", footprintRenderer);
            Assign(controller, "_highlightRoot", highlight.transform);
            Assign(controller, "_rangeIndicator", range);
        }

        // Menzil halkası: X'te 90° yatırılmış (yerel XY → dünya XZ), TransformZ hizalı, yerel uzayda LineRenderer. Noktaları,
        // kalınlığı ve rengi runtime'da BuildRangePreview yazar (BuildConfig).
        private static LineRenderer BuildRangeIndicator(SceneBuildContext context, Transform parent, Material material)
        {
            GameObject range = context.CreateObject(RangeName, parent, IgnoreRaycastLayer);
            range.transform.SetLocalPositionAndRotation(new Vector3(0f, RangeLift, 0f), Quaternion.Euler(90f, 0f, 0f));
            var line = range.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.loop = true;
            line.alignment = LineAlignment.TransformZ;
            line.positionCount = 0;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.lightProbeUsage = LightProbeUsage.Off;
            line.reflectionProbeUsage = ReflectionProbeUsage.Off;
            range.SetActive(false);
            return line;
        }

        private static void Assign(Component component, string fieldName, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
