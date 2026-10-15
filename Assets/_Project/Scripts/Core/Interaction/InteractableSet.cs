using System;
using System.Collections.Generic;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="IInteractableRegistry"/>'nin saf C# implementasyonu. <see cref="InteractableRegistry"/> bunu sarar; testler
    /// doğrudan kullanır. Kaydını silmeden yok edilen Unity nesneleri atlanır.
    /// </summary>
    public sealed class InteractableSet : IInteractableRegistry
    {
        private readonly List<IInteractable> _items = new List<IInteractable>();

        public int Count => _items.Count;

        public bool Register(IInteractable interactable)
        {
            if (interactable == null)
            {
                throw new ArgumentNullException(nameof(interactable));
            }

            if (_items.Contains(interactable))
            {
                return false;
            }

            _items.Add(interactable);
            return true;
        }

        public bool Unregister(IInteractable interactable)
        {
            return interactable != null && _items.Remove(interactable);
        }

        public IInteractable FindNearest(UnityEngine.Vector3 position)
        {
            IInteractable nearest = null;
            float best = float.PositiveInfinity;
            for (int i = 0; i < _items.Count; i++)
            {
                IInteractable item = _items[i];
                if (IsDestroyed(item) || !item.CanInteract)
                {
                    continue;
                }

                float distance = item.GetDistance(position);
                if (distance <= item.InteractionRange && distance < best)
                {
                    best = distance;
                    nearest = item;
                }
            }

            return nearest;
        }

        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>Unity'nin "yok edildi" durumunu da sayar (yok edilmiş bileşenin üyelerine erişmek exception fırlatır).</summary>
        public static bool IsDestroyed(IInteractable interactable)
        {
            return interactable == null || (interactable is UnityEngine.Object unityObject && unityObject == null);
        }
    }
}
