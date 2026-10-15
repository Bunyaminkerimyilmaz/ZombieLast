using LastHaven.Core;
using LastHaven.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// Düşman prefab'ına can barı kurar: kökün çocuğu <c>HealthBar</c> (<see cref="EnemyHealthBar"/>, Visual dışında,
    /// başın üstünde) + <c>Background</c> ve <c>Fill</c> quad'ları (collider'sız, gölgesiz). Ölçüler GDD'de yok.
    /// </summary>
    public static class EnemyHealthBarBuilder
    {
        public const string ObjectName = "HealthBar";
        public const string FillName = "Fill";
        public const string BackgroundName = "Background";

        // Oyun kamerasından (~22 m) ~70 px genişlik.
        public const float Width = 0.9f;
        private const float Height = 0.1f;
        private const float Border = 0.02f;
        private const float HeadClearance = 0.35f;

        // Dolgu arka planın kameraya bakan tarafında (billboard'da yerel -Z kameraya doğru).
        private const float FillDepth = -0.01f;

        private const string FillFieldName = "_fill";
        private const string WidthFieldName = "_width";

        /// <summary>Can barı yoksa kurar. Kurduysa 1 döndürür. Materyaller null olabilir (testler).</summary>
        public static int Ensure(GameObject root, float bodyHeight, Material background, Material fill)
        {
            if (root.GetComponentInChildren<EnemyHealthBar>(true) != null)
            {
                return 0;
            }

            Build(root.transform, bodyHeight, background, fill);
            return 1;
        }

        private static void Build(Transform root, float bodyHeight, Material backgroundMaterial, Material fillMaterial)
        {
            var bar = new GameObject(ObjectName) { layer = GameLayers.Enemy };
            bar.transform.SetParent(root, false);
            bar.transform.localPosition = new Vector3(0f, bodyHeight + HeadClearance, 0f);
            var component = bar.AddComponent<EnemyHealthBar>();

            GameObject background = CreateQuad(BackgroundName, bar.transform, backgroundMaterial);
            background.transform.localScale = new Vector3(Width + 2f * Border, Height + 2f * Border, 1f);

            GameObject fill = CreateQuad(FillName, bar.transform, fillMaterial);
            fill.transform.localPosition = new Vector3(0f, 0f, FillDepth);
            fill.transform.localScale = new Vector3(Width, Height, 1f);

            var serialized = new SerializedObject(component);
            serialized.FindProperty(FillFieldName).objectReferenceValue = fill.transform;
            serialized.FindProperty(WidthFieldName).floatValue = Width;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateQuad(string name, Transform parent, Material material)
        {
            GameObject quad = PrimitiveFactory.Create(PrimitiveType.Quad, name, parent, material, GameLayers.Enemy, withCollider: false);
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return quad;
        }
    }
}
