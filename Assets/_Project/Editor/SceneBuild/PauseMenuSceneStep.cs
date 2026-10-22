using LastHaven.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/UI/PauseMenu ve SettingsPanel (14.3): duraklatma menüsü (<see cref="PauseMenuController"/>, Esc / Start) ve ondan açılan
    /// ayarlar ekranı (<see cref="MenuScreenSetup"/>). Ortak PanelSettings; sıralama run sonu panellerinin (20) üstünde: duraklatma 30,
    /// ayarlar 40. UI adımından (800) sonra, ses adımından (810; tıklama sesi ortak paneli dinler) önce.
    /// </summary>
    public sealed class PauseMenuSceneStep : ISceneBuildStep
    {
        public const string PauseMenuName = "PauseMenu";
        public const float PauseMenuSortingOrder = 30f;
        public const float SettingsPanelSortingOrder = 40f;

        public int Order => 805;

        public void Build(SceneBuildContext context)
        {
            PanelSettings settings = UIGenerator.LoadOrCreatePanelSettings();
            Transform group = context.GetOrCreateGroup(context.Systems, UISceneStep.GroupName);

            var pause = UISceneStep.CreateScreen<PauseMenuController>(
                context, group, PauseMenuName, settings, UIGenerator.PauseMenuPath, PauseMenuSortingOrder);
            SettingsPanelController panel = MenuScreenSetup.CreateSettingsPanel(context, group, settings, SettingsPanelSortingOrder);
            MenuScreenSetup.LinkSettingsPanel(pause, panel);
        }
    }
}
