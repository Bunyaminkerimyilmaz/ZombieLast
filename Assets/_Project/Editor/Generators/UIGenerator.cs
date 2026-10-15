using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// UI Toolkit asset'leri (6.3): <c>UI/PanelSettings.asset</c> (Scale With Screen Size, 1920×1080) eksikse oluşturulur ve
    /// boş temasına <c>UI/Themes/Runtime.tss</c> bağlanır. Var olan ayarlara dokunulmaz. UXML/USS/TSS dosyaları üretilmez; elle
    /// yazılmış metin kaynaklarıdır (yollar burada tek yerde).
    /// </summary>
    public sealed class UIGenerator : IGenerationStep
    {
        public const string Folder = "Assets/_Project/UI";
        public const string PanelSettingsPath = Folder + "/PanelSettings.asset";
        public const string RuntimeThemePath = Folder + "/Themes/Runtime.tss";
        public const string ThemeStylePath = Folder + "/Themes/Theme.uss";
        public const string HudPath = Folder + "/HUD/HUD.uxml";
        public const string DawnPanelPath = Folder + "/Panels/DawnPanel.uxml";
        public const string GameOverPanelPath = Folder + "/Panels/GameOverPanel.uxml";
        public const string VictoryPanelPath = Folder + "/Panels/VictoryPanel.uxml";

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
        }

        public void GeneratePrefabs()
        {
        }
    }
}
