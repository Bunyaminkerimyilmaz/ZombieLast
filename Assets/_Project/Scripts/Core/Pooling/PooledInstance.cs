using System;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Havuzdaki tek bir örneğin kaydı: obje, sahibi olan havuz, <see cref="IPoolable"/> bileşenleri ve durum.</summary>
    internal sealed class PooledInstance
    {
        private readonly IPoolable[] _poolables;
        private Component _cachedComponent;

        public PooledInstance(GameObject gameObject, GameObjectPool pool)
        {
            GameObject = gameObject;
            Transform = gameObject.transform;
            Pool = pool;
            _poolables = gameObject.GetComponentsInChildren<IPoolable>(true);
        }

        public GameObject GameObject { get; }

        public Transform Transform { get; }

        public GameObjectPool Pool { get; }

        /// <summary>Havuzdan alınmış ve henüz iade edilmemiş.</summary>
        public bool IsSpawned { get; private set; }

        /// <summary>Obje havuz dışında yok edilmiş (ör. biri Destroy çağırdı).</summary>
        public bool IsDestroyed => GameObject == null;

        /// <summary>Kökteki <typeparamref name="T"/> bileşeni; ilk istekte bulunur, sonra önbellekten döner.</summary>
        public T GetComponentCached<T>() where T : Component
        {
            if (_cachedComponent is T cached)
            {
                return cached;
            }

            var component = GameObject.GetComponent<T>();
            _cachedComponent = component;
            return component;
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            Transform.SetPositionAndRotation(position, rotation);
            GameObject.SetActive(true);
            IsSpawned = true;
            for (int i = 0; i < _poolables.Length; i++)
            {
                try
                {
                    _poolables[i].OnSpawned();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, GameObject);
                }
            }
        }

        /// <summary>Örneği kapatır ve (başka bir yere taşındıysa) havuzun kabına geri koyar.</summary>
        public void Despawn(Transform container)
        {
            IsSpawned = false;
            for (int i = 0; i < _poolables.Length; i++)
            {
                try
                {
                    _poolables[i].OnDespawned();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, GameObject);
                }
            }

            GameObject.SetActive(false);
            if (Transform.parent != container)
            {
                Transform.SetParent(container, false);
            }
        }
    }
}
