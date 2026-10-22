using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>
    /// Game.unity'yi açar veya oluşturur, [Systems] ve [Gameplay] köklerini silip tüm <see cref="ISceneBuildStep"/>'lerle
    /// yeniden kurar, sahneyi kaydeder ve Build Settings'e ekler (proje dışı sahnelerin önüne; proje sahneleri arasındaki sıra korunur,
    /// ana menü önde kalır: <see cref="BuildSceneList"/>). [Environment] yoksa boş oluşturulur, varsa dokunulmaz
    /// (<see cref="SceneBuildRunner"/>).
    /// </summary>
    public static class GameSceneBuilder
    {
        public const string MenuName = "Build Game Scene";
        public const string ScenesFolder = "Assets/_Project/Scenes";
        public const string GameScenePath = ScenesFolder + "/" + SceneNames.Game + ".unity";
        public const string SystemsRootName = SceneBuildRunner.SystemsRootName;
        public const string GameplayRootName = SceneBuildRunner.GameplayRootName;
        public const string EnvironmentRootName = SceneBuildRunner.EnvironmentRootName;

        /// <summary>Sahneyi kurar. Kullanıcı kaydetmeyi iptal ederse, Play Mode'daysa veya bir adım hata verirse false döner.</summary>
        public static bool Build()
        {
            int failed = SceneBuildRunner.Run<ISceneBuildStep>(GameScenePath, MenuName, step => step.Order,
                (step, context) => step.Build(context));
            if (failed < 0)
            {
                return false;
            }

            SceneBuildRunner.UpdateBuildSettings(GameScenePath, scenes => BuildSceneList.Ensure(scenes, GameScenePath, ScenesFolder));
            return failed == 0;
        }
    }
}
