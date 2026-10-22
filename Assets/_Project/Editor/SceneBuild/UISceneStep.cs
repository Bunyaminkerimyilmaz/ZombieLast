using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Player;
using LastHaven.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/UI: her ekran için bir <see cref="UIDocument"/> (ortak PanelSettings) + denetleyicisi. HUD'a oyuncunun ve
    /// jeneratörün <see cref="Health"/>'i bağlanır (Player ve Base adımlarından sonra çalışır); HUD'un bölüm denetleyicileri
    /// (cüzdan, kombo, yakıt/güç, inşa çubuğu, yapı ipucu, etkileşim ipucu, tuş ipuçları, sandık göstergeleri, saldırı okları, istihbarat
    /// paneli, gündüz sonu uyarısı, "Hazırım" ipucu, boss barı, XP şeridi, perk listesi; 14.1: uçan yazılar ve evre başlığı,
    /// ikisine de <see cref="HudFeelConfig"/>) aynı UIDocument'ı paylaşır. İnşa
    /// çubuğuna sahnedeki <see cref="BuildSite"/>'ın <see cref="BuildConfig"/>'i (Building adımından sonra) ve kart şablonu,
    /// market paneline kart şablonu, seviye kartları paneline (13.3) <see cref="LevelUpPanelConfig"/> ve kart şablonu bağlanır.
    /// Sıralama: HUD altta, market onun üstünde, şafak paneli, seviye kartları (şafakta da açılabilir), en üstte run sonu panelleri.
    /// </summary>
    public sealed class UISceneStep : ISceneBuildStep
    {
        public const string GroupName = "UI";
        public const string HudName = "HUD";
        public const string ShopPanelName = "ShopPanel";
        public const string DawnPanelName = "DawnPanel";
        public const string LevelUpPanelName = "LevelUpPanel";
        public const string GameOverPanelName = "GameOverPanel";
        public const string VictoryPanelName = "VictoryPanel";

        public const float HudSortingOrder = 0f;
        public const float ShopPanelSortingOrder = 5f;
        public const float DawnPanelSortingOrder = 10f;
        public const float LevelUpPanelSortingOrder = 15f;
        public const float RunEndPanelSortingOrder = 20f;

        private const string DocumentField = "_document";
        private const string CardTemplateField = "_cardTemplate";
        private const string ConfigField = "_config";

        public int Order => 800;

        public void Build(SceneBuildContext context)
        {
            PanelSettings settings = UIGenerator.LoadOrCreatePanelSettings();
            Transform group = context.CreateObject(GroupName, context.Systems).transform;

            var hud = CreateScreen<HUDController>(context, group, HudName, settings, UIGenerator.HudPath, HudSortingOrder);
            Health playerHealth = FindPlayerHealth(context);
            Health generatorHealth = FindGeneratorHealth(context);
            var serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("_playerHealth").objectReferenceValue = playerHealth;
            serializedHud.FindProperty("_generatorHealth").objectReferenceValue = generatorHealth;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
            AddController<HudWalletController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudComboController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudFuelController>(hud.gameObject, hud.GetComponent<UIDocument>());
            var buildBar = AddController<HudBuildBarController>(hud.gameObject, hud.GetComponent<UIDocument>());
            var serializedBuildBar = new SerializedObject(buildBar);
            serializedBuildBar.FindProperty(ConfigField).objectReferenceValue = FindBuildConfig(context);
            serializedBuildBar.FindProperty(CardTemplateField).objectReferenceValue = UIGenerator.LoadLayout(UIGenerator.BuildCardPath);
            serializedBuildBar.ApplyModifiedPropertiesWithoutUndo();
            AddController<HudStructureTooltipController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudInteractionPromptController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudInputHintsController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudLootIndicatorsController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudAttackIndicatorsController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudIntelController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudDayWarningController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudReadyHintController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudBossController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudXpController>(hud.gameObject, hud.GetComponent<UIDocument>());
            AddController<HudTextWarmupController>(hud.gameObject, hud.GetComponent<UIDocument>()); // 15.2
            AddController<HudPerksController>(hud.gameObject, hud.GetComponent<UIDocument>());
            HudFeelConfig feel = UIGenerator.LoadOrCreateHudFeelConfig();
            AssignConfig(AddController<HudFloatingTextController>(hud.gameObject, hud.GetComponent<UIDocument>()), feel);
            AssignConfig(AddController<HudBannerController>(hud.gameObject, hud.GetComponent<UIDocument>()), feel);

            var shop = CreateScreen<ShopPanelController>(
                context, group, ShopPanelName, settings, UIGenerator.ShopPanelPath, ShopPanelSortingOrder);
            var serializedShop = new SerializedObject(shop);
            serializedShop.FindProperty(CardTemplateField).objectReferenceValue = UIGenerator.LoadLayout(UIGenerator.ShopCardPath);
            serializedShop.ApplyModifiedPropertiesWithoutUndo();

            CreateScreen<DawnPanelController>(
                context, group, DawnPanelName, settings, UIGenerator.DawnPanelPath, DawnPanelSortingOrder);

            var levelUp = CreateScreen<LevelUpPanelController>(
                context, group, LevelUpPanelName, settings, UIGenerator.LevelUpPanelPath, LevelUpPanelSortingOrder);
            var serializedLevelUp = new SerializedObject(levelUp);
            serializedLevelUp.FindProperty(ConfigField).objectReferenceValue = UIGenerator.LoadOrCreateLevelUpConfig();
            serializedLevelUp.FindProperty(CardTemplateField).objectReferenceValue = UIGenerator.LoadLayout(UIGenerator.PerkCardPath);
            serializedLevelUp.ApplyModifiedPropertiesWithoutUndo();

            CreateRunEndPanel(context, group, GameOverPanelName, settings, UIGenerator.GameOverPanelPath, GamePhase.GameOver);
            CreateRunEndPanel(context, group, VictoryPanelName, settings, UIGenerator.VictoryPanelPath, GamePhase.Victory);
        }

        private static void CreateRunEndPanel(
            SceneBuildContext context, Transform group, string name, PanelSettings settings, string layoutPath, GamePhase phase)
        {
            var panel = CreateScreen<RunEndPanelController>(context, group, name, settings, layoutPath, RunEndPanelSortingOrder);
            var serialized = new SerializedObject(panel);
            serialized.FindProperty("_showOnPhase").intValue = (int)phase;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static T CreateScreen<T>(
            SceneBuildContext context, Transform group, string name, PanelSettings settings, string layoutPath, float sortingOrder)
            where T : UIScreen
        {
            GameObject screenObject = context.CreateObject(name, group);
            var document = screenObject.AddComponent<UIDocument>();
            document.panelSettings = settings;
            document.visualTreeAsset = UIGenerator.LoadLayout(layoutPath);
            document.sortingOrder = sortingOrder;
            return AddController<T>(screenObject, document);
        }

        /// <summary>Denetleyiciyi ekler ve UIDocument'ını bağlar (bir UIDocument'ı birden çok denetleyici paylaşabilir).</summary>
        public static T AddController<T>(GameObject screenObject, UIDocument document) where T : UIDocumentController
        {
            var controller = screenObject.AddComponent<T>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty(DocumentField).objectReferenceValue = document;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static void AssignConfig(Object controller, Object config)
        {
            var serialized = new SerializedObject(controller);
            serialized.FindProperty(ConfigField).objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Health FindPlayerHealth(SceneBuildContext context)
        {
            var player = context.Gameplay.GetComponentInChildren<PlayerCharacter>(true);
            if (player == null)
            {
                Debug.LogWarning("[Last Haven] UI: sahnede oyuncu yok; HUD can barı bağlanmadı.");
                return null;
            }

            return player.GetComponent<Health>();
        }

        private static BuildConfig FindBuildConfig(SceneBuildContext context)
        {
            var site = context.Systems.GetComponentInChildren<BuildSite>(true);
            if (site == null || site.Config == null)
            {
                Debug.LogWarning("[Last Haven] UI: sahnede BuildSite veya BuildConfig yok; inşa çubuğu boş kalacak.");
                return null;
            }

            return site.Config;
        }

        private static Health FindGeneratorHealth(SceneBuildContext context)
        {
            var generator = context.Gameplay.GetComponentInChildren<Generator>(true);
            if (generator == null)
            {
                Debug.LogWarning("[Last Haven] UI: sahnede jeneratör yok; HUD jeneratör barı bağlanmadı.");
                return null;
            }

            return generator.GetComponent<Health>();
        }
    }
}
