using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace LastHaven.Core
{
    /// <summary>
    /// Tek bir prefab'ın havuzu (<see cref="ObjectPool{T}"/> üzerinde). Örnekler kapalı "staging" kabında oluşturulur;
    /// böylece Awake/OnEnable ilk Get'e kadar çalışmaz. Etkinleştirme ve <see cref="IPoolable"/> çağrıları
    /// <see cref="PooledInstance"/>'ta; bu sınıf oluşturma, sayım ve kayıt tutar.
    /// </summary>
    internal sealed class GameObjectPool
    {
        // Havuzda bekleyen kapalı örnek sayısına sınır yok: en fazla tepe kullanım kadar örnek kalır.
        private const int UnboundedSize = int.MaxValue;

        private readonly GameObject _prefab;
        private readonly Transform _container;
        private readonly Transform _staging;
        private readonly Dictionary<GameObject, PooledInstance> _registry;
        private readonly ObjectPool<PooledInstance> _pool;
        private int _prewarmed;
        private bool _prewarming;
        private bool _growthReported;

        /// <param name="container">Örneklerin durduğu kap (etkin).</param>
        /// <param name="staging">Yeni örneklerin oluşturulduğu kapalı kap.</param>
        /// <param name="registry">Obje → kayıt sözlüğü; oluşturulan örnekler eklenir, yok edilenler çıkarılır.</param>
        public GameObjectPool(GameObject prefab, Transform container, Transform staging, Dictionary<GameObject, PooledInstance> registry)
        {
            _prefab = prefab;
            _container = container;
            _staging = staging;
            _registry = registry;
            _pool = new ObjectPool<PooledInstance>(Create, actionOnDestroy: OnDestroyInstance, collectionCheck: false, maxSize: UnboundedSize);
        }

        public PoolStats Stats => new PoolStats(_pool.CountAll, _pool.CountActive, _pool.CountInactive);

        /// <summary>Havuzdan bir örnek alır; havuz dışında yok edilmiş örnekleri atlar. Örnek henüz etkinleştirilmemiştir.</summary>
        public PooledInstance Take()
        {
            while (true)
            {
                PooledInstance instance = _pool.Get();
                if (!instance.IsDestroyed)
                {
                    return instance;
                }

                Debug.LogWarning($"PoolService: '{_prefab.name}' havuzundaki bir örnek havuz dışında yok edilmiş; atlandı. Destroy yerine Release kullan.");
                _registry.Remove(instance.GameObject);
            }
        }

        public void Release(PooledInstance instance)
        {
            instance.Despawn(_container);
            _pool.Release(instance);
        }

        /// <summary>Havuzda en az <paramref name="count"/> kapalı örnek olacak şekilde eksikleri oluşturur.</summary>
        public void Prewarm(int count)
        {
            if (count <= _pool.CountInactive)
            {
                return;
            }

            // Get önce bekleyenleri verir, bitince yenisini oluşturur; hepsi hemen geri bırakılır (etkinleşmeden).
            List<PooledInstance> batch = ListPool<PooledInstance>.Get();
            _prewarming = true;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    batch.Add(Take());
                }

                for (int i = 0; i < batch.Count; i++)
                {
                    _pool.Release(batch[i]);
                }
            }
            finally
            {
                _prewarming = false;
                ListPool<PooledInstance>.Release(batch);
            }

            _prewarmed = Mathf.Max(_prewarmed, _pool.CountAll);
        }

        private PooledInstance Create()
        {
            ReportGrowth();

            // Kapalı kapta oluşur: Awake/OnEnable çalışmaz. Kendisi kapatılıp asıl kaba taşınır.
            GameObject gameObject = Object.Instantiate(_prefab, _staging);
            gameObject.name = _prefab.name;
            gameObject.SetActive(false);
            gameObject.transform.SetParent(_container, false);

            var instance = new PooledInstance(gameObject, this);
            _registry[gameObject] = instance;
            return instance;
        }

        private void OnDestroyInstance(PooledInstance instance)
        {
            _registry.Remove(instance.GameObject);
            if (!instance.IsDestroyed)
            {
                PoolService.DestroyInstance(instance.GameObject);
            }
        }

        /// <summary>Prewarm edilen sayı aşılınca (oynanışta Instantiate) bir kez uyarır; prewarm sayısını artırmak gerekir.</summary>
        private void ReportGrowth()
        {
            if (_prewarming || _growthReported || _prewarmed == 0 || _pool.CountAll < _prewarmed)
            {
                return;
            }

            _growthReported = true;
            Debug.LogWarning(
                $"PoolService: '{_prefab.name}' havuzu prewarm sayısını ({_prewarmed}) aştı; oynanışta Instantiate yapılıyor. " +
                "Prewarm sayısını artır.");
        }
    }
}
