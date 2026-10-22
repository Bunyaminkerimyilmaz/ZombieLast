using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/Pause: oyunun duraklatılması (<see cref="PauseService"/>, <see cref="IPauseService"/>; 13.3).</summary>
    public sealed class PauseSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Pause";

        public int Order => 25;

        public void Build(SceneBuildContext context)
        {
            context.CreateObject(ObjectName, context.Systems).AddComponent<PauseService>();
        }
    }
}
