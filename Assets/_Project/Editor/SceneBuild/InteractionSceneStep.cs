using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/Interaction: <see cref="IInteractableRegistry"/> servisi (onarım, ileride acil yakıt ve sandıklar).</summary>
    public sealed class InteractionSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Interaction";

        public int Order => 15;

        public void Build(SceneBuildContext context)
        {
            context.CreateObject(ObjectName, context.Systems).AddComponent<InteractableRegistry>();
        }
    }
}
