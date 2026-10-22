using LastHaven.Core;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Gameplay]/SpawnZones altına dört yöne birer <see cref="SpawnZone"/> koyar (GDD §8; her birinin altında kapalı istihbarat ışık
    /// sütunu, GDD §9) ve [Systems]/WaveSpawner'ı (<see cref="WaveSpawner"/>: config üreticiden, bölgeler buradan; aynı objede
    /// sütunları yakan <see cref="SpawnZoneBeacons"/> ve havuzları gecelere göre ısıtan <see cref="WavePrewarmer"/>) kurar.
    /// </summary>
    public sealed class WavesSceneStep : ISceneBuildStep
    {
        public const string SpawnZonesGroupName = "SpawnZones";
        public const string WaveSpawnerName = "WaveSpawner";

        public int Order => 300;

        public void Build(SceneBuildContext context)
        {
            SpawnZone[] zones = BuildSpawnZones(context);
            BuildWaveSpawner(context, zones);
        }

        private static SpawnZone[] BuildSpawnZones(SceneBuildContext context)
        {
            MapLayoutConfig config = MapGenerator.LoadOrCreateConfig();
            Transform group = context.GetOrCreateGroup(context.Gameplay, SpawnZonesGroupName);
            var zones = new SpawnZone[CompassDirections.All.Count];

            for (int i = 0; i < CompassDirections.All.Count; i++)
            {
                CompassDirection direction = CompassDirections.All[i];
                GameObject zoneObject = context.CreateObject($"SpawnZone_{direction}", group);
                zoneObject.transform.localPosition = config.GetSpawnZoneCenter(direction);

                var zone = zoneObject.AddComponent<SpawnZone>();
                var serialized = new SerializedObject(zone);
                serialized.FindProperty("_direction").enumValueIndex = (int)direction;
                serialized.FindProperty("_radius").floatValue = config.SpawnZoneRadius;
                serialized.FindProperty("_beacon").objectReferenceValue = IntelBeaconMaterial.CreateBeacon(zoneObject.transform);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                zones[i] = zone;
            }

            return zones;
        }

        private static void BuildWaveSpawner(SceneBuildContext context, SpawnZone[] zones)
        {
            GameObject spawnerObject = context.CreateObject(WaveSpawnerName, context.Systems);
            var spawner = spawnerObject.AddComponent<WaveSpawner>();

            var serialized = new SerializedObject(spawner);
            serialized.FindProperty("_config").objectReferenceValue = WavesGenerator.LoadOrCreateConfig();
            SerializedProperty zoneList = serialized.FindProperty("_zones");
            zoneList.arraySize = zones.Length;
            for (int i = 0; i < zones.Length; i++)
            {
                zoneList.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            var beacons = spawnerObject.AddComponent<SpawnZoneBeacons>();
            var serializedBeacons = new SerializedObject(beacons);
            SerializedProperty beaconZones = serializedBeacons.FindProperty("_zones");
            beaconZones.arraySize = zones.Length;
            for (int i = 0; i < zones.Length; i++)
            {
                beaconZones.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
            }

            serializedBeacons.ApplyModifiedPropertiesWithoutUndo();

            // 15.2: havuzları gecelerin ihtiyacına göre ısıt (oynanışta Instantiate olmasın).
            var prewarmer = spawnerObject.AddComponent<WavePrewarmer>();
            var serializedPrewarmer = new SerializedObject(prewarmer);
            serializedPrewarmer.FindProperty("_waves").objectReferenceValue = WavesGenerator.LoadOrCreateConfig();
            serializedPrewarmer.FindProperty("_wanderers").objectReferenceValue = DayWanderersGenerator.LoadOrCreateConfig();
            serializedPrewarmer.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
