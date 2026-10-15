using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Hedef → (<see cref="IDamageable"/>, <see cref="Collider"/>) önbelleği. Düşmanlar hedef değiştiğinde buradan alır;
    /// her seferinde GetComponent çağrılmaz. Hedefler (oyuncu, jeneratör, yapılar) havuzlansa da bileşenleri aynı kalır.
    /// </summary>
    /// <remarks>Bileşenler hedefin kendi objesinde veya üst objelerinde aranır; prefab'da hazır olmalı.</remarks>
    public static class TargetComponentCache
    {
        private static readonly Dictionary<ITargetable, Entry> _cache = new Dictionary<ITargetable, Entry>();

        /// <summary>Hedefin hasar alan bileşeni ve ayak izi collider'ı (ikisi de null olabilir).</summary>
        public static void Get(ITargetable target, out IDamageable damageable, out Collider collider)
        {
            damageable = null;
            collider = null;
            if (target == null || (target is Object unityObject && unityObject == null))
            {
                return;
            }

            if (!_cache.TryGetValue(target, out Entry entry))
            {
                Transform transform = target.Transform;
                entry = new Entry(transform.GetComponentInParent<IDamageable>(), transform.GetComponentInParent<Collider>());
                _cache[target] = entry;
            }

            damageable = entry.Damageable;
            collider = entry.Collider;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
        }

        internal static void Clear()
        {
            _cache.Clear();
        }

        private readonly struct Entry
        {
            public Entry(IDamageable damageable, Collider collider)
            {
                Damageable = damageable;
                Collider = collider;
            }

            public IDamageable Damageable { get; }

            public Collider Collider { get; }
        }
    }
}
