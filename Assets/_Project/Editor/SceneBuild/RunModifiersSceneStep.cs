using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/RunModifiers: <see cref="IRunModifiers"/> servisini kaydeden <see cref="RunModifiersHost"/>.</summary>
    public sealed class RunModifiersSceneStep : ISceneBuildStep
    {
        public const string RunModifiersName = "RunModifiers";

        public int Order => 20;

        public void Build(SceneBuildContext context)
        {
            context.CreateObject(RunModifiersName, context.Systems).AddComponent<RunModifiersHost>();
        }
    }
}
