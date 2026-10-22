using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>
    /// Tools/Last Haven/Build Main Menu Scene (14.3): <c>Scenes/MainMenu.unity</c>'yi açar veya oluşturur, [Systems] ve [Gameplay]'i
    /// tüm <see cref="IMainMenuSceneStep"/>'lerle yeniden kurar, kaydeder ve Build Settings'in başına alır (player build ana menüyle
    /// açılır; sıra MainMenu, Game). [Environment]'a dokunmaz (<see cref="SceneBuildRunner"/>).
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        public const string MenuName = "Build Main Menu Scene";
        public const string ScenePath = GameSceneBuilder.ScenesFolder + "/" + SceneNames.MainMenu + ".unity";

        /// <summary>Sahneyi kurar. Kullanıcı kaydetmeyi iptal ederse, Play Mode'daysa veya bir adım hata verirse false döner.</summary>
        public static bool Build()
        {
            int failed = SceneBuildRunner.Run<IMainMenuSceneStep>(ScenePath, MenuName, step => step.Order,
                (step, context) => step.Build(context));
            if (failed < 0)
            {
                return false;
            }

            SceneBuildRunner.UpdateBuildSettings(ScenePath, scenes => BuildSceneList.EnsureFirst(scenes, ScenePath));
            return failed == 0;
        }
    }
}
