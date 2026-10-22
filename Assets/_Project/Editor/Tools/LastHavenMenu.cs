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

            // Oyun sahnesi en son: Rebuild All bitince açık sahne Game.unity olsun.
            bool menuBuilt = MainMenuSceneBuilder.Build();
            bool sceneBuilt = GameSceneBuilder.Build();

            if (failed > 0 || !menuBuilt || !sceneBuilt)
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

        [MenuItem(Root + "Build Main Menu Scene", priority = 24)]
        public static void BuildMainMenuScene()
        {
            if (CanRun())
            {
                MainMenuSceneBuilder.Build();
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

        [MenuItem(Root + "Export Balance Sheet", priority = 41)]
        public static void ExportBalanceSheet()
        {
            if (CanRun())
            {
                BalanceSheetExporter.Export();
            }
        }

        [MenuItem(Root + "Build/Windows", priority = 60)]
        public static void BuildWindows()
        {
            if (CanRun())
            {
                PlayerBuilder.Build(PlayerBuildPlatform.Windows);
            }
        }

        [MenuItem(Root + "Build/macOS", priority = 61)]
        public static void BuildMacOS()
        {
            if (CanRun())
            {
                PlayerBuilder.Build(PlayerBuildPlatform.MacOS);
            }
        }

        [MenuItem(Root + "Build/Apply Player Settings", priority = 80)]
        public static void ApplyPlayerSettings()
        {
            if (CanRun())
            {
                PlayerSettingsSetup.Run();
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
