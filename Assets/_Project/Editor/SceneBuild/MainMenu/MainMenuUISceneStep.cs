using LastHaven.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ana menünün [Systems]/UI'ı (14.3): MainMenu (<see cref="MainMenuController"/>: Oyna, Ayarlar, Çıkış; sıra 0) ve ondan açılan
    /// SettingsPanel (sıra 10; <see cref="MenuScreenSetup"/>). Oyun sahnesiyle aynı PanelSettings.
    /// </summary>
    public sealed class MainMenuUISceneStep : IMainMenuSceneStep
    {
        public const string MainMenuName = "MainMenu";
        public const float MainMenuSortingOrder = 0f;
        public const float SettingsPanelSortingOrder = 10f;

        public int Order => 800;

        public void Build(SceneBuildContext context)
        {
            PanelSettings settings = UIGenerator.LoadOrCreatePanelSettings();
            Transform group = context.GetOrCreateGroup(context.Systems, UISceneStep.GroupName);

            var menu = UISceneStep.CreateScreen<MainMenuController>(
                context, group, MainMenuName, settings, UIGenerator.MainMenuPath, MainMenuSortingOrder);
            SettingsPanelController panel = MenuScreenSetup.CreateSettingsPanel(context, group, settings, SettingsPanelSortingOrder);
            MenuScreenSetup.LinkSettingsPanel(menu, panel);
        }
    }
}
