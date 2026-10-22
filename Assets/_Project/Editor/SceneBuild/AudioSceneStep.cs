using LastHaven.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/Audio (14.2): <see cref="AudioService"/> (IAudioService), <see cref="AudioEventBinder"/> (olay → ses),
    /// <see cref="UiClickSounds"/> (HUD'un UIDocument'ı üzerinden ortak paneli dinler; UI adımından sonra çalışır), altında Music
    /// (<see cref="MusicController"/> + iki 2D AudioSource, Music grubu) ve Listener (sahnenin tek <see cref="AudioListener"/>'ı +
    /// <see cref="AudioListenerFollow"/>; kameradaki dinleyici 14.2'de kaldırıldı). 14.3: <see cref="AudioVolumeSettings"/> (ayarlardaki
    /// ses düzeyleri); çekirdek (<see cref="CreateCore"/>) ana menü sahnesinde de kurulur.
    /// </summary>
    public sealed class AudioSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Audio";
        public const string MusicName = "Music";
        public const string ListenerName = "Listener";

        private const string ConfigField = "_config";
        private const string MapField = "_map";

        public int Order => 810; // UISceneStep'ten (800) sonra.

        public void Build(SceneBuildContext context)
        {
            (AudioConfig config, AudioEventMap map) = AudioGenerator.LoadOrCreateAll();
            GameObject audio = CreateCore(context, config, map, FindHudDocument(context));
            Assign(audio.AddComponent<AudioEventBinder>(), MapField, map);

            BuildMusic(context, audio.transform, config, map);
            BuildListener(context, audio.transform, config);
        }

        /// <summary>
        /// [Systems]/Audio'nun her sahnedeki çekirdeği (ana menü de kullanır): <see cref="AudioService"/>, <see cref="UiClickSounds"/>
        /// (<paramref name="document"/>'ın paneli) ve 14.3'ten beri <see cref="AudioVolumeSettings"/> (ayarlardaki ses düzeyleri → mixer).
        /// </summary>
        public static GameObject CreateCore(SceneBuildContext context, AudioConfig config, AudioEventMap map, UIDocument document)
        {
            GameObject audio = context.CreateObject(ObjectName, context.Systems);
            Assign(audio.AddComponent<AudioService>(), ConfigField, config);

            var clicks = audio.AddComponent<UiClickSounds>();
            var serializedClicks = new SerializedObject(clicks);
            serializedClicks.FindProperty("_document").objectReferenceValue = document;
            serializedClicks.FindProperty(MapField).objectReferenceValue = map;
            serializedClicks.ApplyModifiedPropertiesWithoutUndo();

            Assign(audio.AddComponent<AudioVolumeSettings>(), ConfigField, config);
            return audio;
        }

        private static void BuildMusic(SceneBuildContext context, Transform parent, AudioConfig config, AudioEventMap map)
        {
            GameObject music = context.CreateObject(MusicName, parent);
            AudioSource deckA = AddDeck(music, config);
            AudioSource deckB = AddDeck(music, config);

            var controller = music.AddComponent<MusicController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty(ConfigField).objectReferenceValue = config;
            serialized.FindProperty(MapField).objectReferenceValue = map;
            serialized.FindProperty("_deckA").objectReferenceValue = deckA;
            serialized.FindProperty("_deckB").objectReferenceValue = deckB;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Müzik: 2D, döngü (parça başına değişir), en yüksek öncelik (kalabalıkta kaynak sınırında kesilmesin).
        private static AudioSource AddDeck(GameObject music, AudioConfig config)
        {
            var source = music.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.priority = 0;
            source.volume = 0f;
            source.outputAudioMixerGroup = config.MusicGroup;
            return source;
        }

        private static void BuildListener(SceneBuildContext context, Transform parent, AudioConfig config)
        {
            GameObject listener = context.CreateObject(ListenerName, parent);
            listener.transform.position = MapGenerator.LoadOrCreateConfig().PlayerSpawnPosition + Vector3.up * config.ListenerHeight;
            listener.AddComponent<AudioListener>();
            Assign(listener.AddComponent<AudioListenerFollow>(), ConfigField, config);
        }

        private static UIDocument FindHudDocument(SceneBuildContext context)
        {
            Transform hud = context.Systems.Find($"{UISceneStep.GroupName}/{UISceneStep.HudName}");
            UIDocument document = hud != null ? hud.GetComponent<UIDocument>() : context.Systems.GetComponentInChildren<UIDocument>(true);
            if (document == null)
            {
                Debug.LogWarning("[Last Haven] Audio: sahnede UIDocument yok; arayüz tıklama sesi çalmayacak.");
            }

            return document;
        }

        private static void Assign(Object component, string field, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
