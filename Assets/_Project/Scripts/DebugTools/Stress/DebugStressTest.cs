using System;
using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Enemies;
using LastHaven.Progression;
using LastHaven.Waves;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// [Systems]/DebugStress: performans stres testi (15.2; debug panelinden başlatılır). Aynı anda <see cref="EnemyTarget"/> canlı
    /// düşman (varsayılan 200) tutar: ölenlerin yerine spawn bölgelerinden yenisi doğar, kare başına en çok <c>_spawnsPerFrame</c>.
    /// Düşmanlar dalga sistemi dışında <see cref="EnemySpawner"/> ile doğar; dalganın 120 canlı tavanı bu testte geçerli değildir.
    /// Üssün çevresine <see cref="TurretTarget"/> taret (varsayılan 6) kurar (<see cref="StressTurrets"/>). Test boyunca jeneratör ve
    /// taretler hasar almaz (oyun bitmesin, yük sabit kalsın) ve öldürmeler XP vermez (seviye kartları oyunu durdurmasın). Başlarken havuzlar karışımın payına göre ısıtılır. Bitince düşmanlar
    /// öldürülür, taretler iadesiz sökülür, süre / FPS / GC özeti loglanır. Sadece editörde ve development build'de çalışır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugStressTest : MonoBehaviour
    {
        [Header("Düşmanlar")]
        [SerializeField, Tooltip("Düşmanları havuzdan doğurur. Sahne adımı bağlar.")]
        private EnemySpawner _spawner;

        [SerializeField, Tooltip("Düşmanlar bunlardan rastgele birinin dairesinde doğar. Sahne adımı bağlar.")]
        private SpawnZone[] _zones = Array.Empty<SpawnZone>();

        [SerializeField, Tooltip("Tip karışımı (sahne adımı GDD 9. gecenin oranıyla doldurur: 16 Yürüyen, 7 Koşucu, 2 Tank, 3 Tükürücü, 3 Patlayan).")]
        private StressEnemy[] _mix = Array.Empty<StressEnemy>();

        [SerializeField, Min(1), Tooltip("Aynı anda canlı tutulan düşman.")]
        private int _enemyCount = 200;

        // Sürüm build'inde sınıfın içi boş kalır ve bu ayarlar okunmaz; o derlemedeki CS0414 uyarısı bilerek kapalı.
#pragma warning disable CS0414
        [SerializeField, Min(1), Tooltip("Karede en çok doğan düşman (200'ü tek karede doğurmasın).")]
        private int _spawnsPerFrame = 10;

        [SerializeField, Min(0), Tooltip("Ön ısıtmaya eklenen düşman: ölüp yere batanlar (~3 sn) havuz örneğini tutarken yenileri doğar.")]
        private int _corpseMargin = 100;

        [SerializeField, Min(0), Tooltip("Test başında hazırlanan jeton: 6 taret hızlı öldürür, toplanmayan jetonlar 12 sn yerde kalır.")]
        private int _coinPrewarm = 800;

        [Header("Taretler")]
        [SerializeField, Tooltip("Taretlerin kurulduğu inşa alanı ([Systems]/BuildMode). Sahne adımı bağlar.")]
        private BuildSite _buildSite;

        [SerializeField, Tooltip("İnşa yuvalarındaki taret verisinin id'si.")]
        private string _turretId = "turret";

        [SerializeField, Min(0), Tooltip("Kurulacak taret sayısı.")]
        private int _turretCount = 6;

        [SerializeField, Min(0f), Tooltip("Taretlerin jeneratöre uzaklığı (m; üs alanı 14 m).")]
        private float _turretRadius = 8f;
#pragma warning restore CS0414

        [Header("Koruma")]
        [SerializeField, Tooltip("Test boyunca hasar almaz. Taretlerin merkezi de bu. Sahne adımı bağlar.")]
        private Health _generatorHealth;

        [SerializeField, Tooltip("Jeton prefab'ı buradan (havuz ön ısıtması). Sahne adımı bağlar.")]
        private EconomyConfig _economy;

        [SerializeField, Tooltip("Test boyunca kapatılır: öldürmeler XP vermez, seviye kartları oyunu durdurup yükü dondurmaz. Sahne adımı bağlar.")]
        private ExperienceService _experience;

        /// <summary>Aynı anda canlı tutulan düşman sayısı.</summary>
        public int EnemyTarget => _enemyCount;

        /// <summary>Kurulmaya çalışılan taret sayısı.</summary>
        public int TurretTarget => _turretCount;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Prefix = "[Debug] ";

        private readonly StressPopulation _population = new StressPopulation();
        private readonly StressTurrets _turrets = new StressTurrets();
        private readonly DamageImmunity _generatorImmunity = new DamageImmunity();
        private readonly EnemyCull _cull = new EnemyCull();
        private readonly GcAllocationMeter _meter = new GcAllocationMeter();
        private float _startTime;
        private int _startFrame;
        private bool _experiencePaused;

        public bool IsRunning { get; private set; }

        /// <summary>Testin doğurduğu ve hâlâ canlı düşmanlar.</summary>
        public int AliveCount => _population.AliveCount;

        public int TurretCount => _turrets.Count;

        public void Begin()
        {
            if (IsRunning)
            {
                return;
            }

            if (_spawner == null || _zones.Length == 0 || _mix.Length == 0)
            {
                Debug.LogWarning(Prefix + "Stres testi: EnemySpawner, spawn bölgesi veya karışım yok; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            _population.Begin(_mix);
            Prewarm();
            _generatorImmunity.Apply(_generatorHealth);
            _experiencePaused = _experience != null && _experience.enabled;
            if (_experiencePaused)
            {
                _experience.enabled = false;
            }

            ServiceLocator.TryGet(out IWallet wallet);
            Vector3 center = _generatorHealth != null ? _generatorHealth.transform.position : transform.position;
            _turrets.Place(_buildSite, StressTurrets.FindBuildable(_buildSite, _turretId), center, _turretCount, _turretRadius, wallet);

            IsRunning = true;
            _startTime = Time.realtimeSinceStartup;
            _startFrame = Time.frameCount;
            _meter.Start();
            Debug.Log($"{Prefix}Stres testi başladı: {_enemyCount} düşman hedefi, {_turrets.Count}/{_turretCount} taret.", this);
        }

        public void End()
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;
            int killed = _cull.KillAll(_population.Enemies, gameObject);
            _population.Clear();
            _turrets.Remove(_buildSite);
            _generatorImmunity.Remove();
            if (_experiencePaused && _experience != null)
            {
                _experience.enabled = true;
            }

            float duration = Mathf.Max(Time.unscaledDeltaTime, Time.realtimeSinceStartup - _startTime);
            AllocationWindow gc = _meter.Window;
            Debug.Log(FormattableString.Invariant(
                $"{Prefix}Stres testi bitti: {duration:0.0} sn, ort. {(Time.frameCount - _startFrame) / duration:0} FPS, en çok {_population.PeakAlive} canlı, {killed} düşman öldürüldü. GC (editör dahil): kare başı ort. {gc.AverageBytes:0} B, en çok {gc.MaxBytes} B, ayıran kare %{gc.AllocatingRatio * 100d:0.#} ({gc.AllocatingFrames}/{gc.Frames})."),
                this);
        }

        private void Update()
        {
            if (IsRunning)
            {
                _population.Tick(_spawner, _zones, _enemyCount, _spawnsPerFrame);
                _meter.Sample();
            }
        }

        private void OnDestroy()
        {
            // Sahne kapanıyor: yapılar ve düşmanlar zaten yok oluyor, sadece koruma kaldırılır.
            _turrets.Unprotect();
            _generatorImmunity.Remove();
            _meter.Dispose();
        }

        /// <summary>Havuzlar: tip başına karışım payı (ölüp batanlar için pay dahil) ve jetonlar.</summary>
        private void Prewarm()
        {
            if (!ServiceLocator.TryGet(out IPoolService pool))
            {
                return;
            }

            for (int i = 0; i < _mix.Length; i++)
            {
                EnemyData data = _mix[i].Enemy;
                if (data != null && data.Prefab != null)
                {
                    pool.Prewarm(data.Prefab.gameObject, _population.ShareOf(i, _enemyCount + _corpseMargin));
                }
            }

            if (_economy != null && _economy.CoinPrefab != null)
            {
                pool.Prewarm(_economy.CoinPrefab.gameObject, _coinPrewarm);
            }
        }
#endif
    }
}
