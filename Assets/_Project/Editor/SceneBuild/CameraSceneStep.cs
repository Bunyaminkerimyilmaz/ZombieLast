using LastHaven.Core;
using LastHaven.Player;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Gameplay]/CameraRig: <see cref="CameraRig"/> (ICameraService) + F9 kısayolu, altında CameraTarget,
    /// Main Camera (CinemachineBrain, URP post-process açık; 14.2'den beri AudioListener yok — <see cref="AudioSceneStep"/>) ve CinemachineCamera (CinemachineFollow, dünya ofseti; 14.1'den beri
    /// CinemachineImpulseListener). Rig'de sarsıntı kaynağı (CinemachineImpulseSource, Uniform). Editörde de doğru açıdan
    /// görünsün diye kameralar varsayılan preset'le oyuncunun başlangıç noktasına yerleştirilir.
    /// </summary>
    public sealed class CameraSceneStep : ISceneBuildStep
    {
        public const string RigName = "CameraRig";
        public const string TargetName = "CameraTarget";
        public const string MainCameraName = "Main Camera";
        public const string CinemachineCameraName = "CinemachineCamera";

        /// <summary>Sarsıntı kanalı: kaynak bu kanalda yayınlar, dinleyici bu maskeyle dinler.</summary>
        public const int ImpulseChannel = 1;

        // Uniform impulse'ta kullanılmaz ama 0 olursa Cinemachine olayı hiç üretmez.
        private const float ImpulseDissipationDistance = 100f;

        public int Order => 500;

        public void Build(SceneBuildContext context)
        {
            CameraConfig config = CameraGenerator.LoadOrCreateConfig();
            Vector3 start = MapGenerator.LoadOrCreateConfig().PlayerSpawnPosition;
            CameraPresetSettings preset = config.GetPreset(config.DefaultPreset);
            Vector3 cameraPosition = start + preset.FollowOffset;

            GameObject rig = context.CreateObject(RigName, context.Gameplay);
            Transform target = context.CreateObject(TargetName, rig.transform).transform;
            target.position = start;

            Camera camera = BuildMainCamera(context, rig.transform, cameraPosition, preset);
            (CinemachineCamera cinemachineCamera, CinemachineFollow follow) =
                BuildCinemachineCamera(context, rig.transform, target, cameraPosition, preset, config.FollowDamping);

            CinemachineImpulseSource impulseSource = BuildImpulseSource(rig, config);
            AddImpulseListener(cinemachineCamera.gameObject);

            var cameraRig = rig.AddComponent<CameraRig>();
            var serialized = new SerializedObject(cameraRig);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_camera").objectReferenceValue = camera;
            serialized.FindProperty("_cinemachineCamera").objectReferenceValue = cinemachineCamera;
            serialized.FindProperty("_follow").objectReferenceValue = follow;
            serialized.FindProperty("_target").objectReferenceValue = target;
            serialized.FindProperty("_impulseSource").objectReferenceValue = impulseSource;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            rig.AddComponent<CameraPresetHotkey>();
        }

        // 14.1 sarsıntı: kaynak her yerde aynı hissedilir (uzaklık zayıflaması CameraRig'de); süre ve eğri ayardan (CameraRig Awake'te
        // de yazar). Alanlar açıkça yazılır: AddComponent'te Reset'in çalışmasına güvenilmez.
        private static CinemachineImpulseSource BuildImpulseSource(GameObject rig, CameraConfig config)
        {
            var source = rig.AddComponent<CinemachineImpulseSource>();
            source.ImpulseDefinition = new CinemachineImpulseDefinition
            {
                ImpulseChannel = ImpulseChannel,
                ImpulseShape = config.ShakeShape,
                CustomImpulseShape = new AnimationCurve(),
                ImpulseDuration = config.ShakeDuration,
                ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
                DissipationDistance = ImpulseDissipationDistance,
            };
            source.DefaultVelocity = Vector3.up;
            return source;
        }

        // Ekran düzleminde (kamera uzayı) sarsıntı; ikincil gürültü yok.
        private static void AddImpulseListener(GameObject cinemachineCamera)
        {
            var listener = cinemachineCamera.AddComponent<CinemachineImpulseListener>();
            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = ImpulseChannel;
            listener.Gain = 1f;
            listener.Use2DDistance = false;
            listener.UseCameraSpace = true;
            listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.Additive;
            listener.ReactionSettings = new CinemachineImpulseListener.ImpulseReaction
            {
                AmplitudeGain = 1f,
                FrequencyGain = 1f,
                Duration = 1f,
            };
        }

        private static Camera BuildMainCamera(
            SceneBuildContext context, Transform parent, Vector3 position, CameraPresetSettings preset)
        {
            GameObject cameraObject = context.CreateObject(MainCameraName, parent);
            cameraObject.tag = GameTags.MainCamera;
            cameraObject.transform.SetPositionAndRotation(position, preset.Rotation);

            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = preset.FieldOfView;

            // URP kamerada post-process varsayılan kapalı; gün/gece Volume'leri (6.2) görünsün diye açılır.
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            // 14.2: AudioListener kamerada değil, oyuncunun üstünde ([Systems]/Audio/Listener, AudioSceneStep).
            cameraObject.AddComponent<CinemachineBrain>();
            return camera;
        }

        private static (CinemachineCamera, CinemachineFollow) BuildCinemachineCamera(
            SceneBuildContext context,
            Transform parent,
            Transform target,
            Vector3 position,
            CameraPresetSettings preset,
            float damping)
        {
            GameObject cameraObject = context.CreateObject(CinemachineCameraName, parent);
            cameraObject.transform.SetPositionAndRotation(position, preset.Rotation);

            var cinemachineCamera = cameraObject.AddComponent<CinemachineCamera>();
            cinemachineCamera.Follow = target;
            LensSettings lens = cinemachineCamera.Lens;
            lens.FieldOfView = preset.FieldOfView;
            cinemachineCamera.Lens = lens;

            // Rotasyon bileşeni eklenmez: açı transform'dan gelir ve preset'te sabit kalır (W = ekranın yukarısı).
            var follow = cameraObject.AddComponent<CinemachineFollow>();
            follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            follow.TrackerSettings.PositionDamping = Vector3.one * damping;
            follow.FollowOffset = preset.FollowOffset;
            return (cinemachineCamera, follow);
        }
    }
}
