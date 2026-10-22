using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/HUD: oyun içi gösterge (GDD §16, 6.3 kapsamı). Evre adı + gündüz zamanlayıcısı, Gece X/N, oyuncu ve jeneratör
    /// can barları, silah adı + mermi, gece kalan düşman. Değerler olaylarla güncellenir; Update yok. Başlangıç değerleri Start'ta
    /// servislerden ve sahne adımının bağladığı <see cref="Health"/> bileşenlerinden okunur. Metinler UXML'de, sayılar
    /// <see cref="HudFormat"/>'ta.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HUDController : UIScreen
    {
        [Header("Can kaynakları (sahne adımı bağlar)")]
        [SerializeField, Tooltip("Oyuncunun Health bileşeni ([Gameplay]/Player).")]
        private Health _playerHealth;

        [SerializeField, Tooltip("Jeneratörün Health bileşeni ([Gameplay]/Base/Generator).")]
        private Health _generatorHealth;

        private readonly HealthBarBinding _playerBar = new HealthBarBinding();
        private readonly HealthBarBinding _generatorBar = new HealthBarBinding();
        private readonly VisualElement[] _phaseLabels = new VisualElement[HudElementNames.PhaseLabels.Length];

        private Label _dayTimer;
        private Label _nightValue;
        private VisualElement _remainingGroup;
        private Label _remainingValue;
        private BarView _playerBarView;
        private BarView _generatorBarView;
        private Label _weaponName;
        private Label _ammoValue;
        private Label _ammoReserve;

        private IGameStateService _gameState;
        private int _totalNights;
        private bool _started;

        protected override void BindElements()
        {
            for (int i = 0; i < _phaseLabels.Length; i++)
            {
                _phaseLabels[i] = Require<VisualElement>(HudElementNames.PhaseLabels[i]);
            }

            _dayTimer = Require<Label>(HudElementNames.DayTimer);
            _nightValue = Require<Label>(HudElementNames.NightValue);
            _remainingGroup = Require<VisualElement>(HudElementNames.RemainingGroup);
            _remainingValue = Require<Label>(HudElementNames.RemainingValue);
            _weaponName = Require<Label>(HudElementNames.WeaponName);
            _ammoValue = Require<Label>(HudElementNames.AmmoValue);
            _ammoReserve = Require<Label>(HudElementNames.AmmoReserve);

            var playerFill = Require<VisualElement>(HudElementNames.PlayerHealthFill);
            var playerValue = Require<Label>(HudElementNames.PlayerHealthValue);
            var generatorFill = Require<VisualElement>(HudElementNames.GeneratorHealthFill);
            var generatorValue = Require<Label>(HudElementNames.GeneratorHealthValue);
            _playerBarView = new BarView(playerFill, playerValue);
            _generatorBarView = new BarView(generatorFill, generatorValue);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<DayTimerTickEvent>.Subscribe(OnDayTimerTick);
            EventBus<NightStartedEvent>.Subscribe(OnNightStarted);
            EventBus<WaveProgressEvent>.Subscribe(OnWaveProgress);
            EventBus<PlayerRespawnedEvent>.Subscribe(OnPlayerRespawned);
            EventBus<WeaponChangedEvent>.Subscribe(OnWeaponChanged);
            EventBus<AmmoChangedEvent>.Subscribe(OnAmmoChanged);

            // Servisler ve canlar Start'ta okunur (sahipleri Awake'te canı kurar); sonraki açılışlarda burada.
            if (_started)
            {
                AttachHealthBars();
                ShowCurrentPhase();
            }
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<DayTimerTickEvent>.Unsubscribe(OnDayTimerTick);
            EventBus<NightStartedEvent>.Unsubscribe(OnNightStarted);
            EventBus<WaveProgressEvent>.Unsubscribe(OnWaveProgress);
            EventBus<PlayerRespawnedEvent>.Unsubscribe(OnPlayerRespawned);
            EventBus<WeaponChangedEvent>.Unsubscribe(OnWeaponChanged);
            EventBus<AmmoChangedEvent>.Unsubscribe(OnAmmoChanged);

            _playerBar.Detach();
            _generatorBar.Detach();
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (ServiceLocator.TryGet(out IWaveService waves))
            {
                _totalNights = waves.TotalNights;
            }
            else
            {
                Debug.LogWarning("HUDController: IWaveService bulunamadı; toplam gece sayısı gösterilemeyecek.", this);
            }

            if (!ServiceLocator.TryGet(out _gameState))
            {
                Debug.LogWarning("HUDController: IGameStateService bulunamadı; evre ve gece olaylarla gelene kadar görünmeyecek.", this);
            }

            if (_playerHealth == null || _generatorHealth == null)
            {
                Debug.LogWarning("HUDController: oyuncu veya jeneratör Health'i bağlı değil; Tools/Last Haven/Build Game Scene çalıştır.", this);
            }

            _started = true;
            AttachHealthBars();
            ShowCurrentPhase();
        }

        private void AttachHealthBars()
        {
            _playerBar.Attach(_playerHealth, _playerBarView);
            _generatorBar.Attach(_generatorHealth, _generatorBarView);
        }

        private void ShowCurrentPhase()
        {
            if (_gameState != null)
            {
                ShowPhase(_gameState.CurrentPhase, _gameState.NightIndex, _gameState.PhaseTimeRemaining);
            }
        }

        private void ShowPhase(GamePhase phase, int nightIndex, float timeRemaining)
        {
            string phaseLabel = HudPhaseRules.PhaseLabelName(phase);
            for (int i = 0; i < _phaseLabels.Length; i++)
            {
                _phaseLabels[i].SetShown(string.Equals(HudElementNames.PhaseLabels[i], phaseLabel, System.StringComparison.Ordinal));
            }

            _nightValue.text = HudFormat.OfTotal(nightIndex + 1, _totalNights);

            bool showTimer = HudPhaseRules.ShowsDayTimer(phase);
            _dayTimer.SetShown(showTimer);
            if (showTimer)
            {
                // İlk DayTimerTickEvent'e kadar önceki gündüzün son değeri görünmesin.
                _dayTimer.text = NumberText.Timer(Mathf.CeilToInt(timeRemaining));
            }

            _remainingGroup.SetShown(HudPhaseRules.ShowsRemainingEnemies(phase));
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            ShowPhase(changed.Phase, changed.NightIndex, changed.Duration);
        }

        private void OnDayTimerTick(DayTimerTickEvent tick)
        {
            _dayTimer.text = NumberText.Timer(tick.RemainingSeconds);
        }

        private void OnNightStarted(NightStartedEvent started)
        {
            _remainingValue.text = NumberText.Count(started.TotalEnemies);
        }

        private void OnWaveProgress(WaveProgressEvent progress)
        {
            _remainingValue.text = NumberText.Count(progress.RemainingEnemies);
        }

        private void OnPlayerRespawned(PlayerRespawnedEvent respawned)
        {
            _playerBarView.Set(respawned.CurrentHealth, respawned.MaxHealth);
        }

        private void OnWeaponChanged(WeaponChangedEvent changed)
        {
            _weaponName.text = changed.DisplayName;
        }

        // Şarjör ve yedek ayrı etiketler: her atışta sadece önbellekli şarjör sayısı değişir, yeni string oluşmaz.
        private void OnAmmoChanged(AmmoChangedEvent changed)
        {
            _ammoValue.text = NumberText.Count(changed.Ammo);
            _ammoReserve.text = changed.HasInfiniteReserve ? HudFormat.Infinite : NumberText.Count(changed.Reserve);
        }
    }
}
