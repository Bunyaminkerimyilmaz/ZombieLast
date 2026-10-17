using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Jeneratörün yakıtı ve gücü (GDD §12); jeneratörün kökünde, <see cref="IPowerService"/> olarak Awake'te kaydolur.
    /// Gece (<see cref="GamePhase.Night"/>) taban + taret başına tüketimle yakıt yakar (<see cref="FuelBurner"/>) ve cüzdandan
    /// (<see cref="IWallet.Fuel"/>) tam birim düşer. Cüzdanın yakıt kapasitesini <see cref="GeneratorData.FuelCapacity"/> yapar.
    /// Güç = ayakta ve yakıt &gt; 0 (<see cref="PowerRule"/>): yakıt bitince <see cref="PowerChangedEvent"/> (false) +
    /// <see cref="PowerLostEvent"/>, yakıt gelince (market, acil yakıt, F8) <see cref="PowerChangedEvent"/> (true).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Generator))]
    public sealed class GeneratorPower : MonoBehaviour, IPowerService
    {
        private const string LogPrefix = "[Base] ";

        private readonly FuelBurner _burner = new FuelBurner();
        private Generator _generator;
        private IWallet _wallet;
        private ITargetRegistry _registry;
        private bool _isNight;
        private bool _isPowered = true;
        private float _rate;

        public bool IsPowered => _isPowered;

        public bool IsConsuming => _isNight && Owner.Health.IsAlive;

        public int FuelCapacity => Data != null ? Data.FuelCapacity : 0;

        public float FuelConsumptionRate => _rate;

        public float EstimatedFuelSeconds => _burner.EstimateSeconds(_wallet != null ? _wallet.Fuel : 0, _rate);

        /// <summary>Yakılmış ama cüzdandan henüz düşülmemiş kesir (debug/test).</summary>
        public float PendingFuel => _burner.Pending;

        private Generator Owner => _generator != null ? _generator : _generator = GetComponent<Generator>();

        private GeneratorData Data => Owner.Data;

        private void Awake()
        {
            ServiceLocator.Register<IPowerService>(this);
            if (Data != null)
            {
                _rate = FuelBurner.Rate(Data.BaseFuelConsumption, Data.FuelConsumptionPerTurret, 0);
            }
        }

        private void OnEnable()
        {
            Owner.Health.Died += OnDied;
            EventBus<FuelChangedEvent>.Subscribe(OnFuelChanged);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
        }

        private void OnDisable()
        {
            Owner.Health.Died -= OnDied;
            EventBus<FuelChangedEvent>.Unsubscribe(OnFuelChanged);
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPowerService>(this);
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out _wallet))
            {
                if (Data != null)
                {
                    _wallet.SetCapacity(WalletResource.Fuel, Data.FuelCapacity);
                }
            }
            else
            {
                Debug.LogWarning("GeneratorPower: IWallet bulunamadı; yakıt tükenmeyecek, güç hep var. Sahnede [Systems]/Economy var mı?", this);
            }

            ServiceLocator.TryGet(out _registry);
            if (ServiceLocator.TryGet(out IGameStateService gameState))
            {
                _isNight = gameState.CurrentPhase == GamePhase.Night;
            }

            // Başlangıçta yakıt yoksa (veya Start sırası yüzünden varsayılanı okuyan olduysa) durum olayla düzeltilir.
            Refresh(PowerLossReason.OutOfFuel);
        }

        private void Update()
        {
            if (_wallet == null || Data == null)
            {
                return;
            }

            int turrets = _registry != null ? _registry.GetAll(TargetType.Turret).Count : 0;
            _rate = FuelBurner.Rate(Data.BaseFuelConsumption, Data.FuelConsumptionPerTurret, turrets);
            if (!IsConsuming)
            {
                return;
            }

            int units = _burner.Burn(Time.deltaTime, _rate, _wallet.Fuel);
            if (units > 0 && _wallet.TrySpend(WalletResource.Fuel, units))
            {
                Refresh(PowerLossReason.OutOfFuel);
            }
        }

        private void OnFuelChanged(FuelChangedEvent changed)
        {
            Refresh(PowerLossReason.OutOfFuel);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            _isNight = changed.Phase == GamePhase.Night;
        }

        private void OnDied(DiedEvent died)
        {
            Refresh(PowerLossReason.GeneratorDestroyed);
        }

        // Durum değiştiyse duyurur. Cüzdan yoksa yakıt kısıtı yok.
        private void Refresh(PowerLossReason reason)
        {
            int fuel = _wallet != null ? _wallet.Fuel : int.MaxValue;
            bool powered = PowerRule.IsPowered(Owner.Health.IsAlive, fuel);
            if (powered == _isPowered)
            {
                return;
            }

            _isPowered = powered;
            EventBus<PowerChangedEvent>.Raise(new PowerChangedEvent(powered));
            if (powered)
            {
                Debug.Log($"{LogPrefix}PowerChangedEvent: güç geri geldi (yakıt {fuel}).", this);
                return;
            }

            string why = reason == PowerLossReason.GeneratorDestroyed ? "jeneratör yıkıldı" : "yakıt bitti";
            Debug.Log($"{LogPrefix}PowerLostEvent: güç kesildi ({why}).", this);
            EventBus<PowerLostEvent>.Raise(new PowerLostEvent(transform.position, reason));
        }
    }
}
