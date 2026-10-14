using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Collider → <see cref="IDamageable"/> eşlemesini önbellekler; mermi, patlama ve yakın dövüş isabetleri her seferinde
    /// GetComponent çağırmasın diye. Hasar alamayan collider'lar da (null olarak) önbelleğe girer.
    /// </summary>
    /// <remarks>
    /// Havuzdan gelen nesnelerin collider'ı aynı kaldığı için kayıt geçerli kalır. Yok edilmiş hedef tekrar aranır.
    /// Bir collider'a sonradan IDamageable eklenirse önbellek bunu görmez; bileşenler prefab'da hazır olmalı.
    /// </remarks>
    public static class DamageableLookup
    {
        private static readonly Dictionary<Collider, IDamageable> _cache = new Dictionary<Collider, IDamageable>();

        /// <summary>Collider'ın objesinde veya üst objelerinde IDamageable arar; yoksa null.</summary>
        public static IDamageable Find(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            if (_cache.TryGetValue(collider, out IDamageable cached) && !IsDestroyed(cached))
            {
                return cached;
            }

            IDamageable found = collider.GetComponentInParent<IDamageable>();
            _cache[collider] = found;
            return found;
        }

        private static bool IsDestroyed(IDamageable damageable)
        {
            return damageable is Object unityObject && unityObject == null;
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
