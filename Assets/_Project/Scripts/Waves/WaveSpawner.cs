using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// [Systems]/WaveSpawner: <see cref="IWaveService"/>. Gecenin <see cref="WaveRunner"/>'ını sürer; her spawn isteğinde ilgili
    /// yöndeki <see cref="SpawnZone"/>'dan rastgele bir nokta seçip <see cref="IEnemySpawner"/>'la düşmanı doğurur.
    /// Doğurduğu düşmanları izler; ölümleri (<see cref="DiedEvent"/>) runner'a bildirir ve dalga olaylarını gönderir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaveSpawner : MonoBehaviour, IWaveService
    {
        [SerializeField, Tooltip("Gece listesi ve canlı düşman tavanı. Üretici bağlar.")]
        private WaveConfig _config;

        [SerializeField, Tooltip("Dört yönün spawn bölgeleri ([Gameplay]/SpawnZones). Sahne adımı bağlar.")]
        private SpawnZone[] _zones = Array.Empty<SpawnZone>();

        private readonly HashSet<GameObject> _aliveEnemies = new HashSet<GameObject>();
        private readonly List<WaveGroup> _groups = new List<WaveGroup>();
        private WaveRunner _runner;
        private SpawnZoneSet _zoneSet;
        private IEnemySpawner _enemySpawner;
        private int _nightIndex = -1;
        private int _nightNumber;
        private bool _registered;

        /// <inheritdoc />
        public int TotalNights => _config != null ? _config.Nights.Count : 0;

        /// <inheritdoc />
        public bool IsNightRunning => _runner != null && _runner.IsRunning;

        /// <inheritdoc />
        public int AliveCount => _runner != null ? _runner.AliveCount : 0;

        /// <summary>Kalan düşman (bekleyen + kuyrukta + canlı); gece yoksa 0.</summary>
        public int RemainingCount => IsNightRunning ? _runner.RemainingCount : 0;

        /// <summary>Son başlatılan gecenin 0 tabanlı indeksi; hiç başlamadıysa -1.</summary>
        public int CurrentNightIndex => _nightIndex;

        /// <inheritdoc />
        public bool StartNight(int nightIndex)
        {
            if (_runner == null)
            {
                Debug.LogWarning("[Waves] WaveSpawner hazır değil (config yok); gece başlatılamadı.", this);
                return false;
            }

            if (_runner.IsRunning)
            {
                Debug.LogWarning($"[Waves] Gece {_nightNumber} hâlâ sürüyor; {nightIndex + 1}. gece başlatılmadı.", this);
                return false;
            }

            if (nightIndex < 0 || nightIndex >= TotalNights)
            {
                Debug.LogError($"[Waves] Geçersiz gece indeksi {nightIndex} (toplam {TotalNights}).", this);
                return false;
            }

            NightData night = _config.Nights[nightIndex];
            if (night == null)
            {
                Debug.LogError($"[Waves] WaveConfig'in {nightIndex}. gecesi boş.", _config);
                return false;
            }

            _groups.Clear();
            WavePlan.BuildGroups(night, _groups);
            _aliveEnemies.Clear();
            _nightIndex = nightIndex;
            _nightNumber = night.NightNumber;
            _runner.Start(_groups, _config.MaxAliveEnemies);

            Debug.Log($"[Waves] NightStartedEvent: Gece {_nightNumber} başladı ({_runner.TotalCount} düşman, {_groups.Count} grup).", this);
            EventBus<NightStartedEvent>.Raise(new NightStartedEvent(_nightIndex, _nightNumber, _runner.TotalCount));
            RaiseProgress();
            return true;
        }

        /// <inheritdoc />
        public NightInfo GetTonightInfo(int nightIndex)
        {
            if (nightIndex < 0 || nightIndex >= TotalNights)
            {
                throw new ArgumentOutOfRangeException(nameof(nightIndex), nightIndex, $"Gece indeksi 0–{TotalNights - 1} aralığında olmalı.");
            }

            NightData night = _config.Nights[nightIndex];
            return night != null
                ? WavePlan.BuildInfo(nightIndex, night)
                : new NightInfo(nightIndex, nightIndex + 1, null, null, null);
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("WaveSpawner: WaveConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _zoneSet = new SpawnZoneSet(_zones);
            _runner = new WaveRunner(TrySpawn);
            _runner.RemainingChanged += RaiseProgress;
            _runner.Completed += OnNightCompleted;

            ServiceLocator.Register<IWaveService>(this);
            _registered = true;
        }

        private void OnEnable()
        {
            EventBus<DiedEvent>.Subscribe(OnDied);
        }

        private void OnDisable()
        {
            EventBus<DiedEvent>.Unsubscribe(OnDied);
        }

        private void OnDestroy()
        {
            if (_registered)
            {
                ServiceLocator.Unregister<IWaveService>(this);
            }
        }

        private void Update()
        {
            if (_runner.IsRunning)
            {
                _runner.Tick(Time.deltaTime);
            }
        }

        private bool TrySpawn(WaveSpawnRequest request)
        {
            // Servis tembel alınır: gece, bu bileşenin Start'ından önce başlatılsa da çalışsın.
            if (_enemySpawner == null && !ServiceLocator.TryGet(out _enemySpawner))
            {
                Debug.LogWarning("[Waves] IEnemySpawner bulunamadı; düşman doğurulamadı. Sahnede [Systems]/EnemySpawner var mı?", this);
                return false;
            }

            if (!_zoneSet.TryGet(request.Direction, out SpawnZone zone))
            {
                Debug.LogWarning($"[Waves] {request.Direction} yönünde SpawnZone yok; '{request.Enemy.Id}' doğurulamadı.", this);
                return false;
            }

            GameObject enemy = _enemySpawner.Spawn(request.Enemy.Id, zone.GetRandomPoint());
            if (enemy == null)
            {
                return false; // EnemySpawner nedenini logladı.
            }

            _aliveEnemies.Add(enemy);
            return true;
        }

        private void OnDied(DiedEvent died)
        {
            // Sadece bu gecenin doğurduğu düşmanlar sayılır (debug spawner'ınkiler, oyuncu, yapılar değil).
            if (died.Target != null && _aliveEnemies.Remove(died.Target))
            {
                _runner.ReportDeath();
            }
        }

        private void RaiseProgress()
        {
            EventBus<WaveProgressEvent>.Raise(new WaveProgressEvent(_nightIndex, _runner.RemainingCount, _runner.TotalCount));
        }

        private void OnNightCompleted()
        {
            _aliveEnemies.Clear();
            string dropped = _runner.DroppedCount > 0 ? $", {_runner.DroppedCount} düşman doğurulamadı" : string.Empty;
            Debug.Log($"[Waves] NightCompletedEvent: Gece {_nightNumber} tamamlandı{dropped}.", this);
            EventBus<NightCompletedEvent>.Raise(new NightCompletedEvent(_nightIndex, _nightNumber));
        }
    }
}
