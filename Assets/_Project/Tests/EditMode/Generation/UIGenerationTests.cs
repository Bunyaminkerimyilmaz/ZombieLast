using LastHaven.Editor;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// UI asset'leri (6.3): UXML'ler kodun aradığı elementleri içerir (ad + tip), paneller gizli başlar, USS/TSS hatasız içe aktarılır,
    /// PanelSettings ayarları görevdeki gibidir. Asset oluşturmaz.
    /// </summary>
    public class UIGenerationTests
    {
        [Test]
        public void Hud_ContainsAllElements()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);

            AssertScreenRoot(root, startsHidden: false);
            foreach (string phaseLabel in HudElementNames.PhaseLabels)
            {
                AssertHas<Label>(root, phaseLabel);
            }

            AssertHas<Label>(root, HudElementNames.DayTimer);
            AssertHas<Label>(root, HudElementNames.NightValue);
            AssertHas<VisualElement>(root, HudElementNames.RemainingGroup);
            AssertHas<Label>(root, HudElementNames.RemainingValue);
            AssertHas<VisualElement>(root, HudElementNames.PlayerHealthFill);
            AssertHas<Label>(root, HudElementNames.PlayerHealthValue);
            AssertHas<VisualElement>(root, HudElementNames.GeneratorHealthFill);
            AssertHas<Label>(root, HudElementNames.GeneratorHealthValue);
            AssertHas<Label>(root, HudElementNames.WeaponName);
            AssertHas<Label>(root, HudElementNames.AmmoValue);
            AssertHas<Label>(root, HudElementNames.MoneyValue);
            AssertHas<Label>(root, HudElementNames.FuelValue);
            AssertHas<VisualElement>(root, HudElementNames.ComboGroup);
            AssertHas<Label>(root, HudElementNames.ComboCount);
            AssertHas<Label>(root, HudElementNames.ComboMultiplier);
            AssertHas<VisualElement>(root, HudElementNames.ComboWindowFill);
        }

        [Test]
        public void Hud_ComboStartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);

            Assert.IsTrue(root.Q(HudElementNames.ComboGroup).ClassListContains(UIClassNames.Hidden), "Kombo yokken gizli olmalı.");
            Assert.IsTrue(root.Q(HudElementNames.ComboMultiplier).ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void DawnPanel_ContainsAllElements_StartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.DawnPanelPath);

            AssertScreenRoot(root, startsHidden: true);
            AssertHas<Label>(root, PanelElementNames.DawnNightValue);
            AssertHas<Label>(root, PanelElementNames.NightKillsValue);
            AssertHas<Label>(root, PanelElementNames.NightMoneyValue);
            AssertHas<Label>(root, PanelElementNames.NightBestComboValue);
            AssertHas<Label>(root, PanelElementNames.InterestValue);
            AssertHas<Label>(root, PanelElementNames.TotalKillsValue);
            AssertHas<Button>(root, PanelElementNames.ContinueButton);
        }

        [TestCase(UIGenerator.GameOverPanelPath)]
        [TestCase(UIGenerator.VictoryPanelPath)]
        public void RunEndPanels_ContainAllElements_StartHidden(string path)
        {
            TemplateContainer root = Instantiate(path);

            AssertScreenRoot(root, startsHidden: true);
            AssertHas<Label>(root, PanelElementNames.RunEndNightValue);
            AssertHas<Label>(root, PanelElementNames.TotalKillsValue);
            AssertHas<Button>(root, PanelElementNames.RestartButton);
        }

        [TestCase(UIGenerator.ThemeStylePath)]
        [TestCase(UIGenerator.Folder + "/HUD/HUD.uss")]
        [TestCase(UIGenerator.Folder + "/Panels/Panels.uss")]
        public void StyleSheets_ImportWithoutErrors(string path)
        {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);

            Assert.IsNotNull(sheet, path);
            Assert.IsFalse(sheet.importedWithErrors, $"{path} hatalı içe aktarıldı; konsola bak.");
            Assert.IsFalse(sheet.importedWithWarnings, $"{path} uyarıyla içe aktarıldı; konsola bak.");
        }

        [Test]
        public void RuntimeTheme_IsThemeStyleSheet()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(UIGenerator.RuntimeThemePath);

            Assert.IsNotNull(theme, UIGenerator.RuntimeThemePath);
            Assert.IsFalse(theme.importedWithErrors);
        }

        [Test]
        public void Configure_WritesScaleWithScreenSize1080p()
        {
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            try
            {
                UIGenerator.Configure(settings);

                AssertScaling(settings);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void PanelSettingsAsset_IsConfiguredWithRuntimeTheme()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(UIGenerator.PanelSettingsPath);
            if (settings == null)
            {
                Assert.Inconclusive($"{UIGenerator.PanelSettingsPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            AssertScaling(settings);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(UIGenerator.RuntimeThemePath), settings.themeStyleSheet);
        }

        private static void AssertScaling(PanelSettings settings)
        {
            Assert.AreEqual(PanelScaleMode.ScaleWithScreenSize, settings.scaleMode);
            Assert.AreEqual(new Vector2Int(1920, 1080), settings.referenceResolution);
            Assert.AreEqual(PanelScreenMatchMode.MatchWidthOrHeight, settings.screenMatchMode);
        }

        private static TemplateContainer Instantiate(string path)
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Assert.IsNotNull(layout, path);
            return layout.Instantiate();
        }

        private static void AssertScreenRoot(VisualElement root, bool startsHidden)
        {
            VisualElement screen = root.Q(className: UIClassNames.Screen);
            Assert.IsNotNull(screen, $"UXML kökünde '{UIClassNames.Screen}' sınıfı yok.");
            Assert.AreEqual(startsHidden, screen.ClassListContains(UIClassNames.Hidden),
                startsHidden ? "Panel gizli başlamalı (editörde Game view'ı kapatmasın)." : "HUD görünür başlamalı.");
        }

        private static void AssertHas<T>(VisualElement root, string name) where T : VisualElement
        {
            Assert.IsNotNull(root.Q<T>(name), $"UXML'de '{name}' ({typeof(T).Name}) yok.");
        }
    }
}
