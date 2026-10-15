using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Editor
{
    /// <summary>
    /// GEÇİCİ: [Gameplay]/GeneratorPlaceholder — üs merkezinde düşmanların varsayılan hedefi (<see cref="GeneratorPlaceholder"/>).
    /// Kök: BoxCollider + Health + GeneratorPlaceholder + NavMeshObstacle (carve yok), Structure layer, Generator tag.
    /// "Visual": sarı 2×2×2 küp (GDD §18). Gün 5'te gerçek jeneratörün adımı gelince bu dosya silinecek.
    /// </summary>
    public sealed class GeneratorPlaceholderSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "GeneratorPlaceholder";

        // GDD §18: jeneratör 2×2×2 küp.
        private static readonly Vector3 Size = new Vector3(2f, 2f, 2f);

        public int Order => 250;

        public void Build(SceneBuildContext context)
        {
            // Üs alanı haritanın merkezinde (MapSceneStep).
            GameObject root = context.CreateObject(ObjectName, context.Gameplay, GameLayers.Structure);
            root.tag = GameTags.Generator;

            Vector3 center = new Vector3(0f, Size.y * 0.5f, 0f);
            var collider = root.AddComponent<BoxCollider>();
            collider.size = Size;
            collider.center = center;

            root.AddComponent<Health>();
            root.AddComponent<GeneratorPlaceholder>();

            // Carve yok: hedef noktası (merkez) NavMesh üzerinde kalsın; ajanlar yerel kaçınmayla etrafından geçer.
            var obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Size;
            obstacle.center = center;
            obstacle.carving = false;

            GameObject visual = context.CreateObject(PlayerGenerator.VisualName, root.transform, GameLayers.Structure);
            GameObject body = context.CreatePrimitive(
                PrimitiveType.Cube, "Body", visual.transform, PlaceholderMaterialId.Generator, GameLayers.Structure, withCollider: false);
            body.transform.localPosition = center;
            body.transform.localScale = Size;
        }
    }
}
