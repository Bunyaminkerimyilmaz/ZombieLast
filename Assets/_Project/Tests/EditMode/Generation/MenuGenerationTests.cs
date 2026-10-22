using System;
using System.Collections.Generic;
using LastHaven.Audio;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.GameFlow;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// 14.3 menülerinin metin kaynakları ve üretilen asset'leri: ana menü / duraklatma / ayarlar UXML'leri, run sonu panellerinin yeni
    /// elementleri, menü ve ayar config'leri, mixer'ın ses düzeyi parametreleri ve Build Settings sırası (MainMenu, Game). Asset
    /// oluşturmaz (üretilmemişse Inconclusive).
    /// </summary>
    public class MenuGenerationTests
    {
        [Test]
        public void MainMenu_ContainsButtons_StartsVisible()
        {
            TemplateContainer root = Instantiate(UIGenerator.MainMenuPath);

            AssertScreenRoot(root, startsHidden: false);
            AssertHas<Button>(root, MenuElementNames.PlayButton);
            AssertHas<Button>(root, MenuElementNames.SettingsButton);
            AssertHas<Button>(root, MenuElementNames.QuitButton);
        }

        [Test]
        public void PauseMenu_ContainsButtons_StartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.PauseMenuPath);

            AssertScreenRoot(root, startsHidden: true);
            AssertHas<Button>(root, MenuElementNames.ResumeButton);
            AssertHas<Button>(root, MenuElementNames.SettingsButton);
            AssertHas<Button>(root, MenuElementNames.MainMenuButton);
        }

        [Test]
        public void Settings_ContainsAllControls_InsideRows_StartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.SettingsPanelPath);

            AssertScreenRoot(root, startsHidden: true);
            AssertInRow<SliderInt>(root, MenuElementNames.MasterVolumeSlider);
            AssertInRow<SliderInt>(root, MenuElementNames.MusicVolumeSlider);
            AssertInRow<SliderInt>(root, MenuElementNames.SfxVolumeSlider);
            AssertInRow<Label>(root, MenuElementNames.MasterVolumeValue);
            AssertInRow<Label>(root, MenuElementNames.MusicVolumeValue);
            AssertInRow<Label>(root, MenuElementNames.SfxVolumeValue);
            AssertInRow<Button>(root, MenuElementNames.IsometricButton);
            AssertInRow<Button>(root, MenuElementNames.TopDownButton);
            AssertInRow<Toggle>(root, MenuElementNames.ShakeToggle);
            AssertInRow<Toggle>(root, MenuElementNames.FullscreenToggle);
            AssertHas<Button>(root, MenuElementNames.BackButton);

            SliderInt master = root.Q<SliderInt>(MenuElementNames.MasterVolumeSlider);
            Assert.AreEqual(VolumePercent.Min, master.lowValue);
            Assert.AreEqual(VolumePercent.Max, master.highValue);
        }

        [TestCase(UIGenerator.GameOverPanelPath)]
        [TestCase(UIGenerator.VictoryPanelPath)]
        public void RunEndPanels_HaveFullStatsPerksAndBothButtons(string path)
        {
            TemplateContainer root = Instantiate(path);

            AssertHas<Label>(root, PanelElementNames.TotalMoneyValue);
            AssertHas<Label>(root, PanelElementNames.BestComboValue);
            AssertHas<VisualElement>(root, PanelElementNames.RunEndPerks);
            AssertHas<VisualElement>(root, PanelElementNames.RunEndNoPerks);
            AssertHas<Button>(root, PanelElementNames.RestartButton);
            AssertHas<Button>(root, PanelElementNames.RunEndMainMenuButton);
        }

        [Test]
        public void MenuConfigAsset_ExistsWithPositiveValues()
        {
            var config = AssetDatabase.LoadAssetAtPath<MenuConfig>(UIGenerator.MenuConfigPath);
            if (config == null)
            {
                Assert.Inconclusive($"{UIGenerator.MenuConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.Greater(config.VolumeStepPercent, 0);
            Assert.Greater(config.OrbitRadius, 0f);
            Assert.AreNotEqual(0f, config.OrbitSpeed, "Kamera dönmeli.");
        }

        [Test]
        public void SettingsConfigAsset_DefaultsAreFullVolume()
        {
            var config = AssetDatabase.LoadAssetAtPath<SettingsConfig>(SettingsGenerator.ConfigPath);
            if (config == null)
            {
                Assert.Inconclusive($"{SettingsGenerator.ConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            GameSettings defaults = config.Defaults;
            Assert.Greater(defaults.MasterVolume, 0f);
            Assert.Greater(defaults.MusicVolume, 0f);
            Assert.Greater(defaults.SfxVolume, 0f);
        }

        [Test]
        public void Mixer_ExposesConfiguredVolumeParameters()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(AudioGenerator.ConfigPath);
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(AudioGenerator.MixerPath);
            if (config == null || mixer == null)
            {
                Assert.Inconclusive("AudioConfig veya mixer yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            List<string> exposed = AudioMixerParameters.GetExposedNames(mixer);
            CollectionAssert.Contains(exposed, config.MasterVolumeParameter);
            CollectionAssert.Contains(exposed, config.MusicVolumeParameter);
            CollectionAssert.Contains(exposed, config.SfxVolumeParameter);
            Assert.IsTrue(mixer.GetFloat(config.SfxVolumeParameter, out _), "Parametre adıyla okunabilmeli (SetFloat aynı yol).");
        }

        [Test]
        public void BuildSettings_MainMenuFirst_ThenGame()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuSceneBuilder.ScenePath) == null)
            {
                Assert.Inconclusive($"{MainMenuSceneBuilder.ScenePath} yok; Tools/Last Haven/Build Main Menu Scene çalıştır.");
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Assert.GreaterOrEqual(scenes.Length, 2);
            Assert.AreEqual(MainMenuSceneBuilder.ScenePath, scenes[0].path);
            Assert.IsTrue(scenes[0].enabled);
            int game = Array.FindIndex(scenes, scene => string.Equals(scene.path, GameSceneBuilder.GameScenePath, StringComparison.Ordinal));
            Assert.AreEqual(1, game, "Sıra: MainMenu, Game.");
        }

        [Test]
        public void ScenePaths_UseSceneNames()
        {
            StringAssert.EndsWith("/" + SceneNames.MainMenu + ".unity", MainMenuSceneBuilder.ScenePath);
            StringAssert.EndsWith("/" + SceneNames.Game + ".unity", GameSceneBuilder.GameScenePath);
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
            Assert.AreEqual(startsHidden, screen.ClassListContains(UIClassNames.Hidden));
        }

        private static void AssertHas<T>(VisualElement root, string name) where T : VisualElement
        {
            Assert.IsNotNull(root.Q<T>(name), $"UXML'de '{name}' ({typeof(T).Name}) yok.");
        }

        private static void AssertInRow<T>(VisualElement root, string name) where T : VisualElement
        {
            T element = root.Q<T>(name);
            Assert.IsNotNull(element, $"UXML'de '{name}' ({typeof(T).Name}) yok.");
            VisualElement parent = element.parent;
            while (parent != null && !parent.ClassListContains(MenuElementNames.RowClass))
            {
                parent = parent.parent;
            }

            Assert.IsNotNull(parent, $"'{name}' bir '{MenuElementNames.RowClass}' içinde olmalı (odak vurgusu).");
        }
    }
}
