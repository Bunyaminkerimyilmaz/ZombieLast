using LastHaven.Core;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>[Gameplay]/SpawnZones altına dört yöne birer <see cref="SpawnZone"/> koyar (GDD §8).</summary>
    public sealed class WavesSceneStep : ISceneBuildStep
    {
        public const string SpawnZonesGroupName = "SpawnZones";

        public int Order => 300;

        public void Build(SceneBuildContext context)
        {
            MapLayoutConfig config = MapGenerator.LoadOrCreateConfig();
            Transform group = context.GetOrCreateGroup(context.Gameplay, SpawnZonesGroupName);

            for (int i = 0; i < CompassDirections.All.Count; i++)
            {
                CompassDirection direction = CompassDirections.All[i];
                GameObject zoneObject = context.CreateObject($"SpawnZone_{direction}", group);
                zoneObject.transform.localPosition = config.GetSpawnZoneCenter(direction);

                var zone = zoneObject.AddComponent<SpawnZone>();
                var serialized = new SerializedObject(zone);
                serialized.FindProperty("_direction").enumValueIndex = (int)direction;
                serialized.FindProperty("_radius").floatValue = config.SpawnZoneRadius;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
