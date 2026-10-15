using LastHaven.Core;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Ekonomi kısayolları ([Systems]/DebugEconomy): F7 para, F8 yakıt ekler (<see cref="IWallet"/>; miktarlar inspector'dan).
    /// Ekleme normal cüzdan yoluyla yapılır: HUD ve olaylar her zamanki gibi güncellenir. Sol üstte, gece kısayollarının altında
    /// küçük panel. Sadece editörde ve development build'de çalışır (sınıf ve alanları her build'de var; sahnede "missing script" olmasın).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugEconomyControls : MonoBehaviour
    {
        [SerializeField, Min(0), Tooltip("F7'nin eklediği para.")]
        private int _moneyAmount = 1000;

        [SerializeField, Min(0), Tooltip("F8'in eklediği yakıt.")]
        private int _fuelAmount = 50;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Prefix = "[Debug] ";

        // DebugWaveControls panelinin (y 120–204) altında.
        private static readonly Rect PanelRect = new Rect(10f, 210f, 290f, 64f);
        private static readonly Rect PanelTextRect = new Rect(18f, 214f, 274f, 56f);

        private InputAction[] _actions;
        private IWallet _wallet;
        private string _panelText = string.Empty;

        private void Awake()
        {
            _actions = new[]
            {
                DebugHotkeys.Create("<Keyboard>/f7", AddMoney),
                DebugHotkeys.Create("<Keyboard>/f8", AddFuel),
            };

            _panelText = "Ekonomi Kısayolları\n"
                         + "F7   +" + _moneyAmount + " para\n"
                         + "F8   +" + _fuelAmount + " yakıt";
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

        private void OnGUI()
        {
            GUI.Box(PanelRect, GUIContent.none);
            GUI.Label(PanelTextRect, _panelText);
        }

        private void AddMoney()
        {
            Add(WalletResource.Money, _moneyAmount, "F7", "para");
        }

        private void AddFuel()
        {
            Add(WalletResource.Fuel, _fuelAmount, "F8", "yakıt");
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
