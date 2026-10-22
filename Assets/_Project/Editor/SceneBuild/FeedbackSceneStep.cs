using LastHaven.Feedback;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/Feedback: olayları efektlere bağlayan <see cref="FeedbackDirector"/> (config'ler üreticiden). 14.1'den beri altında
    /// ScreenVignette: kırmızı ekran kenarı için global <see cref="Volume"/> (gün/gece Volume'lerinin üstünde, ağırlık 0 başlar) +
    /// <see cref="ScreenVignette"/>; yönetmen hasar flaşını ona verir.
    /// </summary>
    public sealed class FeedbackSceneStep : ISceneBuildStep
    {
        public const string FeedbackObjectName = "Feedback";
        public const string ScreenVignetteName = "ScreenVignette";

        /// <summary>Gün (0) ve gece (1) Volume'lerinin üstünde: kırmızı kenar ağırlığı kadar onları ezer.</summary>
        public const float ScreenVignettePriority = 10f;

        public int Order => 40; // PoolSceneStep'ten (30) sonra.

        public void Build(SceneBuildContext context)
        {
            CombatFeedbackConfig config = FeedbackGenerator.LoadOrCreateAll();
            FeedbackConfig feel = FeedbackFeelAssets.LoadOrCreateConfig();
            GameObject feedback = context.CreateObject(FeedbackObjectName, context.Systems);
            ScreenVignette vignette = BuildScreenVignette(context, feedback.transform, feel);
            var director = feedback.AddComponent<FeedbackDirector>();

            var serialized = new SerializedObject(director);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_feel").objectReferenceValue = feel;
            serialized.FindProperty("_screenVignette").objectReferenceValue = vignette;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ScreenVignette BuildScreenVignette(SceneBuildContext context, Transform parent, FeedbackConfig feel)
        {
            GameObject vignetteObject = context.CreateObject(ScreenVignetteName, parent);
            var volume = vignetteObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = ScreenVignettePriority;
            volume.weight = 0f;
            volume.sharedProfile = FeedbackFeelAssets.LoadOrCreateVignetteProfile();

            var vignette = vignetteObject.AddComponent<ScreenVignette>();
            var serialized = new SerializedObject(vignette);
            serialized.FindProperty("_config").objectReferenceValue = feel;
            serialized.FindProperty("_volume").objectReferenceValue = volume;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return vignette;
        }
    }
}
