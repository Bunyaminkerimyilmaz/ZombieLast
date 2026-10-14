using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Arayüz tipine göre servis kaydı. Kayıt <c>Awake</c>'te, erişim <c>Start</c>'ta veya sonrasında yapılır.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Servisi <typeparamref name="T"/> tipiyle kaydeder. Tip zaten kayıtlıysa uyarı verir ve yenisiyle değiştirir.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), $"ServiceLocator: '{typeof(T).Name}' için null kaydedilemez.");
            }

            Type type = typeof(T);
            if (_services.TryGetValue(type, out object existing) && !IsDestroyed(existing))
            {
                Debug.LogWarning(
                    $"ServiceLocator: '{type.Name}' zaten kayıtlı ({existing}). Yeni kayıt eskisinin yerine geçiyor ({service}).",
                    service as UnityEngine.Object);
            }

            _services[type] = service;
        }

        /// <summary>
        /// Kaydı siler; sadece kayıtlı nesne <paramref name="service"/> ise.
        /// Böylece yok edilen eski bir nesne, yerine gelen yeni kaydı silemez.
        /// </summary>
        public static void Unregister<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object existing) && ReferenceEquals(existing, service))
            {
                _services.Remove(type);
            }
        }

        /// <summary>Kayıtlı servisi döndürür; yoksa açıklayıcı bir <see cref="InvalidOperationException"/> fırlatır.</summary>
        public static T Get<T>() where T : class
        {
            if (TryGet(out T service))
            {
                return service;
            }

            throw new InvalidOperationException(
                $"ServiceLocator: '{typeof(T).Name}' kayıtlı değil. Servis Awake'te Register edilmeli, " +
                "Get ise Start'ta veya sonrasında çağrılmalı. Sahnede bu servisi sağlayan bileşen var mı?");
        }

        /// <summary>Kayıtlı servisi bulursa true döner. Yok edilmiş Unity nesneleri kayıtsız sayılır.</summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object registered))
            {
                if (!IsDestroyed(registered))
                {
                    service = (T)registered;
                    return true;
                }

                _services.Remove(type);
            }

            service = null;
            return false;
        }

        private static bool IsDestroyed(object service)
        {
            return service is UnityEngine.Object unityObject && unityObject == null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
        }

        internal static void Clear()
        {
            _services.Clear();
        }
    }
}
