using System.Collections.Generic;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// 15.3 build araçlarının saf kuralları: config doğrulaması (<see cref="PlayerBuildRules"/>), çıktı yolu
    /// (<see cref="PlayerBuildTargets"/>) ve build'e girecek sahneler (<see cref="BuildSceneSelection"/>). Asset oluşturmaz,
    /// Player Settings'e ve Build Settings'e yazmaz.
    /// </summary>
    public class PlayerBuildRulesTests
    {
        private const string Folder = "Assets/_Project/Scenes";
        private const string Game = Folder + "/Game.unity";
        private const string Menu = Folder + "/MainMenu.unity";
        private const string Sample = "Assets/Scenes/SampleScene.unity";

        // --- Config ---

        [Test]
        public void Collect_DefaultConfig_IsValid()
        {
            var config = ScriptableObject.CreateInstance<PlayerBuildConfig>();
            try
            {
                CollectionAssert.IsEmpty(PlayerBuildRules.Collect(config));
                Assert.AreEqual("0.1.0", config.Version);
                Assert.AreEqual(FullScreenMode.FullScreenWindow, config.FullScreenMode);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Collect_NullConfig_IsProblem()
        {
            Assert.AreEqual(1, PlayerBuildRules.Collect(null).Count);
        }

        [TestCase("Last Haven")]
        [TestCase("Last Haven - Day & Night")]
        [TestCase("LastHaven")]
        public void IsValidName_PathSafeNames_AreValid(string name)
        {
            Assert.IsTrue(PlayerBuildRules.IsValidName(name));
        }

        [TestCase("Last Haven: Day & Night")]
        [TestCase("Last/Haven")]
        [TestCase("Last\\Haven")]
        [TestCase("Haven?")]
        [TestCase("Haven*")]
        [TestCase("\"Haven\"")]
        [TestCase("Haven.")]
        [TestCase(" Haven")]
        [TestCase("Haven ")]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void IsValidName_InvalidNames_AreRejected(string name)
        {
            Assert.IsFalse(PlayerBuildRules.IsValidName(name));
        }

        [TestCase("0.1.0", true)]
        [TestCase("1.12.305", true)]
        [TestCase("0.1", false)]
        [TestCase("0.1.0-beta", false)]
        [TestCase("v0.1.0", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsValidVersion(string version, bool expected)
        {
            Assert.AreEqual(expected, PlayerBuildRules.IsValidVersion(version));
        }

        [TestCase("com.lasthaven.game", true)]
        [TestCase("com.last-haven.game2", true)]
        [TestCase("lasthaven", false)]
        [TestCase("com..game", false)]
        [TestCase("com.1haven.game", false)]
        [TestCase("com.last haven.game", false)]
        [TestCase(null, false)]
        public void IsValidApplicationIdentifier(string identifier, bool expected)
        {
            Assert.AreEqual(expected, PlayerBuildRules.IsValidApplicationIdentifier(identifier));
        }

        [TestCase("Builds", true)]
        [TestCase("Builds/Release", true)]
        [TestCase("../Builds", false)]
        [TestCase("Builds/../..", false)]
        [TestCase("/Builds", false)]
        [TestCase("C:/Builds", false)]
        [TestCase("", false)]
        public void IsValidOutputFolder(string folder, bool expected)
        {
            Assert.AreEqual(expected, PlayerBuildRules.IsValidOutputFolder(folder));
        }

        // --- Platform ---

        [Test]
        public void OutputPath_Windows_IsExeUnderPlatformFolder()
        {
            Assert.AreEqual("Builds/Windows/LastHaven.exe", PlayerBuildTargets.OutputPath("Builds", PlayerBuildPlatform.Windows, "LastHaven"));
        }

        [Test]
        public void OutputPath_MacOS_IsAppBundleUnderPlatformFolder()
        {
            Assert.AreEqual("Builds/macOS/LastHaven.app", PlayerBuildTargets.OutputPath("Builds", PlayerBuildPlatform.MacOS, "LastHaven"));
        }

        [Test]
        public void OutputPath_NormalizesSlashesAndKeepsGivenExtension()
        {
            Assert.AreEqual("Builds/Release/Windows/LastHaven.exe",
                PlayerBuildTargets.OutputPath("\\Builds\\Release\\", PlayerBuildPlatform.Windows, "LastHaven.exe"));
        }

        [Test]
        public void Target_MapsToStandaloneTargets()
        {
            Assert.AreEqual(BuildTarget.StandaloneWindows64, PlayerBuildTargets.Target(PlayerBuildPlatform.Windows));
            Assert.AreEqual(BuildTarget.StandaloneOSX, PlayerBuildTargets.Target(PlayerBuildPlatform.MacOS));
        }

        // --- Sahneler ---

        [Test]
        public void Select_KeepsEnabledProjectScenesInOrder_SkipsTemplateScene()
        {
            var skipped = new List<string>();
            List<string> scenes = BuildSceneSelection.Select(new[]
            {
                new BuildSceneEntry(Menu, true, true),
                new BuildSceneEntry(Game, true, true),
                new BuildSceneEntry(Sample, true, true),
            }, Folder, skipped);

            CollectionAssert.AreEqual(new[] { Menu, Game }, scenes);
            CollectionAssert.AreEqual(new[] { Sample }, skipped);
        }

        [Test]
        public void Select_SkipsDisabledAndMissingScenes()
        {
            var skipped = new List<string>();
            List<string> scenes = BuildSceneSelection.Select(new[]
            {
                new BuildSceneEntry(Menu, false, true),
                new BuildSceneEntry(Folder + "/Deleted.unity", true, false),
                new BuildSceneEntry(Game, true, true),
            }, Folder, skipped);

            CollectionAssert.AreEqual(new[] { Game }, scenes);
            Assert.AreEqual(2, skipped.Count);
        }

        [Test]
        public void Select_Empty_ReturnsEmpty()
        {
            CollectionAssert.IsEmpty(BuildSceneSelection.Select(new BuildSceneEntry[0], Folder));
        }
    }
}
