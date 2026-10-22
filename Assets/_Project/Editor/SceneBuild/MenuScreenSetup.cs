using LastHaven.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// Menü ekranlarının ortak kurulumu (14.3): ayarlar ekranı hem ana menüde hem oyun sahnesinin duraklatma menüsünde aynıdır
    /// (<c>UI/Menus/Settings.uxml</c> + <see cref="SettingsPanelController"/> + <see cref="MenuConfig"/>); açan menüye bağlanır.
    /// </summary>
    public static class MenuScreenSetup
    {
        public const string SettingsPanelName = "SettingsPanel";

        private const string SettingsPanelField = "_settingsPanel";

        public static SettingsPanelController CreateSettingsPanel(
            SceneBuildContext context, Transform group, PanelSettings settings, float sortingOrder)
        {
            var panel = UISceneStep.CreateScreen<SettingsPanelController>(
                context, group, SettingsPanelName, settings, UIGenerator.SettingsPanelPath, sortingOrder);
            var serialized = new SerializedObject(panel);
            serialized.FindProperty("_config").objectReferenceValue = UIGenerator.LoadOrCreateMenuConfig();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        /// <summary>Ayarlar ekranını açan menünün (<c>_settingsPanel</c> alanı) referansına bağlar.</summary>
        public static void LinkSettingsPanel(Object menu, SettingsPanelController panel)
        {
            var serialized = new SerializedObject(menu);
            serialized.FindProperty(SettingsPanelField).objectReferenceValue = panel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
