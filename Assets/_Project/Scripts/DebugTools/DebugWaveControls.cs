using LastHaven.Core;
using LastHaven.Enemies;
using LastHaven.GameFlow;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Gece kısayolları ([Systems]/DebugWaves; debug panelinin "Gece" bölümü de bunları çağırır):
    /// <list type="bullet">
    /// <item>F5 — gece sürüyorsa <b>geceyi atla</b>: doğmamış düşmanlar iptal (<see cref="IWaveService.CancelPendingSpawns"/>), canlılar
    /// öldürülür; gece normal yoldan biter, şafak gelir. Gece yoksa ve oyun akışı yoksa sıradaki geceyi başlatır (akış varken
    /// geceyi akış başlatır: Enter = Hazırım).</item>
    /// <item>F6 — <see cref="EnemySpawner"/>'ın doğurduğu tüm canlı düşmanları öldürür (dalga + gezgin + debug spawner).</item>
    /// <item><see cref="GoToNight"/> — düşmanları öldürüp <see cref="GameFlowController.JumpToNight"/> ile N. gecenin gündüzüne atlar.</item>
    /// </list>
    /// Öldürme normal hasarla yapılır (<see cref="EnemyCull"/>); ölümler olaylara ve ödüllere her zamanki gibi gider. Sadece editörde
    /// ve development build'de çalışır (sınıf ve alanları her build'de var; sahnede "missing script" olmasın).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugWaveControls : MonoBehaviour
    {
        [SerializeField, Tooltip("F6 bunun doğurduğu canlı düşmanları öldürür. Sahne adımı bağlar.")]
        private EnemySpawner _enemySpawner;

        [SerializeField, Tooltip("Oyun akışı ([Systems]/GameFlow; opsiyonel). Varsa \"N. geceye git\" çalışır ve F5 gündüz geceyi başlatmaz. "
                                 + "Sahne adımı bağlar.")]
        private GameFlowController _gameFlow;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Prefix = "[Debug] ";

        private readonly EnemyCull _cull = new EnemyCull();
        private InputAction[] _actions;
        private IWaveService _waves;
        private int _nextNightIndex;

        /// <summary>Toplam gece; dalga servisi yoksa 0.</summary>
        public int TotalNights => _waves != null ? _waves.TotalNights : 0;

        /// <summary>Canlı düşman sayısı (<see cref="EnemyCull.CountAlive"/>).</summary>
        public int CountAliveEnemies()
        {
            return _cull.CountAlive(_enemySpawner);
        }

        /// <summary>F6: tüm canlı düşmanları öldürür; ölen sayısını döndürür.</summary>
        public int KillAllEnemies()
        {
            if (_enemySpawner == null)
            {
                Debug.LogWarning(Prefix + "F6: EnemySpawner bağlı değil; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return 0;
            }

            int killed = _cull.KillAll(_enemySpawner, gameObject);
            Debug.Log($"{Prefix}F6: {killed} düşman öldürüldü.", this);
            return killed;
        }

        /// <summary>F5 (gece sürerken): doğmamışları iptal eder, canlıları öldürür; gece bir sonraki karede biter.</summary>
        public void SkipNight()
        {
            if (_waves == null || !_waves.IsNightRunning)
            {
                Debug.LogWarning(Prefix + "Geceyi atla: sürmekte olan gece yok (gündüzü Enter ile bitir veya \"N. geceye git\").", this);
                return;
            }

            int cancelled = _waves.CancelPendingSpawns();
            int killed = KillAllEnemies();
            Debug.Log($"{Prefix}Geceyi atla: {cancelled} düşman iptal edildi, {killed} düşman öldürüldü; gece bitince şafak gelir.", this);
        }

        /// <summary>
        /// <paramref name="nightNumber"/>. (1 tabanlı) gecenin gündüzüne atlar: önce düşmanlar öldürülür, sonra akış sürmekte olan
        /// geceyi durdurup gündüze geçer (sandıklar ve gezginler yeniden çıkar). Akış yoksa, oyun bittiyse veya numara geçersizse false.
        /// </summary>
        public bool GoToNight(int nightNumber)
        {
            if (_gameFlow == null || !_gameFlow.isActiveAndEnabled)
            {
                Debug.LogWarning(Prefix + "N. geceye git: oyun akışı yok ([Systems]/GameFlow).", this);
                return false;
            }

            GamePhase phase = _gameFlow.CurrentPhase;
            if (phase == GamePhase.Boot || phase == GamePhase.GameOver || phase == GamePhase.Victory
                || nightNumber < 1 || nightNumber > TotalNights)
            {
                Debug.LogWarning($"{Prefix}N. geceye git: {nightNumber}. geceye atlanamaz (evre {phase}, toplam {TotalNights}).", this);
                return false;
            }

            // Önce öldür: gece hâlâ sürüyorsa ölümler sayılır ama bitiş sadece dalganın Update'inde kontrol edilir; hemen ardından
            // akış geceyi durdurduğu için şafak gelmez. Sonra öldürmek yeni gündüzün gezginlerini de öldürürdü.
            int killed = _cull.KillAll(_enemySpawner, gameObject);
            if (!_gameFlow.JumpToNight(nightNumber - 1))
            {
                return false;
            }

            Debug.Log($"{Prefix}{nightNumber}. gecenin gündüzüne atlandı ({killed} düşman öldürüldü).", this);
            return true;
        }

        private void Awake()
        {
            _actions = new[]
            {
                DebugHotkeys.Create("<Keyboard>/f5", OnF5),
                DebugHotkeys.Create("<Keyboard>/f6", () => KillAllEnemies()),
            };
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _waves))
            {
                Debug.LogWarning("DebugWaveControls: IWaveService bulunamadı; F5 çalışmayacak. Sahnede [Systems]/WaveSpawner var mı?", this);
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Enable();
            }

            EventBus<NightStartedEvent>.Subscribe(OnNightStarted);
        }

        private void OnDisable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Disable();
            }

            EventBus<NightStartedEvent>.Unsubscribe(OnNightStarted);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Dispose();
            }
        }

        private void OnF5()
        {
            if (_waves != null && _waves.IsNightRunning)
            {
                SkipNight();
            }
            else if (_gameFlow != null)
            {
                // Akış dışında başlatılan gece evreleri karıştırır (gündüz ortasında gece, aynı gecenin tekrarı).
                Debug.LogWarning(Prefix + "F5: Gece sürmüyor; geceyi oyun akışı başlatır (Enter = Hazırım) ya da panelden \"N. geceye git\".", this);
            }
            else
            {
                StartNextNight();
            }
        }

        /// <summary>Akışsız sahnelerde (testler) sıradaki geceyi başlatır.</summary>
        private void StartNextNight()
        {
            if (_waves == null)
            {
                Debug.LogWarning(Prefix + "F5: IWaveService yok; gece başlatılamadı.", this);
                return;
            }

            if (_nextNightIndex >= _waves.TotalNights)
            {
                Debug.LogWarning($"{Prefix}F5: Sıradaki gece yok ({_waves.TotalNights} gecenin hepsi oynandı).", this);
                return;
            }

            Debug.Log($"{Prefix}F5: {_nextNightIndex + 1}. gece başlatılıyor.", this);
            _waves.StartNight(_nextNightIndex);
        }

        private void OnNightStarted(NightStartedEvent started)
        {
            _nextNightIndex = started.NightIndex + 1;
        }
#endif
    }
}
