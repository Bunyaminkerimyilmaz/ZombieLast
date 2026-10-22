using LastHaven.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// UI Toolkit asset'leri (6.3): <c>UI/PanelSettings.asset</c> (Scale With Screen Size, 1920×1080) eksikse oluşturulur ve
    /// boş temasına <c>UI/Themes/Runtime.tss</c> bağlanır; 13.3'ten beri seviye kartları ayarları
    /// (<c>Data/UI/LevelUpPanelConfig.asset</c>), 14.1'den beri HUD oyun hissi ayarları (<c>Data/UI/HudFeelConfig.asset</c>), 14.3'ten
    /// beri menü ayarları (<c>Data/UI/MenuConfig.asset</c>) da. Var olan ayarlara dokunulmaz. UXML/USS/TSS dosyaları üretilmez; elle yazılmış
    /// metin kaynaklarıdır (yollar burada tek yerde).
    /// </summary>
    public sealed class UIGenerator : IGenerationStep
    {
        public const string Folder = "Assets/_Project/UI";
        public const string PanelSettingsPath = Folder + "/PanelSettings.asset";
        public const string RuntimeThemePath = Folder + "/Themes/Runtime.tss";
        public const string ThemeStylePath = Folder + "/Themes/Theme.uss";
        public const string HudPath = Folder + "/HUD/HUD.uxml";
        public const string HudStylePath = Folder + "/HUD/HUD.uss";
        public const string HudBuildStylePath = Folder + "/HUD/HudBuild.uss";
        public const string HudInteractionStylePath = Folder + "/HUD/HudInteraction.uss";
        public const string HudLootStylePath = Folder + "/HUD/HudLoot.uss";
        public const string HudIntelStylePath = Folder + "/HUD/HudIntel.uss";
        public const string BuildCardPath = Folder + "/HUD/BuildCard.uxml";
        public const string PanelsStylePath = Folder + "/Panels/Panels.uss";
        public const string DawnPanelPath = Folder + "/Panels/DawnPanel.uxml";
        public const string GameOverPanelPath = Folder + "/Panels/GameOverPanel.uxml";
        public const string VictoryPanelPath = Folder + "/Panels/VictoryPanel.uxml";
        public const string ShopPanelPath = Folder + "/Shop/Shop.uxml";
        public const string ShopCardPath = Folder + "/Shop/ShopCard.uxml";
        public const string ShopStylePath = Folder + "/Shop/Shop.uss";
        public const string HudPerksStylePath = Folder + "/HUD/HudPerks.uss";
        public const string LevelUpPanelPath = Folder + "/LevelUp/LevelUpPanel.uxml";
        public const string PerkCardPath = Folder + "/LevelUp/PerkCard.uxml";
        public const string LevelUpStylePath = Folder + "/LevelUp/LevelUp.uss";
        public const string HudFeelStylePath = Folder + "/HUD/HudFeel.uss";

        // 14.3: ana menü, duraklatma menüsü, ayarlar.
        public const string MainMenuPath = Folder + "/Menus/MainMenu.uxml";
        public const string PauseMenuPath = Folder + "/Menus/PauseMenu.uxml";
        public const string SettingsPanelPath = Folder + "/Menus/Settings.uxml";
        public const string MenusStylePath = Folder + "/Menus/Menus.uss";

        /// <summary>Menü ayarları: ses kaydırıcısı adımı, ana menü kamerası (14.3).</summary>
        public const string MenuConfigPath = "Assets/_Project/Data/UI/MenuConfig.asset";

        /// <summary>Seviye kartları zamanlamaları (13.3).</summary>
        public const string LevelUpConfigPath = "Assets/_Project/Data/UI/LevelUpPanelConfig.asset";

        /// <summary>HUD oyun hissi: uçan yazılar ve evre başlığı (14.1).</summary>
        public const string HudFeelConfigPath = "Assets/_Project/Data/UI/HudFeelConfig.asset";

        /// <summary>Referans çözünürlük (görev: 1920×1080).</summary>
        public static readonly Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);

        /// <summary>Genişlik/yükseklik eşleme oranı: 0.5 = ikisinin ortası (16:9 dışı ekranlarda dengeli ölçek; görevde yok).</summary>
        public const float WidthHeightMatch = 0.5f;

        public int Order => 1200;

        public string Name => "UI";

        /// <summary>Sahne adımı buradan alır; Generate Default Data çalışmamış olsa da asset oluşur.</summary>
        public static PanelSettings LoadOrCreatePanelSettings()
        {
            var settings = EditorAssetUtility.LoadOrCreate<PanelSettings>(PanelSettingsPath, out bool created);
            if (created)
            {
                Configure(settings);
                Debug.Log($"[Last Haven] UI: {PanelSettingsPath} oluşturuldu (Scale With Screen Size, {ReferenceResolution.x}×{ReferenceResolution.y}).");
            }

            // Unity yeni PanelSettings'e projedeki herhangi bir TSS'yi kendiliğinden atayabiliyor; ilk oluşturmada bizimkini zorla.
            if (created || settings.themeStyleSheet == null)
            {
                var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(RuntimeThemePath);
                if (theme != null)
                {
                    if (settings.themeStyleSheet != theme)
                    {
                        settings.themeStyleSheet = theme;
                        EditorUtility.SetDirty(settings);
                        Debug.Log($"[Last Haven] UI: PanelSettings temasına {RuntimeThemePath} bağlandı.");
                    }
                }
                else
                {
                    Debug.LogError($"[Last Haven] UI: {RuntimeThemePath} bulunamadı; PanelSettings temasız kaldı (yazılar görünmeyebilir).");
                }
            }

            AssetDatabase.SaveAssetIfDirty(settings);
            return settings;
        }

        /// <summary>Ölçekleme ayarlarını yazar (sadece ilk oluşturmada).</summary>
        public static void Configure(PanelSettings settings)
        {
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = ReferenceResolution;
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = WidthHeightMatch;
            EditorUtility.SetDirty(settings);
        }

        /// <summary>Seviye kartları ayarları (13.3); yoksa varsayılanlarla oluşturulur, varsa dokunulmaz. Sahne adımı buradan alır.</summary>
        public static LevelUpPanelConfig LoadOrCreateLevelUpConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<LevelUpPanelConfig>(LevelUpConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] UI: {LevelUpConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>HUD oyun hissi ayarları (14.1); yoksa varsayılanlarla oluşturulur, varsa dokunulmaz. Sahne adımı buradan alır.</summary>
        public static HudFeelConfig LoadOrCreateHudFeelConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<HudFeelConfig>(HudFeelConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] UI: {HudFeelConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>Menü ayarları (14.3); yoksa varsayılanlarla oluşturulur, varsa dokunulmaz. Sahne adımları buradan alır.</summary>
        public static MenuConfig LoadOrCreateMenuConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<MenuConfig>(MenuConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] UI: {MenuConfigPath} oluşturuldu.");
            }

            return config;
        }

        /// <summary>UXML'i yükler; yoksa hata loglar ve null döner.</summary>
        public static VisualTreeAsset LoadLayout(string path)
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            if (layout == null)
            {
                Debug.LogError($"[Last Haven] UI: {path} bulunamadı.");
            }

            return layout;
        }

        public void GenerateData()
        {
            LoadOrCreatePanelSettings();
            LoadOrCreateLevelUpConfig();
            LoadOrCreateHudFeelConfig();
            LoadOrCreateMenuConfig();
        }

        public void GeneratePrefabs()
        {
        }
    }
}
