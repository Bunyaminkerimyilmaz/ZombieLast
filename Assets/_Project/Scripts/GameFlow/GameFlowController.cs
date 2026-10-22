using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// [Systems]/GameFlow: <see cref="IGameStateService"/>. <see cref="GameStateMachine"/>'i sürer ve Unity'ye bağlar:
    /// gece evresinde <see cref="IWaveService.StartNight"/>, <see cref="NightCompletedEvent"/> → şafak,
    /// <see cref="GeneratorDestroyedEvent"/> → oyun bitti. Makinenin çıktılarını <see cref="GameFlowAnnouncer"/> global olaylara çevirir.
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
            GameFlowAnnouncer.Attach(_machine, this);

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

        /// <summary>
        /// Debug ("N. geceye git"): sürmekte olan geceyi bitmiş saymadan durdurur (<see cref="IWaveService.AbortNight"/>) ve
        /// <paramref name="nightIndex"/>. gecenin (0 tabanlı) gündüzüne atlar. Sahnedeki düşmanları temizlemek çağıranın işi; gündüz
        /// başı olayları (sandıklar, gezginler) her zamanki gibi çalışır. Akış başlamadıysa, oyun bittiyse veya indeks geçersizse false.
        /// </summary>
        public bool JumpToNight(int nightIndex)
        {
            if (_machine == null || _machine.Phase == GamePhase.Boot || _machine.IsOver
                || nightIndex < 0 || nightIndex >= _machine.TotalNights)
            {
                Debug.LogWarning($"{LogPrefix}{nightIndex + 1}. geceye atlanamadı (evre {CurrentPhase}).", this);
                return false;
            }

            _waves.AbortNight();
            Debug.Log($"{LogPrefix}Debug: {nightIndex + 1}. gecenin gündüzüne atlanıyor.", this);
            return _machine.JumpToDay(nightIndex);
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
    }
}
