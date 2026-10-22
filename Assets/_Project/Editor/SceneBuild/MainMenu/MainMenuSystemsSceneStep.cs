namespace LastHaven.Editor
{
    /// <summary>
    /// Ana menünün [Systems]'i (14.3): Input (<c>InputReader</c>; menü UI modunda, Esc / B ayarlardan çıkar), Settings
    /// (<c>SettingsService</c> + <c>SettingsApplier</c>) ve Pool (arayüz tıklama sesinin ses kaynakları). Oyun sahnesinin adımlarıyla
    /// aynı kurulum (<see cref="InputSceneStep"/>, <see cref="SettingsSceneStep"/>, <see cref="PoolSceneStep"/>).
    /// </summary>
    public sealed class MainMenuSystemsSceneStep : IMainMenuSceneStep
    {
        public int Order => 0;

        public void Build(SceneBuildContext context)
        {
            InputSceneStep.Create(context);
            SettingsSceneStep.Create(context);
            PoolSceneStep.Create(context);
        }
    }
}
