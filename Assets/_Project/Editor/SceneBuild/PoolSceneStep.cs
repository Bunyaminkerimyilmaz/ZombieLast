using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/Pool: <see cref="IPoolService"/>. Havuz kapları Play'de bu objenin altında açılır.</summary>
    public sealed class PoolSceneStep : ISceneBuildStep
    {
        public const string PoolObjectName = "Pool";

        public int Order => 30;

        public void Build(SceneBuildContext context)
        {
            Create(context);
        }

        /// <summary>[Systems]/Pool'u kurar (ana menü sahnesi de kullanır: arayüz sesleri havuzdan).</summary>
        public static PoolService Create(SceneBuildContext context)
        {
            return context.CreateObject(PoolObjectName, context.Systems).AddComponent<PoolService>();
        }
    }
}
