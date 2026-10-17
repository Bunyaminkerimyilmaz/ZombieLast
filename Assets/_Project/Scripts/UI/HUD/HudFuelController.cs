using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un yakıt ve güç bölümü (GDD §12, §16): cüzdan panelinde yakıt barı (yakıt / kapasite) ve tahmini kalan süre
    /// (<see cref="IPowerService.EstimatedFuelSeconds"/>; gündüz de bu gecenin tüketimiyle), güç yokken yanıp sönen "GÜÇ YOK".
    /// Yanıp sönme USS transition'ıyla: <c>hud-power-warning--dim</c> sınıfı her transition bitişinde (<see cref="TransitionEndEvent"/>)
    /// açılıp kapanır (süre USS'te). HUD'un UIDocument'ını paylaşır. Bar <see cref="FuelChangedEvent"/>, uyarı
    /// <see cref="PowerChangedEvent"/> ile; tahmin her kare okunur, metin sadece gösterilen saniye değişince yazılır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudFuelController : UIDocumentController
    {
        /// <summary>Uyarının sönük yarısı (<c>HUD.uss</c>).</summary>
        public const string WarningDimClass = "hud-power-warning--dim";

        private VisualElement _fuelFill;
        private Label _estimate;
        private VisualElement _warning;
        private IWallet _wallet;
        private IPowerService _power;
        private int _shownSeconds = int.MinValue;
        private bool _blinking;
        private bool _started;

        protected override void BindElements()
        {
            _fuelFill = Require<VisualElement>(HudElementNames.FuelFill);
            _estimate = Require<Label>(HudElementNames.FuelEstimate);
            _warning = Require<VisualElement>(HudElementNames.PowerWarning);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _warning.RegisterCallback<TransitionEndEvent>(OnWarningTransitionEnd);
            EventBus<FuelChangedEvent>.Subscribe(OnFuelChanged);
            EventBus<PowerChangedEvent>.Subscribe(OnPowerChanged);

            // Servisler Awake'te kaydolur; ilk değer Start'ta, sonraki açılışlarda burada okunur.
            if (_started)
            {
                ShowCurrent();
            }
        }

        private void OnDisable()
        {
            EventBus<FuelChangedEvent>.Unsubscribe(OnFuelChanged);
            EventBus<PowerChangedEvent>.Unsubscribe(OnPowerChanged);
            _warning?.UnregisterCallback<TransitionEndEvent>(OnWarningTransitionEnd);
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            ServiceLocator.TryGet(out _wallet);
            if (!ServiceLocator.TryGet(out _power))
            {
                Debug.LogWarning("HudFuelController: IPowerService bulunamadı; yakıt barı, tahmin ve güç uyarısı çalışmayacak. "
                                 + "Sahnede jeneratör var mı?", this);
            }

            _started = true;
            ShowCurrent();
        }

        private void Update()
        {
            if (!IsBound || _power == null)
            {
                return;
            }

            int seconds = HudFormat.DisplaySeconds(_power.EstimatedFuelSeconds);
            if (seconds != _shownSeconds)
            {
                _shownSeconds = seconds;
                _estimate.text = HudFormat.Estimate(seconds);
            }
        }

        private void ShowCurrent()
        {
            _shownSeconds = int.MinValue; // geçersiz: tahmin bir sonraki karede mutlaka yazılsın
            if (_wallet != null)
            {
                SetFuel(_wallet.Fuel);
            }

            SetPowered(_power == null || _power.IsPowered);
        }

        private void OnFuelChanged(FuelChangedEvent changed)
        {
            SetFuel(changed.Fuel);
        }

        private void OnPowerChanged(PowerChangedEvent changed)
        {
            SetPowered(changed.IsPowered);
        }

        private void SetFuel(int fuel)
        {
            int capacity = _power != null ? _power.FuelCapacity : 0;
            _fuelFill.style.width = Length.Percent(HudFormat.Ratio(fuel, capacity) * 100f);
        }

        private void SetPowered(bool powered)
        {
            _warning.SetShown(!powered);
            _warning.RemoveFromClassList(WarningDimClass);
            _blinking = !powered;
            if (_blinking)
            {
                // Az önce görünür oldu: transition'ın başlaması için sönükleşme bir sonraki karede.
                _warning.schedule.Execute(StartBlink);
            }
        }

        private void StartBlink()
        {
            if (_blinking)
            {
                _warning.AddToClassList(WarningDimClass);
            }
        }

        private void OnWarningTransitionEnd(TransitionEndEvent transitionEnd)
        {
            // Sönükleşme/parlama bitti: tersine çevir (yanıp sönme döngüsü). Güç gelince döngü durur.
            if (_blinking)
            {
                _warning.ToggleInClassList(WarningDimClass);
            }
        }
    }
}
