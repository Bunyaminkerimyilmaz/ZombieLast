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
    /// Bir sahneyi adımlarla kurar (Build Game Scene, 14.3'ten beri Build Main Menu Scene): sahneyi açar veya oluşturur, [Systems] ve
    /// [Gameplay] köklerini silip yeniden oluşturur, <typeparamref name="TStep"/> adımlarını (<see cref="StepDiscovery"/>) sırayla
    /// çalıştırır ve kaydeder. [Environment] yoksa boş oluşturulur, varsa dokunulmaz.
    /// </summary>
    public static class SceneBuildRunner
    {
        public const string SystemsRootName = "[Systems]";
        public const string GameplayRootName = "[Gameplay]";
        public const string EnvironmentRootName = "[Environment]";

        /// <summary>
        /// Sahneyi kurar ve kaydeder. Hatalı adım sayısını döndürür; sahne açılamadıysa (Play Mode, kullanıcı iptal etti) -1.
        /// </summary>
        public static int Run<TStep>(string scenePath, string menuName, Func<TStep, int> orderOf, Action<TStep, SceneBuildContext> build)
            where TStep : class
        {
            if (!TryOpenScene(scenePath, menuName, out Scene scene))
            {
                return -1;
            }

            Transform systems = RecreateRoot(scene, SystemsRootName);
            Transform gameplay = RecreateRoot(scene, GameplayRootName);
            Transform environment = FindRoot(scene, EnvironmentRootName);
            if (environment == null)
            {
                environment = new GameObject(EnvironmentRootName).transform;
            }

            systems.SetSiblingIndex(0);
            gameplay.SetSiblingIndex(1);
            environment.SetSiblingIndex(2);

            var context = new SceneBuildContext(scene, systems, gameplay, new PlaceholderMaterials());
            List<TStep> steps = StepDiscovery.CreateAll(orderOf);
            int failed = 0;
            foreach (TStep step in steps)
            {
                try
                {
                    build(step, context);
                }
                catch (Exception exception)
                {
                    failed++;
                    Debug.LogError($"[Last Haven] {menuName}: {step.GetType().Name} başarısız.");
                    Debug.LogException(exception);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            string summary = $"[Last Haven] {menuName}: {steps.Count} adım, {failed} hata. Kaydedildi: {scenePath}";
            if (failed > 0)
            {
                Debug.LogError(summary);
            }
            else
            {
                Debug.Log(summary);
            }

            return failed;
        }

        /// <summary>Build Settings listesini <paramref name="edit"/> ile düzenler; değiştiyse yazar ve loglar.</summary>
        public static void UpdateBuildSettings(string scenePath, Func<List<EditorBuildSettingsScene>, BuildSceneList.Change> edit)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            BuildSceneList.Change change = edit(scenes);
            if (change == BuildSceneList.Change.None)
            {
                return;
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets(); // ProjectSettings/EditorBuildSettings.asset hemen diske (yoksa editör kapanınca yazılır).
            Debug.Log($"[Last Haven] Build Settings: {scenePath} — {change}.");
        }

        private static bool TryOpenScene(string scenePath, string menuName, out Scene scene)
        {
            scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError($"[Last Haven] {menuName} Play Mode'dayken çalışmaz. Önce Play Mode'dan çık.");
                return false;
            }

            if (string.Equals(scene.path, scenePath, StringComparison.Ordinal))
            {
                return true;
            }

            // Açık sahnede kaydedilmemiş değişiklik varsa kullanıcıya sorulur; iptal ederse hiçbir şey yapılmaz.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning($"[Last Haven] {menuName} iptal edildi.");
                return false;
            }

            if (File.Exists(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                return true;
            }

            EditorAssetUtility.EnsureFolder(scenePath.Substring(0, scenePath.LastIndexOf('/')));
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Last Haven] {scenePath} oluşturuldu.");
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

            return new GameObject(name).transform;
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
    }
}
