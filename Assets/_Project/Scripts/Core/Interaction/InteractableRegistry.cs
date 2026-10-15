using UnityEngine;

namespace LastHaven.Core
{
    /// <summary><see cref="IInteractableRegistry"/> servisi ([Systems]/Interaction). Mantık <see cref="InteractableSet"/>'te.</summary>
    [DisallowMultipleComponent]
    public sealed class InteractableRegistry : MonoBehaviour, IInteractableRegistry
    {
        private readonly InteractableSet _items = new InteractableSet();

        public int Count => _items.Count;

        public bool Register(IInteractable interactable)
        {
            return _items.Register(interactable);
        }

        public bool Unregister(IInteractable interactable)
        {
            return _items.Unregister(interactable);
        }

        public IInteractable FindNearest(Vector3 position)
        {
            return _items.FindNearest(position);
        }

        private void Awake()
        {
            ServiceLocator.Register<IInteractableRegistry>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IInteractableRegistry>(this);
            _items.Clear();
        }
    }
}
