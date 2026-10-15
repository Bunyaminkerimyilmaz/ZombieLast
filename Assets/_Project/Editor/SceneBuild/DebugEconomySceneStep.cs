using LastHaven.DebugTools;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DebugEconomy: F7 +1000 para, F8 +50 yakıt (<see cref="DebugEconomyControls"/>; sadece editör ve development build).
    /// Cüzdanı runtime'da <c>IWallet</c> servisinden alır.
    /// </summary>
    public sealed class DebugEconomySceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DebugEconomy";

        public int Order => 720;

        public void Build(SceneBuildContext context)
        {
            context.CreateObject(ObjectName, context.Systems).AddComponent<DebugEconomyControls>();
        }
    }
}
