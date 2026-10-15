using LastHaven.Core;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// [Systems]/EnemySpawner: <see cref="IEnemySpawner"/>. Düşmanı <see cref="IPoolService"/>'ten çeker ve
    /// <see cref="NavMesh.SamplePosition(Vector3, out NavMeshHit, float, int)"/> ile bulunan en yakın NavMesh noktasına, rastgele bir
    /// yöne bakacak şekilde koyar. <see cref="EnemySpawnerConfig"/>'teki tipler oyun başında havuzda ön ısıtılır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour, IEnemySpawner
    {
        private const float FullTurnDegrees = 360f;

        [SerializeField, Tooltip("Doğurulabilecek tipler, havuz sayıları ve NavMesh'e yerleştirme mesafesi. Üretici bağlar.")]
        private EnemySpawnerConfig _config;

        private readonly EnemyTypeLookup _types = new EnemyTypeLookup();
        private IPoolService _pool;
        private bool _registered;

        /// <inheritdoc />
        public GameObject Spawn(string enemyId, Vector3 position)
        {
            if (!_types.TryGet(enemyId, out EnemyData data))
            {
                Debug.LogWarning($"EnemySpawner: '{enemyId}' id'li düşman tipi yok. {nameof(EnemySpawnerConfig)} listesinde mi?", this);
                return null;
            }

            Enemy enemy = Spawn(data, position);
            return enemy != null ? enemy.gameObject : null;
        }

        /// <summary>
        /// <paramref name="data"/> tipinden bir düşmanı havuzdan çekip <paramref name="position"/>'a en yakın NavMesh noktasına koyar.
        /// Config listesinde olmayan tip de doğurulabilir (ön ısıtılmamış olur). Başarısızsa uyarı loglar ve null döner.
        /// </summary>
        public Enemy Spawn(EnemyData data, Vector3 position)
        {
            if (data == null || data.Prefab == null)
            {
                Debug.LogWarning($"EnemySpawner: '{(data != null ? data.Id : "null")}' için prefab yok; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                return null;
            }

            if (!TryResolvePool())
            {
                Debug.LogWarning("EnemySpawner: IPoolService bulunamadı; düşman doğurulamadı. Sahnede [Systems]/Pool var mı?", this);
                return null;
            }

            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, _config.NavMeshSampleDistance, NavMesh.AllAreas))
            {
                Debug.LogWarning($"EnemySpawner: {position} yakınında ({_config.NavMeshSampleDistance} m) NavMesh yok; '{data.Id}' doğurulamadı.", this);
                return null;
            }

            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, FullTurnDegrees), 0f);
            return _pool.Get(data.Prefab, hit.position, rotation);
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("EnemySpawner: EnemySpawnerConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            BuildLookup();
            ServiceLocator.Register<IEnemySpawner>(this);
            _registered = true;
        }

        private void Start()
        {
            if (!TryResolvePool())
            {
                Debug.LogWarning("EnemySpawner: IPoolService bulunamadı; düşmanlar ön ısıtılmadı.", this);
                return;
            }

            Prewarm();
        }

        private void OnDestroy()
        {
            if (_registered)
            {
                ServiceLocator.Unregister<IEnemySpawner>(this);
            }
        }

        private bool TryResolvePool()
        {
            // Başka bir bileşen Start'ta (bu bileşenin Start'ından önce) Spawn çağırsa da çalışsın.
            return _pool != null || ServiceLocator.TryGet(out _pool);
        }

        private void BuildLookup()
        {
            for (int i = 0; i < _config.Enemies.Count; i++)
            {
                EnemyData data = _config.Enemies[i].Data;
                EnemyTypeAddResult result = _types.TryAdd(data);
                switch (result)
                {
                    case EnemyTypeAddResult.MissingData:
                        Debug.LogWarning($"EnemySpawner: config'in {i}. satırında EnemyData boş; atlandı.", _config);
                        break;
                    case EnemyTypeAddResult.EmptyId:
                        Debug.LogWarning($"EnemySpawner: '{data.name}' verisinin id'si boş; atlandı.", data);
                        break;
                    case EnemyTypeAddResult.DuplicateId:
                        Debug.LogWarning($"EnemySpawner: '{data.Id}' id'si config'te birden fazla; '{data.name}' atlandı.", data);
                        break;
                }
            }
        }

        private void Prewarm()
        {
            for (int i = 0; i < _config.Enemies.Count; i++)
            {
                EnemyPoolEntry entry = _config.Enemies[i];
                if (entry.Data == null)
                {
                    continue;
                }

                if (entry.Data.Prefab == null)
                {
                    Debug.LogWarning($"EnemySpawner: '{entry.Data.Id}' için prefab yok; ön ısıtılmadı.", entry.Data);
                    continue;
                }

                _pool.Prewarm(entry.Data.Prefab.gameObject, entry.Prewarm);
            }
        }
    }
}
