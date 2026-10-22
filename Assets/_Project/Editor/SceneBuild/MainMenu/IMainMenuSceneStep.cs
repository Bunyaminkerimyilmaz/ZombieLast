namespace LastHaven.Editor
{
    /// <summary>
    /// MainMenu.unity'nin bir bölümünü kurar (14.3; <see cref="MainMenuSceneBuilder"/>). Oyun sahnesinin <see cref="ISceneBuildStep"/>'i
    /// gibi <c>TypeCache</c> ile bulunur ve <see cref="Order"/>'a göre çalışır; parametresiz constructor zorunludur.
    /// </summary>
    /// <remarks>Objeleri sadece [Systems] ve [Gameplay] altına kur; [Environment]'a dokunulmaz.</remarks>
    public interface IMainMenuSceneStep
    {
        /// <summary>Küçük olan önce çalışır.</summary>
        int Order { get; }

        void Build(SceneBuildContext context);
    }
}
