using LastHaven.Building;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// Yapı prefab'ına onarım barı kurar (8.3): kökün çocuğu <c>RepairBar</c> (<see cref="StructureRepairBar"/>, Visual dışında,
    /// yapının tepesinin üstünde) + <c>Background</c> ve <c>Fill</c> quad'ları (collider'sız, gölgesiz). Ölçüler GDD'de yok:
    /// düşman barından (0.9 m) biraz geniş, oyun kamerasından ~90 px.
    /// </summary>
    public static class StructureRepairBarBuilder
    {
        public const string ObjectName = "RepairBar";
        public const string FillName = "Fill";
        public const string BackgroundName = "Background";

        public const float Width = 1.2f;
        private const float Height = 0.12f;
        private const float Border = 0.02f;
        private const float TopClearance = 0.45f;

        // Dolgu arka planın kameraya bakan tarafında (billboard'da yerel -Z kameraya doğru).
        private const float FillDepth = -0.01f;

        private const string FillFieldName = "_fill";
        private const string WidthFieldName = "_width";

        /// <summary>Barı kurar ve döndürür. Materyaller null olabilir (testler).</summary>
        /// <param name="structureHeight">Yapının yüksekliği (m); bar onun üstünde durur.</param>
        public static StructureRepairBar Build(Transform root, float structureHeight, Material background, Material fill)
        {
            var bar = new GameObject(ObjectName) { layer = GameLayers.Structure };
            bar.transform.SetParent(root, false);
            bar.transform.localPosition = new Vector3(0f, structureHeight + TopClearance, 0f);
            var component = bar.AddComponent<StructureRepairBar>();

            GameObject backgroundQuad = CreateQuad(BackgroundName, bar.transform, background);
            backgroundQuad.transform.localScale = new Vector3(Width + 2f * Border, Height + 2f * Border, 1f);

            GameObject fillQuad = CreateQuad(FillName, bar.transform, fill);
            fillQuad.transform.localPosition = new Vector3(0f, 0f, FillDepth);
            fillQuad.transform.localScale = new Vector3(Width, Height, 1f);

            var serialized = new SerializedObject(component);
            serialized.FindProperty(FillFieldName).objectReferenceValue = fillQuad.transform;
            serialized.FindProperty(WidthFieldName).floatValue = Width;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return component;
        }

        private static GameObject CreateQuad(string name, Transform parent, Material material)
        {
            GameObject quad = PrimitiveFactory.Create(PrimitiveType.Quad, name, parent, material, GameLayers.Structure, withCollider: false);
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.enabled = false; // Onarım başlayınca görünür.
            return quad;
        }
    }
}
