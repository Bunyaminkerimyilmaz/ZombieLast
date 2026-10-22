using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un cüzdan bölümü (GDD §16): para ve yakıt. HUD'un UIDocument'ını <see cref="HUDController"/> ile paylaşır.
    /// Başlangıç değerleri Start'ta <see cref="IWallet"/>'tan, sonrası <see cref="MoneyChangedEvent"/> / <see cref="FuelChangedEvent"/>
    /// ile gelir; Update yok.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudWalletController : UIDocumentController
    {
        private Label _moneyValue;
        private Label _fuelValue;
        private bool _started;

        protected override void BindElements()
        {
            _moneyValue = Require<Label>(HudElementNames.MoneyValue);
            _fuelValue = Require<Label>(HudElementNames.FuelValue);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
            EventBus<FuelChangedEvent>.Subscribe(OnFuelChanged);

            // Cüzdan Awake'te kurulur; ilk değer Start'ta, sonraki açılışlarda burada okunur.
            if (_started)
            {
                ShowCurrent();
            }
        }

        private void OnDisable()
        {
            EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
            EventBus<FuelChangedEvent>.Unsubscribe(OnFuelChanged);
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            _started = true;
            if (!ShowCurrent())
            {
                Debug.LogWarning("HudWalletController: IWallet bulunamadı; para ve yakıt olaylarla gelene kadar görünmeyecek. "
                                 + "Sahnede [Systems]/Economy var mı?", this);
            }
        }

        private bool ShowCurrent()
        {
            if (!ServiceLocator.TryGet(out IWallet wallet))
            {
                return false;
            }

            _moneyValue.text = NumberText.Count(wallet.Money);
            _fuelValue.text = NumberText.Count(wallet.Fuel);
            return true;
        }

        private void OnMoneyChanged(MoneyChangedEvent changed)
        {
            _moneyValue.text = NumberText.Count(changed.Money);
        }

        private void OnFuelChanged(FuelChangedEvent changed)
        {
            _fuelValue.text = NumberText.Count(changed.Fuel);
        }
    }
}
