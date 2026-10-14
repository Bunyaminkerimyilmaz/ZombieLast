using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary><see cref="ITargetRegistry"/> servisi ([Systems]/TargetRegistry). Mantık <see cref="TargetSet"/>'te.</summary>
    [DisallowMultipleComponent]
    public sealed class TargetRegistry : MonoBehaviour, ITargetRegistry
    {
        private readonly TargetSet _targets = new TargetSet();

        /// <summary>Tüm türlerdeki toplam kayıt sayısı.</summary>
        public int Count => _targets.Count;

        public bool Register(ITargetable target)
        {
            return _targets.Register(target);
        }

        public bool Unregister(ITargetable target)
        {
            return _targets.Unregister(target);
        }

        public ITargetable FindNearest(TargetType type, Vector3 position, float maxDistance)
        {
            return _targets.FindNearest(type, position, maxDistance);
        }

        public IReadOnlyList<ITargetable> GetAll(TargetType type)
        {
            return _targets.GetAll(type);
        }

        private void Awake()
        {
            ServiceLocator.Register<ITargetRegistry>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ITargetRegistry>(this);
            _targets.Clear();
        }
    }
}
