using LastHaven.Feedback;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastHaven.Editor
{
    /// <summary>
    /// Oyun hissi asset'leri (14.1, <see cref="FeedbackGenerator"/> çağırır): <see cref="FeedbackConfig"/>
    /// (<c>Data/Feedback/FeedbackConfig.asset</c>; ilk oluşturmada pompalının orta sarsıntısı ve ağır düşman id'leri — Tank, Brute,
    /// Brute (Final) — yazılır) ve ekran kenarının kırmızı Vignette profili (<c>Data/Feedback/ScreenVignette.asset</c>). Varsa dokunulmaz.
    /// </summary>
    public static class FeedbackFeelAssets
    {
        public const string ConfigPath = "Assets/_Project/Data/Feedback/FeedbackConfig.asset";
        public const string VignetteProfilePath = "Assets/_Project/Data/Feedback/ScreenVignette.asset";

        /// <summary>Pompalının atış sarsıntısı (görev: orta). Diğer silahlar config'in varsayılanını (minik) kullanır.</summary>
        public const float ShotgunShake = 0.2f;

        /// <summary>Kırmızı kenarın yoğunluğu (gece vignette'i 0.4; ağırlık 1'de bunun üstüne çıkar).</summary>
        public const float VignetteIntensity = 0.5f;

        public const float VignetteSmoothness = 0.45f;

        public static readonly Color VignetteColor = new Color(0.78f, 0.02f, 0.02f);

        private static readonly string[] HeavyEnemyIds =
        {
            EnemyDefinitions.Tank.Id, NightDefinitions.BruteId, NightDefinitions.BruteFinalId,
        };

        public static FeedbackConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<FeedbackConfig>(ConfigPath, out bool created);
            if (!created)
            {
                return config;
            }

            var serialized = new SerializedObject(config);
            SerializedProperty shakes = serialized.FindProperty("_weaponShakes");
            shakes.arraySize = 1;
            SerializedProperty shotgun = shakes.GetArrayElementAtIndex(0);
            shotgun.FindPropertyRelative("_weaponId").stringValue = WeaponsGenerator.ShotgunId;
            shotgun.FindPropertyRelative("_strength").floatValue = ShotgunShake;

            SerializedProperty heavy = serialized.FindProperty("_heavyEnemyIds");
            heavy.arraySize = HeavyEnemyIds.Length;
            for (int i = 0; i < HeavyEnemyIds.Length; i++)
            {
                heavy.GetArrayElementAtIndex(i).stringValue = HeavyEnemyIds[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(config);
            Debug.Log($"[Last Haven] Feedback: {ConfigPath} oluşturuldu.");
            return config;
        }

        /// <summary>Sadece Vignette'i override eden profil (renk, yoğunluk, yumuşaklık); diğer efektler gün/gece profillerinde kalır.</summary>
        public static VolumeProfile LoadOrCreateVignetteProfile()
        {
            VolumeProfile profile = PostProcessProfiles.LoadOrCreate(VignetteProfilePath, Populate, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Feedback: {VignetteProfilePath} oluşturuldu.");
            }

            return profile;
        }

        /// <summary>Boş profile kırmızı Vignette'i ekler (bellekte). Testler de kullanır.</summary>
        public static void Populate(VolumeProfile profile)
        {
            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(VignetteIntensity);
            vignette.smoothness.Override(VignetteSmoothness);
            vignette.color.Override(VignetteColor);
        }
    }
}
