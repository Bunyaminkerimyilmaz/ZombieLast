using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// Game.unity'yi açar veya oluşturur, [Systems] ve [Gameplay] köklerini silip tüm <see cref="ISceneBuildStep"/>'lerle
    /// yeniden kurar, sahneyi kaydeder ve Build Settings'e ekler. [Environment] yoksa boş oluşturulur, varsa dokunulmaz.
    /// </summary>
    public static class GameSceneBuilder
    {
        public const string ScenesFolder = "Assets/_Project/Scenes";
        public const string GameScenePath = ScenesFolder + "/Game.unity";
        public const string SystemsRootName = "[Systems]";
        public const string GameplayRootName = "[Gameplay]";
        public const string EnvironmentRootName = "[Environment]";

        /// <summary>Sahneyi kurar. Kullanıcı kaydetmeyi iptal ederse veya Play Mode'daysa false döner.</summary>
        public static bool Build()
        {
            if (!TryOpenGameScene(out Scene scene))
            {
                return false;
            }

            Transform systems = RecreateRoot(scene, SystemsRootName);
            Transform gameplay = RecreateRoot(scene, GameplayRootName);
            Transform environment = FindRoot(scene, EnvironmentRootName);
            if (environment == null)
            {
                environment = CreateRoot(EnvironmentRootName);
            }

            systems.SetSiblingIndex(0);
            gameplay.SetSiblingIndex(1);
            environment.SetSiblingIndex(2);

            var context = new SceneBuildContext(scene, systems, gameplay, new PlaceholderMaterials());
            List<ISceneBuildStep> steps = StepDiscovery.CreateAll<ISceneBuildStep>(step => step.Order);
            int failed = 0;
            foreach (ISceneBuildStep step in steps)
            {
                try
                {
                    step.Build(context);
                }
                catch (Exception exception)
                {
                    failed++;
                    Debug.LogError($"[Last Haven] Build Game Scene: {step.GetType().Name} başarısız.");
                    Debug.LogException(exception);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EnsureInBuildSettings(GameScenePath);

            string summary = $"[Last Haven] Build Game Scene: {steps.Count} adım, {failed} hata. Kaydedildi: {GameScenePath}";
            if (failed > 0)
            {
                Debug.LogError(summary);
            }
            else
            {
                Debug.Log(summary);
            }

            return failed == 0;
        }

        private static bool TryOpenGameScene(out Scene scene)
        {
            scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Last Haven] Build Game Scene Play Mode'dayken çalışmaz. Önce Play Mode'dan çık.");
                return false;
            }

            if (string.Equals(scene.path, GameScenePath, StringComparison.Ordinal))
            {
                return true;
            }

            // Açık sahnede kaydedilmemiş değişiklik varsa kullanıcıya sorulur; iptal ederse hiçbir şey yapılmaz.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Last Haven] Build Game Scene iptal edildi.");
                return false;
            }

            if (File.Exists(GameScenePath))
            {
                scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
                return true;
            }

            EditorAssetUtility.EnsureFolder(ScenesFolder);
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, GameScenePath);
            Debug.Log($"[Last Haven] {GameScenePath} oluşturuldu.");
            return true;
        }

        private static Transform RecreateRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, name, StringComparison.Ordinal))
                {
                    Object.DestroyImmediate(roots[i]);
                }
            }

            return CreateRoot(name);
        }

        private static Transform FindRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, name, StringComparison.Ordinal))
                {
                    return roots[i].transform;
                }
            }

            return null;
        }

        private static Transform CreateRoot(string name)
        {
            return new GameObject(name).transform;
        }

        private static void EnsureInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (!string.Equals(scenes[i].path, scenePath, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!scenes[i].enabled)
                {
                    scenes[i].enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                    Debug.Log($"[Last Haven] Build Settings: {scenePath} etkinleştirildi.");
                }

                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[Last Haven] Build Settings: {scenePath} eklendi.");
        }
    }
}
