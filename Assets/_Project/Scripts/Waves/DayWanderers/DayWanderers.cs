using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace LastHaven.Waves
{
    /// <summary>
    /// [Systems]/DayWanderers: gündüz tehlikesi (GDD §9). Her gündüz başında <see cref="DayWanderersConfig"/>'teki sayıda (3–6)
    /// Yürüyen'i üs çevresindeki dış halkada (oyuncudan uzakta, NavMesh üstünde) doğurur ve gezgin moduna alır
    /// (<see cref="Enemy.BeginWander"/>): üsse gitmezler, halkada dolaşırlar, oyuncu aggro yarıçapına girerse saldırırlar.
    /// Gece başlayınca (<see cref="NightStartedEvent"/>) canlı gezginler normal düşmana döner (<see cref="Enemy.EndWander"/>) ve
    /// geceye katılır (<see cref="WaveSpawner.Adopt"/>): kalan düşmana sayılır, gece onlar ölmeden bitmez. Düşmanlar
    /// <see cref="IEnemySpawner"/>'dan (havuz) gelir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DayWanderers : MonoBehaviour
    {
        private const string LogPrefix = "[Waves] ";

        private static readonly Func<float> UnityRandom = () => Random.value;

        [SerializeField, Tooltip("Sayı, tip, halka ve dolaşma ayarları. Üretici bağlar.")]
        private DayWanderersConfig _config;

        [SerializeField, Tooltip("Gece başlayınca gezginleri geceye katan dalga sistemi ([Systems]/WaveSpawner). Sahne adımı bağlar.")]
        private WaveSpawner _waves;

        private readonly List<Enemy> _wanderers = new List<Enemy>();
        private IEnemySpawner _spawner;
        private IBaseArea _baseArea;
        private IPlayer _player;

        /// <summary>Şu an canlı ve gezgin modundaki düşman sayısı (debug ve testler için).</summary>
        public int WandererCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _wanderers.Count; i++)
                {
                    if (IsWandering(_wanderers[i]))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>Bu gündüzün gezginlerini doğurur (gündüz başı; debug için de). Doğan sayıyı döndürür.</summary>
        public int SpawnWanderers()
        {
            _wanderers.RemoveAll(enemy => !IsWandering(enemy));
            if (_spawner == null)
            {
                return 0;
            }

            WanderSettings area = _config.CreateWanderSettings(_baseArea != null ? _baseArea.Center : Vector3.zero);
            int count = _config.PickCount(Random.value);
            int spawned = 0;
            for (int i = 0; i < count; i++)
            {
                if (!TryFindSpawnPoint(area, out Vector3 point))
                {
                    Debug.LogWarning($"{LogPrefix}Gündüz gezginleri: {i + 1}. gezgine yer bulunamadı ({_config.SpawnAttempts} deneme).", this);
                    break;
                }

                GameObject spawnedObject = _spawner.Spawn(_config.Enemy.Id, point);
                if (spawnedObject == null || !spawnedObject.TryGetComponent(out Enemy enemy))
                {
                    continue; // EnemySpawner nedenini logladı.
                }

                enemy.BeginWander(area);
                _wanderers.Add(enemy);
                spawned++;
            }

            Debug.Log($"{LogPrefix}Gündüz gezginleri: {spawned} {_config.Enemy.DisplayName} dış halkada " +
                      $"({area.MinRadius:0}–{area.MaxRadius:0} m) dolaşıyor.", this);
            return spawned;
        }

        private void Awake()
        {
            if (_config == null || _config.Enemy == null)
            {
                Debug.LogError("DayWanderers: DayWanderersConfig veya gezgin tipi atanmamış; Tools/Last Haven/Rebuild All çalıştır.", this);
                enabled = false;
                return;
            }

            if (_waves == null)
            {
                Debug.LogWarning("DayWanderers: WaveSpawner atanmamış; gezginler gece normal düşmana döner ama gecenin sayacına katılmaz.", this);
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _spawner))
            {
                Debug.LogWarning("DayWanderers: IEnemySpawner bulunamadı; gezgin çıkmayacak. Sahnede [Systems]/EnemySpawner var mı?", this);
            }

            if (!ServiceLocator.TryGet(out _baseArea))
            {
                Debug.LogWarning("DayWanderers: IBaseArea bulunamadı; halka dünya merkezine göre.", this);
            }

            ServiceLocator.TryGet(out _player);
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<NightStartedEvent>.Subscribe(OnNightStarted);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<NightStartedEvent>.Unsubscribe(OnNightStarted);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (changed.Phase == GamePhase.Day)
            {
                SpawnWanderers();
            }
        }

        private void OnNightStarted(NightStartedEvent started)
        {
            int released = 0;
            int adopted = 0;
            for (int i = 0; i < _wanderers.Count; i++)
            {
                Enemy enemy = _wanderers[i];
                if (!IsWandering(enemy))
                {
                    continue;
                }

                enemy.EndWander();
                released++;
                if (_waves != null && _waves.Adopt(enemy.gameObject))
                {
                    adopted++;
                }
            }

            _wanderers.Clear();
            if (released > 0)
            {
                Debug.Log($"{LogPrefix}Gece {started.NightNumber}: {released} gezgin normal düşmana döndü ({adopted} tanesi gecenin sayacında).", this);
            }
        }

        /// <summary>Halkada, oyuncudan uzak ve NavMesh üstünde (halkada kalan) bir nokta.</summary>
        private bool TryFindSpawnPoint(in WanderSettings area, out Vector3 point)
        {
            for (int attempt = 0; attempt < _config.SpawnAttempts; attempt++)
            {
                Vector3 candidate = area.SampleRing(UnityRandom);
                if (_player != null && PlanarDistance.Between(_player.Position, candidate) < _config.MinPlayerDistance)
                {
                    continue;
                }

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, area.NavMeshSampleDistance, NavMesh.AllAreas)
                    && area.Contains(hit.position))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = Vector3.zero;
            return false;
        }

        private static bool IsWandering(Enemy enemy)
        {
            return enemy != null && enemy.IsAlive && enemy.IsWanderer;
        }
    }
}
