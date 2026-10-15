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
    /// Main Camera (CinemachineBrain, URP post-process açık) ve CinemachineCamera (CinemachineFollow, dünya ofseti). Editörde de doğru açıdan
    /// görünsün diye kameralar varsayılan preset'le oyuncunun başlangıç noktasına yerleştirilir.
    /// </summary>
    public sealed class CameraSceneStep : ISceneBuildStep
    {
        public const string RigName = "CameraRig";
        public const string TargetName = "CameraTarget";
        public const string MainCameraName = "Main Camera";
        public const string CinemachineCameraName = "CinemachineCamera";

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

            var cameraRig = rig.AddComponent<CameraRig>();
            var serialized = new SerializedObject(cameraRig);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_camera").objectReferenceValue = camera;
            serialized.FindProperty("_cinemachineCamera").objectReferenceValue = cinemachineCamera;
            serialized.FindProperty("_follow").objectReferenceValue = follow;
            serialized.FindProperty("_target").objectReferenceValue = target;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            rig.AddComponent<CameraPresetHotkey>();
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
            cameraObject.AddComponent<AudioListener>();
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
