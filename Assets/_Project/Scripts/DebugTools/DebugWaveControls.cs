using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Gece kısayolları ([Systems]/DebugWaves): F5 sıradaki geceyi hemen başlatır (<see cref="IWaveService"/>), F6 tüm canlı
    /// düşmanları öldürür (<see cref="EnemySpawner"/>'ın doğurdukları: dalga + debug spawner). Öldürme normal hasarla yapılır;
    /// ölümler dalga sayacına, olaylara ve ödüllere her zamanki gibi gider. Sol üstte, DebugSpawner panelinin altında durum kutusu.
    /// Sadece editörde ve development build'de çalışır (sınıf ve alanları her build'de var; sahnede "missing script" olmasın).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugWaveControls : MonoBehaviour
    {
        [SerializeField, Tooltip("F6 bunun doğurduğu canlı düşmanları öldürür. Sahne adımı bağlar.")]
        private EnemySpawner _enemySpawner;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Prefix = "[Debug] ";

        // DebugSpawner panelinin (y 10–114) altında.
        private static readonly Rect PanelRect = new Rect(10f, 120f, 290f, 84f);
        private static readonly Rect PanelTextRect = new Rect(18f, 124f, 274f, 76f);

        private readonly List<Enemy> _alive = new List<Enemy>();
        private InputAction[] _actions;
        private IWaveService _waves;
        private int _nextNightIndex;
        private int _currentNightNumber;
        private string _status = "Gece yok";
        private string _panelText = string.Empty;

        private void Awake()
        {
            _actions = new[]
            {
                DebugHotkeys.Create("<Keyboard>/f5", StartNextNight),
                DebugHotkeys.Create("<Keyboard>/f6", KillAllEnemies),
            };
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _waves))
            {
                Debug.LogWarning("DebugWaveControls: IWaveService bulunamadı; F5 çalışmayacak. Sahnede [Systems]/WaveSpawner var mı?", this);
            }

            RefreshPanel();
        }

        private void OnEnable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Enable();
            }

            EventBus<NightStartedEvent>.Subscribe(OnNightStarted);
            EventBus<WaveProgressEvent>.Subscribe(OnWaveProgress);
            EventBus<NightCompletedEvent>.Subscribe(OnNightCompleted);
        }

        private void OnDisable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Disable();
            }

            EventBus<NightStartedEvent>.Unsubscribe(OnNightStarted);
            EventBus<WaveProgressEvent>.Unsubscribe(OnWaveProgress);
            EventBus<NightCompletedEvent>.Unsubscribe(OnNightCompleted);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Dispose();
            }
        }

        private void OnGUI()
        {
            GUI.Box(PanelRect, GUIContent.none);
            GUI.Label(PanelTextRect, _panelText);
        }

        private void StartNextNight()
        {
            if (_waves == null)
            {
                Debug.LogWarning(Prefix + "F5: IWaveService yok; gece başlatılamadı.", this);
                return;
            }

            if (_waves.IsNightRunning)
            {
                Debug.LogWarning($"{Prefix}F5: Gece {_currentNightNumber} hâlâ sürüyor; önce bitmeli (F6 canlıları öldürür, "
                                 + "kalan gruplar doğup ölünce gece biter).", this);
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

        private void KillAllEnemies()
        {
            if (_enemySpawner == null)
            {
                Debug.LogWarning(Prefix + "F6: EnemySpawner bağlı değil; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            // Önce kopyala: ölüm olayları dinleyicileri tetikler, liste öldürürken değişmesin.
            _enemySpawner.GetAlive(_alive);
            int killed = 0;
            for (int i = 0; i < _alive.Count; i++)
            {
                if (Kill(_alive[i]))
                {
                    killed++;
                }
            }

            _alive.Clear();
            Debug.Log($"{Prefix}F6: {killed} düşman öldürüldü.", this);
        }

        private bool Kill(Enemy enemy)
        {
            var health = enemy.GetComponent<Health>();
            if (health == null || !health.IsAlive)
            {
                return false;
            }

            // Kaynak bu obje (Player tag'i yok): EnemyKilledEvent.KilledByPlayer = false. Yön sıfır: geri itilme yok.
            health.TakeDamage(new DamageInfo(health.Current, DamageType.Bullet, gameObject, enemy.transform.position, Vector3.zero));
            return !health.IsAlive;
        }

        private void OnNightStarted(NightStartedEvent started)
        {
            _nextNightIndex = started.NightIndex + 1;
            _currentNightNumber = started.NightNumber;
            SetStatus(started.TotalEnemies, started.TotalEnemies);
        }

        private void OnWaveProgress(WaveProgressEvent progress)
        {
            SetStatus(progress.RemainingEnemies, progress.TotalEnemies);
        }

        private void OnNightCompleted(NightCompletedEvent completed)
        {
            _status = $"Gece {completed.NightNumber} tamamlandı";
            RefreshPanel();
        }

        private void SetStatus(int remaining, int total)
        {
            _status = $"Gece {_currentNightNumber} sürüyor · kalan {remaining}/{total}";
            RefreshPanel();
        }

        /// <summary>Metin sadece olaylarda kurulur (OnGUI'de string birleştirme yok).</summary>
        private void RefreshPanel()
        {
            int total = _waves != null ? _waves.TotalNights : 0;
            string next = _nextNightIndex < total ? $"Gece {_nextNightIndex + 1}/{total} başlat" : "sıradaki gece yok";
            _panelText = "Gece Kısayolları\n"
                         + "F5   " + next + "\n"
                         + "F6   Tüm canlı düşmanları öldür\n"
                         + _status;
        }
#endif
    }
}
