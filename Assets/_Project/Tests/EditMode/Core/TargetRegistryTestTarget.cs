using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Testlerde yok edilebilen Unity nesnesi hedefi. MonoBehaviour değil: test assembly'si sadece Editor platformunda
    /// olduğu için Unity onun MonoBehaviour'larını GameObject'e eklemez. ScriptableObject aynı "yok edildi" durumunu verir.
    /// </summary>
    public sealed class TargetRegistryTestTarget : ScriptableObject, ITargetable
    {
        private Transform _transform;

        public Transform Transform => _transform;

        public TargetType TargetType => TargetType.Player;

        public bool IsTargetable => true;

        public void Initialize(Transform transform)
        {
            _transform = transform;
        }
    }
}
