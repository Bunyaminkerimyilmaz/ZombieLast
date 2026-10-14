using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/TargetRegistry: <see cref="ITargetRegistry"/> servisi.</summary>
    public sealed class TargetingSceneStep : ISceneBuildStep
    {
        public const string TargetRegistryName = "TargetRegistry";

        public int Order => 10;

        public void Build(SceneBuildContext context)
        {
            context.CreateObject(TargetRegistryName, context.Systems).AddComponent<TargetRegistry>();
        }
    }
}
