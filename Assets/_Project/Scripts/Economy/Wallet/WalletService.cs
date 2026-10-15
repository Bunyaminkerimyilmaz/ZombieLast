using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// [Systems]/Economy: run başında <see cref="EconomyConfig"/>'in başlangıç değerleriyle bir <see cref="Wallet"/> oluşturur ve
    /// <see cref="IWallet"/> olarak kaydeder. Cüzdanın C# olaylarını <see cref="MoneyChangedEvent"/> / <see cref="FuelChangedEvent"/>
    /// olarak duyurur. <see cref="DawnEvent"/>'te faizi öder (<see cref="InterestPaidEvent"/>), <see cref="EarlyReadyEvent"/>'te
    /// bonusu ekler. Config'i <see cref="IEconomyRules"/> olarak da kaydeder (satış iadesi). Sahne yeniden yüklenince (yeni run) sıfırlanır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WalletService : MonoBehaviour
    {
        private const string LogPrefix = "[Economy] ";

        [SerializeField, Tooltip("Başlangıç para/yakıt ve faiz. Üretici bağlar.")]
        private EconomyConfig _config;

        private Wallet _wallet;
        private WalletIncome _income;

        /// <summary>Debug ve testler için; oyun kodu servisi ServiceLocator'dan alır.</summary>
        public Wallet Wallet => _wallet;

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("WalletService: EconomyConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _wallet = new Wallet(_config.StartingMoney, _config.StartingFuel);
            _income = new WalletIncome(_wallet, _config.CreateInterestRule());
            ServiceLocator.Register<IWallet>(_wallet);
            ServiceLocator.Register<IEconomyRules>(_config);
        }

        private void OnEnable()
        {
            if (_wallet == null)
            {
                return;
            }

            _wallet.MoneyChanged += OnMoneyChanged;
            _wallet.FuelChanged += OnFuelChanged;
            EventBus<DawnEvent>.Subscribe(OnDawn);
            EventBus<EarlyReadyEvent>.Subscribe(OnEarlyReady);
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.MoneyChanged -= OnMoneyChanged;
                _wallet.FuelChanged -= OnFuelChanged;
            }

            EventBus<DawnEvent>.Unsubscribe(OnDawn);
            EventBus<EarlyReadyEvent>.Unsubscribe(OnEarlyReady);
        }

        private void OnDestroy()
        {
            if (_wallet != null)
            {
                ServiceLocator.Unregister<IWallet>(_wallet);
                ServiceLocator.Unregister<IEconomyRules>(_config);
            }
        }

        private void OnMoneyChanged(int money, int delta)
        {
            EventBus<MoneyChangedEvent>.Raise(new MoneyChangedEvent(money, delta));
        }

        private void OnFuelChanged(int fuel, int delta)
        {
            EventBus<FuelChangedEvent>.Raise(new FuelChangedEvent(fuel, delta));
        }

        private void OnDawn(DawnEvent dawn)
        {
            int interest = _income.PayInterest();
            Debug.Log($"{LogPrefix}InterestPaidEvent: şafak faizi +{interest} para (toplam {_wallet.Money}).", this);
            EventBus<InterestPaidEvent>.Raise(new InterestPaidEvent(interest, _wallet.Money, dawn.NightIndex));
        }

        private void OnEarlyReady(EarlyReadyEvent ready)
        {
            int bonus = _income.AddEarlyReadyBonus(ready.BonusMoney);
            if (bonus > 0)
            {
                Debug.Log($"{LogPrefix}Erken hazır bonusu +{bonus} para (toplam {_wallet.Money}).", this);
            }
        }
    }
}
