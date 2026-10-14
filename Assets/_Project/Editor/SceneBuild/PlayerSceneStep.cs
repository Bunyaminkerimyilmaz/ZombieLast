using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>[Gameplay]/Player: Player prefab'ının bir örneği, <see cref="MapLayoutConfig.PlayerSpawnPosition"/>'da.</summary>
    public sealed class PlayerSceneStep : ISceneBuildStep
    {
        public int Order => 400;

        public void Build(SceneBuildContext context)
        {
            GameObject prefab = PlayerGenerator.LoadOrCreatePrefab();
            MapLayoutConfig config = MapGenerator.LoadOrCreateConfig();

            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, context.Scene);
            player.transform.SetParent(context.Gameplay, false);
            player.transform.SetLocalPositionAndRotation(config.PlayerSpawnPosition, Quaternion.identity);
        }
    }
}
