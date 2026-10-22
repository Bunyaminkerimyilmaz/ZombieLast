using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="PlayerBuildConfig"/>'i Player Settings'e (ürün/şirket adı, sürüm, uygulama kimliği, pencere modu) ve Graphics Settings'in
    /// sis modlarına yazar (15.3). Setup Project, Tools/Last Haven/Build/Apply Player Settings ve build menüleri çalıştırır.
    /// Sadece farklı olanı değiştirir ve her değişikliği loglar; tekrar çalıştırmak güvenlidir. Config kurala uymuyorsa hiçbir şey yazmaz.
    /// </summary>
    public static class PlayerSettingsSetup
    {
        private const string GraphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";
        private const string LogPrefix = "[Last Haven] Player Settings:";

        // Graphics Settings → Shader Stripping → Fog Modes (Unity'nin serileştirilmiş alanları; public API yok).
        private const int FogStrippingCustom = 1;

        private static readonly (FogMode Mode, string Property)[] FogKeepProperties =
        {
            (FogMode.Linear, "m_FogKeepLinear"),
            (FogMode.Exponential, "m_FogKeepExp"),
            (FogMode.ExponentialSquared, "m_FogKeepExp2"),
        };

        /// <summary>Config'i yükler (yoksa oluşturur) ve uygular. Config hatalıysa false.</summary>
        public static bool Run()
        {
            return Run(out _);
        }

        /// <inheritdoc cref="Run()"/>
        public static bool Run(out PlayerBuildConfig config)
        {
            config = PlayerBuildGenerator.LoadOrCreateConfig();
            List<string> problems = PlayerBuildRules.Collect(config);
            if (problems.Count > 0)
            {
                Debug.LogError($"{LogPrefix} {PlayerBuildGenerator.ConfigPath} hatalı; Player Settings değiştirilmedi:\n- " +
                               string.Join("\n- ", problems), config);
                return false;
            }

            int changes = ApplyPlayerSettings(config) + ApplyFogStripping(config);
            if (changes > 0)
            {
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"{LogPrefix} {changes} ayar değişti ({config.ProductName} {config.Version}, {config.FullScreenMode}).");
            return true;
        }

        private static int ApplyPlayerSettings(PlayerBuildConfig config)
        {
            int changes = 0;
            changes += Set("Ürün adı", PlayerSettings.productName, config.ProductName, value => PlayerSettings.productName = value);
            changes += Set("Şirket adı", PlayerSettings.companyName, config.CompanyName, value => PlayerSettings.companyName = value);
            changes += Set("Sürüm", PlayerSettings.bundleVersion, config.Version, value => PlayerSettings.bundleVersion = value);
            changes += Set("Uygulama kimliği", PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Standalone), config.ApplicationIdentifier,
                value => PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, value));
            changes += Set("Pencere modu", PlayerSettings.fullScreenMode, config.FullScreenMode, value => PlayerSettings.fullScreenMode = value);

            // Tam ekran masaüstü çözünürlüğünde açılır; pencere boyutu sadece pencere modunda kullanılır.
            changes += Set("Tam ekranda yerel çözünürlük", PlayerSettings.defaultIsNativeResolution, true,
                value => PlayerSettings.defaultIsNativeResolution = value);
            changes += Set("Pencere genişliği", PlayerSettings.defaultScreenWidth, config.WindowedWidth, value => PlayerSettings.defaultScreenWidth = value);
            changes += Set("Pencere yüksekliği", PlayerSettings.defaultScreenHeight, config.WindowedHeight, value => PlayerSettings.defaultScreenHeight = value);
            changes += Set("Boyutlandırılabilir pencere", PlayerSettings.resizableWindow, config.ResizableWindow, value => PlayerSettings.resizableWindow = value);
            return changes;
        }

        private static int ApplyFogStripping(PlayerBuildConfig config)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(GraphicsSettingsPath);
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError($"{LogPrefix} {GraphicsSettingsPath} yüklenemedi; sis modları ayarlanmadı.");
                return 0;
            }

            var graphics = new SerializedObject(assets[0]);
            SerializedProperty stripping = graphics.FindProperty("m_FogStripping");
            if (stripping == null)
            {
                Debug.LogError($"{LogPrefix} Graphics Settings'te 'm_FogStripping' yok (Unity sürümü değişmiş olabilir); sis modları ayarlanmadı.");
                return 0;
            }

            int changes = 0;
            if (stripping.intValue != FogStrippingCustom)
            {
                stripping.intValue = FogStrippingCustom;
                Debug.Log($"{LogPrefix} Sis modu ayıklama: Custom.");
                changes++;
            }

            foreach ((FogMode mode, string property) in FogKeepProperties)
            {
                SerializedProperty keep = graphics.FindProperty(property);
                bool wanted = Contains(config.KeptFogModes, mode);
                if (keep != null && keep.boolValue != wanted)
                {
                    keep.boolValue = wanted;
                    Debug.Log($"{LogPrefix} Sis modu {mode}: {(wanted ? "build'de tutulur" : "ayıklanır")}.");
                    changes++;
                }
            }

            if (changes > 0)
            {
                graphics.ApplyModifiedPropertiesWithoutUndo();
            }

            return changes;
        }

        private static int Set<T>(string label, T current, T wanted, Action<T> assign)
        {
            if (EqualityComparer<T>.Default.Equals(current, wanted))
            {
                return 0;
            }

            assign(wanted);
            Debug.Log($"{LogPrefix} {label}: '{current}' → '{wanted}'.");
            return 1;
        }

        private static bool Contains(IReadOnlyList<FogMode> modes, FogMode mode)
        {
            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i] == mode)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
