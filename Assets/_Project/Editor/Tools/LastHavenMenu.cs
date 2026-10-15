using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Tools/Last Haven menüleri. İş yapan kod ilgili sınıflarda; burası sadece giriş noktası.</summary>
    public static class LastHavenMenu
    {
        private const string Root = "Tools/Last Haven/";

        [MenuItem(Root + "Rebuild All", priority = 0)]
        public static void RebuildAll()
        {
            if (!CanRun())
            {
                return;
            }

            ProjectSetup.Run();
            int failed = GenerationPipeline.GenerateData();
            failed += GenerationPipeline.GeneratePrefabs();
            bool sceneBuilt = GameSceneBuilder.Build();

            if (failed > 0 || !sceneBuilt)
            {
                Debug.LogError("[Last Haven] Rebuild All hatalarla bitti; yukarıdaki log'lara bak.");
            }
            else
            {
                Debug.Log("[Last Haven] Rebuild All tamamlandı.");
            }
        }

        [MenuItem(Root + "Setup Project", priority = 20)]
        public static void SetupProject()
        {
            if (CanRun())
            {
                ProjectSetup.Run();
            }
        }

        [MenuItem(Root + "Generate Default Data", priority = 21)]
        public static void GenerateDefaultData()
        {
            if (CanRun())
            {
                GenerationPipeline.GenerateData();
            }
        }

        [MenuItem(Root + "Generate Prefabs", priority = 22)]
        public static void GeneratePrefabs()
        {
            if (CanRun())
            {
                GenerationPipeline.GeneratePrefabs();
            }
        }

        [MenuItem(Root + "Build Game Scene", priority = 23)]
        public static void BuildGameScene()
        {
            if (CanRun())
            {
                GameSceneBuilder.Build();
            }
        }

        [MenuItem(Root + "Validate Project", priority = 40)]
        public static void ValidateProject()
        {
            if (CanRun())
            {
                ProjectValidator.RunAndPublish();
            }
        }

        private static bool CanRun()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Last Haven] Üreticiler Play Mode'dayken çalışmaz. Önce Play Mode'dan çık.");
                return false;
            }

            if (EditorApplication.isCompiling)
            {
                Debug.LogError("[Last Haven] Derleme sürüyor; bitince tekrar dene.");
                return false;
            }

            return true;
        }
    }
}
