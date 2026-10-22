using LastHaven.Core;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Ekonomi kısayolları ([Systems]/DebugEconomy; debug panelinin "Kaynak" düğmeleri de bunları çağırır): F7 para, F8 yakıt ekler
    /// (<see cref="IWallet"/>; miktarlar inspector'dan); Shift+F8 yakıtı az bırakır (güç kesintisini denemek için, 9.2). Değişiklik
    /// normal cüzdan yoluyla yapılır: HUD ve olaylar her zamanki gibi güncellenir. Sadece editörde ve development build'de çalışır
    /// (sınıf ve alanları her build'de var; sahnede "missing script" olmasın).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugEconomyControls : MonoBehaviour
    {
        [SerializeField, Min(0), Tooltip("F7'nin eklediği para.")]
        private int _moneyAmount = 1000;

        [SerializeField, Min(0), Tooltip("F8'in eklediği yakıt.")]
        private int _fuelAmount = 50;

        [SerializeField, Min(0), Tooltip("Shift+F8'in bıraktığı yakıt (fazlası harcanır).")]
        private int _lowFuelAmount = 3;

        /// <summary>F7'nin eklediği para (panel düğme metni).</summary>
        public int MoneyAmount => _moneyAmount;

        /// <summary>F8'in eklediği yakıt.</summary>
        public int FuelAmount => _fuelAmount;

        /// <summary>Shift+F8'in bıraktığı yakıt.</summary>
        public int LowFuelAmount => _lowFuelAmount;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Prefix = "[Debug] ";

        private InputAction[] _actions;
        private IWallet _wallet;

        /// <summary>F7: <see cref="MoneyAmount"/> kadar para ekler.</summary>
        public void AddMoney()
        {
            Add(WalletResource.Money, _moneyAmount, "F7", "para");
        }

        /// <summary>F8: <see cref="FuelAmount"/> kadar yakıt ekler (jeneratör kapasitesinde doyar).</summary>
        public void AddFuel()
        {
            Add(WalletResource.Fuel, _fuelAmount, "F8", "yakıt");
        }

        /// <summary>Shift+F8: yakıtı <see cref="LowFuelAmount"/>'a indirir (fazlası harcanır).</summary>
        public void DrainFuel()
        {
            if (_wallet == null)
            {
                Debug.LogWarning($"{Prefix}Shift+F8: IWallet yok; yakıt değişmedi.", this);
                return;
            }

            int excess = _wallet.Fuel - _lowFuelAmount;
            if (excess > 0)
            {
                _wallet.TrySpend(WalletResource.Fuel, excess);
            }

            Debug.Log($"{Prefix}Shift+F8: yakıt {_wallet.Fuel}.", this);
        }

        private void Awake()
        {
            _actions = new[]
            {
                DebugHotkeys.Create("<Keyboard>/f7", AddMoney),
                DebugHotkeys.Create("<Keyboard>/f8", OnF8),
            };
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _wallet))
            {
                Debug.LogWarning("DebugEconomyControls: IWallet bulunamadı; F7/F8 çalışmayacak. Sahnede [Systems]/Economy var mı?", this);
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Enable();
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Disable();
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Dispose();
            }
        }

        // F8 tek action; Shift basılıysa ekleme yerine yakıtı az bırakır (iki ayrı bağlama ikisini birden tetiklerdi).
        private void OnF8()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.shiftKey.isPressed)
            {
                DrainFuel();
                return;
            }

            AddFuel();
        }

        private void Add(WalletResource resource, int amount, string key, string label)
        {
            if (_wallet == null)
            {
                Debug.LogWarning($"{Prefix}{key}: IWallet yok; {label} eklenemedi.", this);
                return;
            }

            _wallet.Add(resource, amount);
            Debug.Log($"{Prefix}{key}: +{amount} {label} (şimdi {_wallet.Get(resource)}).", this);
        }
#endif
    }
}
