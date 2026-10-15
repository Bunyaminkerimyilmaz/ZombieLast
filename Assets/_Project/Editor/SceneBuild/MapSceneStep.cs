using System.Collections.Generic;
using System.Globalization;
using LastHaven.Core;
using LastHaven.Enemies;
using Unity.AI.Navigation;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Test haritası (GDD §17): zemin + runtime NavMesh, sabit seed'li engeller, görünmez sınır duvarları.
    /// Hepsi [Gameplay]/Map altına kurulur. Üs alanı dairesi <see cref="BaseSceneStep"/>'te.
    /// </summary>
    public sealed class MapSceneStep : ISceneBuildStep
    {
        public const string MapGroupName = "Map";
        public const string GroundName = "Ground";

        private const float PlanePrimitiveSize = 10f;

        public int Order => 100;

        public void Build(SceneBuildContext context)
        {
            MapLayoutConfig config = MapGenerator.LoadOrCreateConfig();
            Transform map = context.GetOrCreateGroup(context.Gameplay, MapGroupName);

            BuildGround(context, map, config);
            BuildObstacles(context, map, config, BaseGenerator.LoadOrCreateAreaConfig().Radius);
            BuildBoundary(context, map, config);
        }

        private static void BuildGround(SceneBuildContext context, Transform map, MapLayoutConfig config)
        {
            GameObject ground = context.CreatePrimitive(
                PrimitiveType.Plane, GroundName, map, PlaceholderMaterialId.Ground, GameLayers.Ground);
            float scale = config.MapSize / PlanePrimitiveSize;
            ground.transform.localScale = new Vector3(scale, 1f, scale);

            // NavMesh editörde bake edilmez; NavMeshBaker oyun başında bu yüzeyi bake eder.
            var surface = ground.AddComponent<NavMeshSurface>();
            NavMeshBaker.ConfigureSurface(surface);
            ground.AddComponent<NavMeshBaker>();
        }

        private static void BuildObstacles(SceneBuildContext context, Transform map, MapLayoutConfig config, float baseAreaRadius)
        {
            Transform group = context.GetOrCreateGroup(map, "Obstacles");
            ObstacleLayoutSettings settings = config.CreateObstacleSettings();
            List<ObstaclePlacement> placements = ObstacleLayout.Generate(
                settings, config.ObstacleShapes, config.CreateObstacleExclusions(baseAreaRadius));

            for (int i = 0; i < placements.Count; i++)
            {
                ObstaclePlacement placement = placements[i];
                string shapeName = config.ObstacleShapes[placement.ShapeIndex].Name;
                string objectName = $"Obstacle_{i.ToString("00", CultureInfo.InvariantCulture)}_{shapeName}";

                GameObject obstacle = context.CreatePrimitive(
                    PrimitiveType.Cube, objectName, group, PlaceholderMaterialId.Obstacle, GameLayers.Obstacle);
                obstacle.transform.localPosition = new Vector3(placement.Position.x, placement.Size.y * 0.5f, placement.Position.y);
                obstacle.transform.localRotation = Quaternion.Euler(0f, placement.YawDegrees, 0f);
                obstacle.transform.localScale = placement.Size;
            }

            if (placements.Count < settings.Count)
            {
                Debug.LogWarning($"[Last Haven] Map: {settings.Count} engel istendi, {placements.Count} yerleşti. " +
                                 $"Halka çok dar veya boşluk çok büyük olabilir ({MapGenerator.ConfigPath}).");
            }
            else
            {
                Debug.Log($"[Last Haven] Map: {placements.Count} engel yerleşti (seed {settings.Seed}).");
            }
        }

        private static void BuildBoundary(SceneBuildContext context, Transform map, MapLayoutConfig config)
        {
            // Görünmez duvarlar Default layer'da: NavMesh bake'ine girmez, oyuncuyu ve fiziği durdurur.
            Transform group = context.GetOrCreateGroup(map, "Boundary");
            float half = config.MapSize * 0.5f;
            float thickness = config.BoundaryWallThickness;
            float height = config.BoundaryWallHeight;
            float length = config.MapSize + thickness * 2f;

            for (int i = 0; i < CompassDirections.All.Count; i++)
            {
                CompassDirection direction = CompassDirections.All[i];
                Vector3 normal = direction.ToVector3();
                GameObject wall = context.CreateObject($"Wall_{direction}", group);
                wall.transform.localPosition = normal * (half + thickness * 0.5f) + Vector3.up * (height * 0.5f);

                var box = wall.AddComponent<BoxCollider>();
                bool alongX = Mathf.Approximately(normal.x, 0f);
                box.size = alongX
                    ? new Vector3(length, height, thickness)
                    : new Vector3(thickness, height, length);
            }
        }
    }
}
