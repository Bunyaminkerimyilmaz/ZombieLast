using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Collider → <see cref="IStatusEffectReceiver"/> eşlemesini önbellekler (<see cref="DamageableLookup"/> deseni): tuzak tikleri ve
    /// mermi isabetleri her seferinde GetComponent çağırmasın diye. Efekt alamayan collider'lar da (null olarak) önbelleğe girer.
    /// </summary>
    /// <remarks>Bir collider'a sonradan alıcı eklenirse önbellek bunu görmez; bileşenler prefab'da hazır olmalı.</remarks>
    public static class StatusEffectLookup
    {
        private static readonly Dictionary<Collider, IStatusEffectReceiver> _cache = new Dictionary<Collider, IStatusEffectReceiver>();

        /// <summary>Collider'ın objesinde veya üst objelerinde alıcı arar; yoksa null.</summary>
        public static IStatusEffectReceiver Find(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            if (_cache.TryGetValue(collider, out IStatusEffectReceiver cached) && !IsDestroyed(cached))
            {
                return cached;
            }

            IStatusEffectReceiver found = collider.GetComponentInParent<IStatusEffectReceiver>();
            _cache[collider] = found;
            return found;
        }

        private static bool IsDestroyed(IStatusEffectReceiver receiver)
        {
            return receiver is Object unityObject && unityObject == null;
        }

        internal static int Count => _cache.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
        }

        internal static void Clear()
        {
            _cache.Clear();
        }
    }
}
