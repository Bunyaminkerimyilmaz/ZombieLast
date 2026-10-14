using LastHaven.Core;
using LastHaven.DebugTools;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Gameplay]/TestDummies: silah testi için üs dışında gri kapsül <see cref="TestDummy"/>'ler ve olayları konsola yazan
    /// <see cref="CombatDebugLogger"/>. <see cref="TestDummiesConfig.Enabled"/> kapalıysa hiçbiri kurulmaz.
    /// Kukla kökü: CapsuleCollider + Health + TestDummy, Enemy layer. "Visual": collider'sız gri kapsül.
    /// </summary>
    public sealed class TestDummiesSceneStep : ISceneBuildStep
    {
        public const string GroupName = "TestDummies";

        // Kapsül primitive'i 2 m boy, 0.5 m yarıçap; ayaklar kökün orijininde (oyuncuyla aynı ölçü).
        private const float BodyHeight = 2f;
        private const float BodyRadius = 0.5f;

        public int Order => 600;

        public void Build(SceneBuildContext context)
        {
            TestDummiesConfig config = TestDummiesGenerator.LoadOrCreateConfig();
            if (!config.Enabled)
            {
                Debug.Log($"[Last Haven] TestDummies: kapalı ({TestDummiesGenerator.ConfigPath}); kurulmadı.");
                return;
            }

            GameObject group = context.CreateObject(GroupName, context.Gameplay);
            group.AddComponent<CombatDebugLogger>();

            for (int i = 0; i < config.Positions.Count; i++)
            {
                BuildDummy(context, group.transform, config, i);
            }

            Debug.Log($"[Last Haven] TestDummies: {config.Positions.Count} kukla kuruldu.");
        }

        private static void BuildDummy(SceneBuildContext context, Transform parent, TestDummiesConfig config, int index)
        {
            GameObject root = context.CreateObject($"Dummy_{index + 1}", parent, GameLayers.Enemy);
            root.transform.localPosition = config.Positions[index];

            var collider = root.AddComponent<CapsuleCollider>();
            collider.height = BodyHeight;
            collider.radius = BodyRadius;
            collider.center = new Vector3(0f, BodyHeight * 0.5f, 0f);

            root.AddComponent<Health>();
            var dummy = root.AddComponent<TestDummy>();

            GameObject visual = context.CreateObject(PlayerGenerator.VisualName, root.transform, GameLayers.Enemy);
            GameObject body = context.CreatePrimitive(
                PrimitiveType.Capsule, "Body", visual.transform, PlaceholderMaterialId.TestDummy, GameLayers.Enemy, withCollider: false);
            body.transform.localPosition = new Vector3(0f, BodyHeight * 0.5f, 0f);

            var serialized = new SerializedObject(dummy);
            serialized.FindProperty("_maxHealth").floatValue = config.MaxHealth;
            serialized.FindProperty("_respawnDelay").floatValue = config.RespawnDelay;
            serialized.FindProperty("_visual").objectReferenceValue = visual;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
