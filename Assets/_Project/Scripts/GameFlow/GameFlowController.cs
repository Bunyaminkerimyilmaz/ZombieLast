using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// [Systems]/GameFlow: <see cref="IGameStateService"/>. <see cref="GameStateMachine"/>'i sürer ve Unity'ye bağlar:
    /// gece evresinde <see cref="IWaveService.StartNight"/>, <see cref="NightCompletedEvent"/> → şafak,
    /// <see cref="GeneratorDestroyedEvent"/> → oyun bitti. Makinenin çıktılarını global olaylara çevirir ve loglar.
    /// "Hazırım" (Enter) gündüzü erken bitirir; şafakta da şafak panelinin "Devam" butonuyla aynı devam isteğini gönderir.
    /// <see cref="GameConfig"/>'i <see cref="IGameRules"/> olarak kaydeder. Akış ilk Update'te başlar (tüm Start'lar bitmiş olur).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameFlowController : MonoBehaviour, IGameStateService
    {
        private const string LogPrefix = "[GameFlow] ";

        [SerializeField, Tooltip("Gündüz/alacakaranlık süreleri, erken hazır bonusu, yeniden doğma. Üretici bağlar.")]
        private GameConfig _config;

        private GameStateMachine _machine;
        private IWaveService _waves;
        private IInputReader _input;
        private bool _registered;
        private bool _started;

        /// <inheritdoc />
        public GamePhase CurrentPhase => _machine != null ? _machine.Phase : GamePhase.Boot;

        /// <inheritdoc />
        public int NightIndex => _machine != null ? _machine.NightIndex : 0;

        /// <inheritdoc />
        public float PhaseTimeRemaining => _machine != null ? _machine.PhaseTimeRemaining : 0f;

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("GameFlowController: GameConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            ServiceLocator.Register<IGameStateService>(this);
            ServiceLocator.Register<IGameRules>(_config);
            _registered = true;
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _waves) || _waves.TotalNights < 1)
            {
                Debug.LogError("GameFlowController: IWaveService yok veya gece listesi boş; oyun akışı başlamayacak. "
                               + "Sahnede [Systems]/WaveSpawner var mı? Tools/Last Haven/Validate Project çalıştır.", this);
                enabled = false;
                return;
            }

            if (!ServiceLocator.TryGet(out _input))
            {
                Debug.LogWarning("GameFlowController: IInputReader bulunamadı; Enter ile hazır/devam çalışmayacak. Sahnede [Systems]/Input var mı?", this);
            }

            _machine = new GameStateMachine(_config.CreateFlowSettings(), _waves.TotalNights, StartNight);
            _machine.PhaseChanged += OnPhaseChanged;
            _machine.DayTimerTicked += OnDayTimerTicked;
            _machine.DayEndingSoon += OnDayEndingSoon;
            _machine.EarlyReady += OnEarlyReady;

            _started = true;
            SubscribeInput();
        }

        private void OnEnable()
        {
            EventBus<NightCompletedEvent>.Subscribe(OnNightCompleted);
            EventBus<GeneratorDestroyedEvent>.Subscribe(OnGeneratorDestroyed);
            EventBus<DawnContinueRequestedEvent>.Subscribe(OnDawnContinueRequested);

            // Servis Start'ta alınır; ilk aboneliği Start yapar.
            if (_started)
            {
                SubscribeInput();
            }
        }

        private void OnDisable()
        {
            EventBus<NightCompletedEvent>.Unsubscribe(OnNightCompleted);
            EventBus<GeneratorDestroyedEvent>.Unsubscribe(OnGeneratorDestroyed);
            EventBus<DawnContinueRequestedEvent>.Unsubscribe(OnDawnContinueRequested);

            if (_input != null)
            {
                _input.ReadyPressed -= OnReadyPressed;
            }
        }

        private void OnDestroy()
        {
            if (_registered)
            {
                ServiceLocator.Unregister<IGameStateService>(this);
                ServiceLocator.Unregister<IGameRules>(_config);
            }
        }

        private void Update()
        {
            if (_machine.Phase == GamePhase.Boot)
            {
                _machine.Begin();
                return;
            }

            _machine.Tick(Time.deltaTime);
        }

        private void SubscribeInput()
        {
            if (_input != null)
            {
                _input.ReadyPressed += OnReadyPressed;
            }
        }

        private void StartNight(int nightIndex)
        {
            if (!_waves.StartNight(nightIndex))
            {
                Debug.LogError($"{LogPrefix}Gece {nightIndex + 1} başlatılamadı; akış gecede bekliyor. "
                               + "Tools/Last Haven/Validate Project ile gece verisini kontrol et.", this);
            }
        }

        private void OnReadyPressed()
        {
            if (_machine == null)
            {
                return;
            }

            if (_machine.Phase == GamePhase.Day)
            {
                _machine.RequestReady();
            }
            else if (_machine.Phase == GamePhase.Dawn)
            {
                // Klavye/gamepad kısayolu: şafak panelinin "Devam" butonu da aynı olayı gönderir.
                EventBus<DawnContinueRequestedEvent>.Raise(new DawnContinueRequestedEvent());
            }
        }

        private void OnNightCompleted(NightCompletedEvent completed)
        {
            _machine?.NotifyNightCompleted();
        }

        private void OnGeneratorDestroyed(GeneratorDestroyedEvent destroyed)
        {
            _machine?.NotifyGeneratorDestroyed();
        }

        private void OnDawnContinueRequested(DawnContinueRequestedEvent request)
        {
            _machine?.RequestDawnContinue();
        }

        private void OnPhaseChanged()
        {
            GamePhase phase = _machine.Phase;
            int night = _machine.NightIndex;
            Debug.Log($"{LogPrefix}PhaseChangedEvent: {phase} — {Describe(phase, night)}", this);
            EventBus<PhaseChangedEvent>.Raise(new PhaseChangedEvent(phase, night, _machine.PhaseDuration));

            switch (phase)
            {
                case GamePhase.Dawn:
                    EventBus<DawnEvent>.Raise(new DawnEvent(night));
                    break;
                case GamePhase.GameOver:
                    EventBus<GameOverEvent>.Raise(new GameOverEvent(night));
                    break;
                case GamePhase.Victory:
                    EventBus<VictoryEvent>.Raise(new VictoryEvent(night));
                    break;
            }
        }

        private void OnDayTimerTicked(int remainingSeconds)
        {
            EventBus<DayTimerTickEvent>.Raise(new DayTimerTickEvent(remainingSeconds));
        }

        private void OnDayEndingSoon(float remainingSeconds)
        {
            Debug.Log($"{LogPrefix}DayEndingSoonEvent: gündüzün son {Mathf.CeilToInt(remainingSeconds)} sn'si.", this);
            EventBus<DayEndingSoonEvent>.Raise(new DayEndingSoonEvent(remainingSeconds));
        }

        private void OnEarlyReady(float remainingSeconds, int bonusMoney)
        {
            Debug.Log($"{LogPrefix}EarlyReadyEvent: gündüz {Mathf.CeilToInt(remainingSeconds)} sn erken bitti, bonus +{bonusMoney} para.", this);
            EventBus<EarlyReadyEvent>.Raise(new EarlyReadyEvent(remainingSeconds, bonusMoney));
        }

        /// <summary>Log açıklaması (sadece evre değişiminde kurulur).</summary>
        private string Describe(GamePhase phase, int night)
        {
            string nightLabel = $"Gece {night + 1}/{_machine.TotalNights}";
            switch (phase)
            {
                case GamePhase.Day:
                    return $"{nightLabel} öncesi gündüz, {_machine.PhaseDuration:0} sn. Enter = Hazırım.";
                case GamePhase.Dusk:
                    return $"alacakaranlık, {_machine.PhaseDuration:0} sn sonra {nightLabel}.";
                case GamePhase.Night:
                    return $"{nightLabel} başlıyor.";
                case GamePhase.Dawn:
                    return $"{nightLabel} atlatıldı. Devam için Enter.";
                case GamePhase.GameOver:
                    return $"jeneratör yıkıldı, oyun bitti ({nightLabel}).";
                case GamePhase.Victory:
                    return $"{nightLabel} atlatıldı, tahliye! Zafer.";
                default:
                    return nightLabel;
            }
        }
    }
}
