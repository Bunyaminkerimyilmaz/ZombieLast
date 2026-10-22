using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Validate Project'in Visual (<see cref="PrefabVisualValidator"/>) ve Build Settings (<see cref="BuildSettingsValidator"/>)
    /// kuralları ile Build Game Scene'in sahne sırası (<see cref="BuildSceneList"/>). Asset oluşturmaz, Build Settings'e yazmaz.
    /// </summary>
    public class SceneAndPrefabValidatorTests
    {
        private const string Folder = "Assets/_Project/Scenes";
        private const string Game = Folder + "/Game.unity";
        private const string Menu = Folder + "/MainMenu.unity";
        private const string Sample = "Assets/Scenes/SampleScene.unity";

        private readonly List<Object> _created = new List<Object>();
        private ValidationReport _report;

        [SetUp]
        public void SetUp()
        {
            _report = new ValidationReport();
            _report.BeginSection("Test");
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                Object.DestroyImmediate(_created[i]);
            }

            _created.Clear();
        }

        // --- Visual ---

        [Test]
        public void ValidatePrefab_NoVisual_IsError()
        {
            GameObject root = CreateRoot();

            PrefabVisualValidator.ValidatePrefab(root, _report);

            Assert.AreEqual(1, _report.ErrorCount);
            Assert.AreSame(root, _report.Issues[0].Context);
        }

        [Test]
        public void ValidatePrefab_VisualNotDirectChild_IsError()
        {
            GameObject root = CreateRoot();
            var holder = new GameObject("Model");
            holder.transform.SetParent(root.transform);
            AddVisual(holder.transform, withRenderer: true);

            PrefabVisualValidator.ValidatePrefab(root, _report);

            Assert.AreEqual(1, _report.ErrorCount);
        }

        [Test]
        public void ValidatePrefab_VisualWithoutRenderer_IsWarning()
        {
            GameObject root = CreateRoot();
            AddVisual(root.transform, withRenderer: false);

            PrefabVisualValidator.ValidatePrefab(root, _report);

            Assert.AreEqual(0, _report.ErrorCount);
            Assert.AreEqual(1, _report.WarningCount);
        }

        [Test]
        public void ValidatePrefab_VisualWithRenderer_IsClean()
        {
            GameObject root = CreateRoot();
            AddVisual(root.transform, withRenderer: true);

            PrefabVisualValidator.ValidatePrefab(root, _report);

            Assert.IsTrue(_report.IsClean);
        }

        [Test]
        public void IsExcluded_OnlyFeedbackAndAudioFolders()
        {
            Assert.IsTrue(PrefabVisualValidationStep.IsExcluded("Assets/_Project/Prefabs/Feedback/Tracer.prefab"));
            Assert.IsTrue(PrefabVisualValidationStep.IsExcluded("Assets/_Project/Prefabs/Audio/SoundVoice.prefab"));
            Assert.IsFalse(PrefabVisualValidationStep.IsExcluded("Assets/_Project/Prefabs/Enemies/Walker.prefab"));
            Assert.IsFalse(PrefabVisualValidationStep.IsExcluded("Assets/_Project/Prefabs/FeedbackExtra/X.prefab"));
            Assert.IsFalse(PrefabVisualValidationStep.IsExcluded("Assets/_Project/Prefabs/AudioExtra/X.prefab"));
        }

        // --- Build Settings kuralları ---

        [Test]
        public void Validate_ProjectSceneFirstAndEnabled_IsClean()
        {
            Validate(new[] { Scene(Game), Scene(Sample) }, Game);

            Assert.IsTrue(_report.IsClean);
        }

        [Test]
        public void Validate_ProjectSceneMissing_IsError()
        {
            Validate(new[] { Scene(Game) }, Game, Menu);

            Assert.AreEqual(1, _report.ErrorCount);
            StringAssert.Contains(Menu, _report.Issues[0].Message);
        }

        [Test]
        public void Validate_ProjectSceneDisabled_IsWarning()
        {
            Validate(new[] { Scene(Game), Scene(Menu, enabled: false) }, Game, Menu);

            Assert.AreEqual(0, _report.ErrorCount);
            Assert.AreEqual(1, _report.WarningCount);
        }

        [Test]
        public void Validate_FirstEnabledSceneOutsideProject_IsWarning()
        {
            Validate(new[] { Scene(Sample), Scene(Game) }, Game);

            Assert.AreEqual(0, _report.ErrorCount);
            Assert.AreEqual(1, _report.WarningCount);
            StringAssert.Contains(Sample, _report.Issues[0].Message);
        }

        [Test]
        public void Validate_DisabledExternalSceneBeforeProject_IsClean()
        {
            Validate(new[] { Scene(Sample, enabled: false), Scene(Game) }, Game);

            Assert.IsTrue(_report.IsClean);
        }

        [Test]
        public void Validate_EntryWithoutFile_IsError()
        {
            Validate(new[] { Scene(Game), new BuildSceneEntry(Folder + "/Deleted.unity", true, false) }, Game);

            Assert.AreEqual(1, _report.ErrorCount);
        }

        [Test]
        public void Validate_NoEnabledScene_IsError()
        {
            Validate(new[] { Scene(Game, enabled: false) }, Game);

            Assert.AreEqual(1, _report.ErrorCount);
        }

        [Test]
        public void IsInFolder_IsOrdinalAndWholeFolder()
        {
            Assert.IsTrue(BuildSettingsValidator.IsInFolder(Game, Folder));
            Assert.IsTrue(BuildSettingsValidator.IsInFolder(Game, Folder + "/"));
            Assert.IsFalse(BuildSettingsValidator.IsInFolder("Assets/_Project/ScenesOld/Game.unity", Folder));
            Assert.IsFalse(BuildSettingsValidator.IsInFolder(Sample, Folder));
        }

        // --- Build Game Scene sırası ---

        [Test]
        public void Ensure_MovesGameBeforeExternalScene()
        {
            List<EditorBuildSettingsScene> scenes = List(Sample, Game);

            BuildSceneList.Change change = BuildSceneList.Ensure(scenes, Game, Folder);

            Assert.AreEqual(BuildSceneList.Change.MovedForward, change);
            CollectionAssert.AreEqual(new[] { Game, Sample }, Paths(scenes));
        }

        [Test]
        public void Ensure_AddsBeforeExternalScene()
        {
            List<EditorBuildSettingsScene> scenes = List(Sample);

            BuildSceneList.Change change = BuildSceneList.Ensure(scenes, Game, Folder);

            Assert.AreEqual(BuildSceneList.Change.Added, change);
            CollectionAssert.AreEqual(new[] { Game, Sample }, Paths(scenes));
            Assert.IsTrue(scenes[0].enabled);
        }

        [Test]
        public void Ensure_KeepsOrderAmongProjectScenes()
        {
            List<EditorBuildSettingsScene> scenes = List(Menu, Sample, Game);

            BuildSceneList.Ensure(scenes, Game, Folder);

            CollectionAssert.AreEqual(new[] { Menu, Game, Sample }, Paths(scenes));
        }

        [Test]
        public void Ensure_EnablesDisabledScene_WithoutMoving()
        {
            List<EditorBuildSettingsScene> scenes = List(Game, Sample);
            scenes[0].enabled = false;

            BuildSceneList.Change change = BuildSceneList.Ensure(scenes, Game, Folder);

            Assert.AreEqual(BuildSceneList.Change.Enabled, change);
            Assert.IsTrue(scenes[0].enabled);
            CollectionAssert.AreEqual(new[] { Game, Sample }, Paths(scenes));
        }

        [Test]
        public void Ensure_AlreadyInPlace_ChangesNothing()
        {
            List<EditorBuildSettingsScene> scenes = List(Game, Sample);

            Assert.AreEqual(BuildSceneList.Change.None, BuildSceneList.Ensure(scenes, Game, Folder));
            CollectionAssert.AreEqual(new[] { Game, Sample }, Paths(scenes));
        }

        // --- 14.3: ana menü açılış sahnesi ---

        [Test]
        public void EnsureFirst_MovesMenuToFront_KeepsOthersOrder()
        {
            List<EditorBuildSettingsScene> scenes = List(Game, Sample, Menu);

            BuildSceneList.Change change = BuildSceneList.EnsureFirst(scenes, Menu);

            Assert.AreEqual(BuildSceneList.Change.MovedForward, change);
            CollectionAssert.AreEqual(new[] { Menu, Game, Sample }, Paths(scenes));
        }

        [Test]
        public void EnsureFirst_AddsAtFront()
        {
            List<EditorBuildSettingsScene> scenes = List(Game, Sample);

            Assert.AreEqual(BuildSceneList.Change.Added, BuildSceneList.EnsureFirst(scenes, Menu));
            CollectionAssert.AreEqual(new[] { Menu, Game, Sample }, Paths(scenes));
            Assert.IsTrue(scenes[0].enabled);
        }

        [Test]
        public void EnsureFirst_AlreadyFirstAndEnabled_ChangesNothing()
        {
            List<EditorBuildSettingsScene> scenes = List(Menu, Game);

            Assert.AreEqual(BuildSceneList.Change.None, BuildSceneList.EnsureFirst(scenes, Menu));
            CollectionAssert.AreEqual(new[] { Menu, Game }, Paths(scenes));
        }

        [Test]
        public void EnsureFirst_EnablesDisabledFirst()
        {
            List<EditorBuildSettingsScene> scenes = List(Menu, Game);
            scenes[0].enabled = false;

            Assert.AreEqual(BuildSceneList.Change.Enabled, BuildSceneList.EnsureFirst(scenes, Menu));
            Assert.IsTrue(scenes[0].enabled);
        }

        [Test]
        public void EnsureGameAfterMenu_KeepsMenuFirst()
        {
            List<EditorBuildSettingsScene> scenes = List(Menu, Sample);

            BuildSceneList.Ensure(scenes, Game, Folder);

            CollectionAssert.AreEqual(new[] { Menu, Game, Sample }, Paths(scenes), "Build Game Scene ana menünün önüne geçmez.");
        }

        [Test]
        public void Validate_StartSceneFirst_IsClean()
        {
            BuildSettingsValidator.Validate(new[] { Scene(Menu), Scene(Game) }, new[] { Game, Menu }, Folder, Menu, _report);

            Assert.IsTrue(_report.IsClean);
        }

        [Test]
        public void Validate_StartSceneNotFirst_IsWarning()
        {
            BuildSettingsValidator.Validate(new[] { Scene(Game), Scene(Menu) }, new[] { Game, Menu }, Folder, Menu, _report);

            Assert.AreEqual(0, _report.ErrorCount);
            Assert.AreEqual(1, _report.WarningCount);
            StringAssert.Contains(Menu, _report.Issues[0].Message);
        }

        [Test]
        public void Validate_StartSceneNotInProject_IsIgnored()
        {
            BuildSettingsValidator.Validate(new[] { Scene(Game) }, new[] { Game }, Folder, Menu, _report);

            Assert.IsTrue(_report.IsClean, "Ana menü sahnesi henüz üretilmediyse kural uygulanmaz.");
        }

        private void Validate(BuildSceneEntry[] build, params string[] projectScenes)
        {
            BuildSettingsValidator.Validate(build, projectScenes, Folder, _report);
        }

        private static BuildSceneEntry Scene(string path, bool enabled = true)
        {
            return new BuildSceneEntry(path, enabled, true);
        }

        private static List<EditorBuildSettingsScene> List(params string[] paths)
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in paths)
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            return scenes;
        }

        private static string[] Paths(List<EditorBuildSettingsScene> scenes)
        {
            var paths = new string[scenes.Count];
            for (int i = 0; i < scenes.Count; i++)
            {
                paths[i] = scenes[i].path;
            }

            return paths;
        }

        private GameObject CreateRoot()
        {
            var root = new GameObject("Entity");
            _created.Add(root);
            return root;
        }

        private static void AddVisual(Transform parent, bool withRenderer)
        {
            var visual = new GameObject(VisualRoot.Name);
            visual.transform.SetParent(parent);
            if (withRenderer)
            {
                var mesh = new GameObject("Mesh");
                mesh.transform.SetParent(visual.transform);
                mesh.AddComponent<MeshRenderer>();
            }
        }
    }
}
