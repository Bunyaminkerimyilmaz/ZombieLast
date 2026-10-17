using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Görselin dinlenme pozu (yerel konum, dönüş, ölçek) ve <see cref="ProceduralPose"/>'un ona eklenmesi. <see cref="AnimationDriver"/> kullanır.
    /// </summary>
    internal readonly struct VisualRestPose
    {
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Vector3 _scale;

        private VisualRestPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
        }

        /// <summary>Görselin şu anki yerel pozunu dinlenme pozu olarak alır.</summary>
        public static VisualRestPose Capture(Transform visual)
        {
            return new VisualRestPose(visual.localPosition, visual.localRotation, visual.localScale);
        }

        /// <summary>Dinlenme pozu + prosedürel kayma, dönüş ve ölçek.</summary>
        public void Apply(Transform visual, ProceduralPose pose)
        {
            visual.localPosition = _position + pose.Offset;
            visual.localRotation = pose.Rotation * _rotation;
            visual.localScale = _scale * pose.Scale;
        }
    }
}
