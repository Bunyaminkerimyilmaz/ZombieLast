using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="IPoolService"/> ([Systems]/Pool). Her prefab için bu objenin altında prefab adıyla bir kap açılır;
    /// örnekler etkinken de kapalıyken de orada durur. Sahne kapanınca havuzlar da yok olur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoolService : MonoBehaviour, IPoolService
    {
        private const string StagingName = "_Staging";

        private readonly Dictionary<GameObject, GameObjectPool> _pools = new Dictionary<GameObject, GameObjectPool>();
        private readonly Dictionary<GameObject, PooledInstance> _instances = new Dictionary<GameObject, PooledInstance>();
        private Transform _staging;

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, position, rotation)?.GameObject;
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            PooledInstance instance = Spawn(prefab != null ? prefab.gameObject : null, position, rotation);
            return instance?.GetComponentCached<T>();
        }

        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!_instances.TryGetValue(instance, out PooledInstance pooled))
            {
                Debug.LogWarning($"PoolService: '{instance.name}' havuzdan gelmedi; yok ediliyor.", instance);
                DestroyInstance(instance);
                return;
            }

            if (!pooled.IsSpawned)
            {
                Debug.LogWarning($"PoolService: '{instance.name}' zaten havuzda; ikinci Release yok sayıldı.", instance);
                return;
            }

            pooled.Pool.Release(pooled);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null)
            {
                Debug.LogError("PoolService: Prewarm için prefab null.", this);
                return;
            }

            GetPool(prefab).Prewarm(count);
        }

        public PoolStats GetStats(GameObject prefab)
        {
            return prefab != null && _pools.TryGetValue(prefab, out GameObjectPool pool) ? pool.Stats : default;
        }

        /// <summary>Play Mode'da Destroy, editörde (EditMode testleri) DestroyImmediate.</summary>
        internal static void DestroyInstance(GameObject gameObject)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private void Awake()
        {
            ServiceLocator.Register<IPoolService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPoolService>(this);

            // Örnekler bu objenin çocukları; onlarla birlikte yok olurlar.
            _pools.Clear();
            _instances.Clear();
        }

        private PooledInstance Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError("PoolService: Get için prefab null.", this);
                return null;
            }

            PooledInstance instance = GetPool(prefab).Take();
            instance.Spawn(position, rotation);
            return instance;
        }

        private GameObjectPool GetPool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out GameObjectPool pool))
            {
                return pool;
            }

            var container = new GameObject(prefab.name).transform;
            container.SetParent(transform, false);
            pool = new GameObjectPool(prefab, container, GetStaging(), _instances);
            _pools.Add(prefab, pool);
            return pool;
        }

        private Transform GetStaging()
        {
            if (_staging == null)
            {
                var staging = new GameObject(StagingName);
                staging.SetActive(false);
                _staging = staging.transform;
                _staging.SetParent(transform, false);
            }

            return _staging;
        }
    }
}
