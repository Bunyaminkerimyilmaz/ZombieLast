using LastHaven.Audio;
using LastHaven.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ses altyapısı (14.2): mixer (<c>Data/Audio/LastHavenMixer.mixer</c>; Master → Music, SFX — <see cref="AudioMixerFactory"/>),
    /// <see cref="AudioConfig"/> (gruplar ve ses kaynağı prefab'ı bağlanır), <see cref="AudioDefinitions"/>'taki tüm
    /// <see cref="SoundData"/>'lar (<c>Data/Audio/Sfx/</c>, <c>Data/Audio/Music/</c>; boş klip listesiyle, çalma ayarları ilk
    /// oluşturmada) ve <see cref="AudioEventMap"/> (boş yerler doldurulur, <see cref="AudioEventMapFiller"/>). Prefab adımında
    /// <c>Prefabs/Audio/SoundVoice.prefab</c> (<see cref="SoundVoicePrefab"/>). Var olan asset'lere dokunulmaz; sadece boş referanslar.
    /// 14.3: mixer'da Master / Music / SFX ses düzeyleri açık parametre (adlar <see cref="AudioConfig"/>'te; eksikse açılır).
    /// </summary>
    public sealed class AudioGenerator : IGenerationStep
    {
        public const string ConfigPath = AudioDefinitions.DataFolder + "/AudioConfig.asset";
        public const string EventMapPath = AudioDefinitions.DataFolder + "/AudioEventMap.asset";
        public const string MixerPath = AudioDefinitions.DataFolder + "/LastHavenMixer.mixer";

        public int Order => 1800;

        public string Name => "Audio";

        public void GenerateData()
        {
            LoadOrCreateConfig();
            LoadOrCreateEventMap();
        }

        public void GeneratePrefabs()
        {
            BindVoicePrefab(LoadOrCreateConfig());
        }

        /// <summary>Config + eşleme + ses kaynağı prefab'ı; sahne adımı buradan alır (Generate çalışmamış olsa da hepsi oluşur).</summary>
        public static (AudioConfig Config, AudioEventMap Map) LoadOrCreateAll()
        {
            AudioConfig config = LoadOrCreateConfig();
            BindVoicePrefab(config);
            return (config, LoadOrCreateEventMap());
        }

        /// <summary>Config'i yükler/oluşturur; mixer'ı üretir ve boş grup referanslarını doldurur.</summary>
        public static AudioConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<AudioConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Audio: {ConfigPath} oluşturuldu.");
            }

            AudioMixer mixer = AudioMixerFactory.LoadOrCreate(MixerPath);
            if (mixer == null)
            {
                return config;
            }

            int filled = 0;
            filled += AssignGroup(config, "_sfxGroup", AudioMixerFactory.SfxGroupName);
            filled += AssignGroup(config, "_musicGroup", AudioMixerFactory.MusicGroupName);
            if (filled > 0)
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Audio: {ConfigPath} mixer grupları bağlandı ({filled}).");
            }

            ExposeVolumes(mixer, config);
            return config;
        }

        /// <summary>14.3: ayarlar ekranının yazdığı üç ses düzeyi parametresi (adlar config'ten); eksik olan açılır.</summary>
        private static void ExposeVolumes(AudioMixer mixer, AudioConfig config)
        {
            int exposed = 0;
            exposed += AudioMixerParameters.EnsureExposedVolume(mixer, MixerPath, AudioMixerFactory.MasterGroupName, config.MasterVolumeParameter) ? 1 : 0;
            exposed += AudioMixerParameters.EnsureExposedVolume(mixer, MixerPath, AudioMixerFactory.MusicGroupName, config.MusicVolumeParameter) ? 1 : 0;
            exposed += AudioMixerParameters.EnsureExposedVolume(mixer, MixerPath, AudioMixerFactory.SfxGroupName, config.SfxVolumeParameter) ? 1 : 0;
            if (exposed > 0)
            {
                EditorUtility.SetDirty(mixer);
                AssetDatabase.SaveAssetIfDirty(mixer);
                Debug.Log($"[Last Haven] Audio: {MixerPath} ses düzeyi parametreleri açıldı ({exposed}).");
            }
        }

        /// <summary>Tüm ses verilerini yükler/oluşturur ve eşlemenin boş yerlerini doldurur.</summary>
        public static AudioEventMap LoadOrCreateEventMap()
        {
            var map = EditorAssetUtility.LoadOrCreate<AudioEventMap>(EventMapPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Audio: {EventMapPath} oluşturuldu.");
            }

            var serialized = new SerializedObject(map);
            int filled = 0;
            foreach (AudioDefinitions.Definition definition in AudioDefinitions.Sfx)
            {
                SoundData sound = LoadOrCreateSound(AudioDefinitions.GetSfxPath(definition.AssetName), definition);
                filled += AudioEventMapFiller.Fill(serialized, definition, sound) ? 1 : 0;
            }

            foreach (AudioDefinitions.Definition definition in AudioDefinitions.Music)
            {
                SoundData sound = LoadOrCreateSound(AudioDefinitions.GetMusicPath(definition.AssetName), definition);
                filled += AudioEventMapFiller.Fill(serialized, definition, sound) ? 1 : 0;
            }

            if (filled > 0)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(map);
                Debug.Log($"[Last Haven] Audio: {EventMapPath} içine {filled} ses bağlandı.");
            }

            return map;
        }

        /// <summary>Tanımın çalma ayarlarını yazar (klip listesine dokunmaz). Testler bellek içi asset'le de kullanır.</summary>
        public static void Apply(SoundData sound, AudioDefinitions.Definition definition)
        {
            var serialized = new SerializedObject(sound);
            serialized.FindProperty("_volume").floatValue = definition.Volume;
            serialized.FindProperty("_minPitch").floatValue = definition.MinPitch;
            serialized.FindProperty("_maxPitch").floatValue = definition.MaxPitch;
            serialized.FindProperty("_spatial").intValue = (int)definition.Spatial;
            serialized.FindProperty("_minDistance").floatValue = definition.MinDistance;
            serialized.FindProperty("_maxDistance").floatValue = definition.MaxDistance;
            serialized.FindProperty("_maxSimultaneous").intValue = definition.MaxSimultaneous;
            serialized.FindProperty("_cooldown").floatValue = definition.Cooldown;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (EditorUtility.IsPersistent(sound))
            {
                AssetDatabase.SaveAssetIfDirty(sound);
            }
        }

        private static SoundData LoadOrCreateSound(string path, AudioDefinitions.Definition definition)
        {
            var sound = EditorAssetUtility.LoadOrCreate<SoundData>(path, out bool created);
            if (created)
            {
                Apply(sound, definition);
                Debug.Log($"[Last Haven] Audio: {path} oluşturuldu (klip yok).");
            }

            return sound;
        }

        private static int AssignGroup(AudioConfig config, string field, string groupName)
        {
            AudioMixerGroup group = AudioMixerFactory.FindGroup(MixerPath, groupName);
            return group != null && EditorAssetUtility.AssignIfMissing(config, field, group) ? 1 : 0;
        }

        private static void BindVoicePrefab(AudioConfig config)
        {
            SoundVoice voice = SoundVoicePrefab.LoadOrCreate();
            if (voice != null && EditorAssetUtility.AssignIfMissing(config, "_voicePrefab", voice))
            {
                AssetDatabase.SaveAssetIfDirty(config);
                Debug.Log($"[Last Haven] Audio: {ConfigPath} ses kaynağı prefab'ı bağlandı.");
            }
        }
    }
}
