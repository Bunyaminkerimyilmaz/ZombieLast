using LastHaven.DebugTools;
using LastHaven.Player;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/Input: <see cref="InputReader"/> (IInputReader servisi) ve kapalı <see cref="InputDebugLogger"/>.
    /// Logger'ı açmak için Play'de bileşenin onay kutusunu işaretle.
    /// </summary>
    public sealed class InputSceneStep : ISceneBuildStep
    {
        public const string InputObjectName = "Input";

        public int Order => 0;

        public void Build(SceneBuildContext context)
        {
            Create(context);
        }

        /// <summary>[Systems]/Input'u kurar (ana menü sahnesi de kullanır).</summary>
        public static GameObject Create(SceneBuildContext context)
        {
            GameObject input = context.CreateObject(InputObjectName, context.Systems);
            input.AddComponent<InputReader>();
            input.AddComponent<InputDebugLogger>().enabled = false;
            return input;
        }
    }
}
