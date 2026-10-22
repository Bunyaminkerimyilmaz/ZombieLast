using LastHaven.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ana menünün [Systems]/Audio'su (14.3): oyun sahnesinin çekirdeği (<see cref="AudioSceneStep.CreateCore"/>: ses servisi, arayüz
    /// tıklama sesi, ayarlardaki ses düzeyleri). Müzik ve olay eşlemesi yok (GDD §18'de menü müziği yok); dinleyici menü kamerasında
    /// (<see cref="MainMenuBackdropSceneStep"/>). UI adımından sonra (tıklama sesi ana menünün panelini dinler).
    /// </summary>
    public sealed class MainMenuAudioSceneStep : IMainMenuSceneStep
    {
        public int Order => 810;

        public void Build(SceneBuildContext context)
        {
            (AudioConfig config, AudioEventMap map) = AudioGenerator.LoadOrCreateAll();
            Transform menu = context.Systems.Find($"{UISceneStep.GroupName}/{MainMenuUISceneStep.MainMenuName}");
            UIDocument document = menu != null ? menu.GetComponent<UIDocument>() : null;
            if (document == null)
            {
                Debug.LogWarning("[Last Haven] Main menu audio: ana menünün UIDocument'ı yok; arayüz tıklama sesi çalmayacak.");
            }

            AudioSceneStep.CreateCore(context, config, map, document);
        }
    }
}
