using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/ShopPanel: market arayüzü (GDD §10, §16). Gündüz Tab / Select ile açılır, tekrar Tab veya Esc / B (iptal) ya da
    /// Kapat ile kapanır; açıkken girdi <see cref="InputMode.UI"/>'dadır (oyuncu durur, oyun durmaz). Gündüz bitince kendiliğinden
    /// kapanır. Kartlar <see cref="IShopService"/>'in öğelerinden bir kez kurulur; durumlar açılışta ve panel açıkken
    /// <see cref="ShopRefreshTriggers"/>'ın olaylarında yenilenir. Gezinme UI Toolkit odağıyla (project-wide
    /// <c>InputSystem_Actions</c> UI haritası): yön tuşları / WASD / D-pad uzamsal, Enter / A odaktaki butona basar (sekme veya
    /// Satın Al). Elementler ve odak <see cref="ShopPanelView"/>'da; bu bileşen servisler, girdi ve evre.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopPanelController : UIScreen
    {
        private const string LogPrefix = "[UI] ";

        [SerializeField, Tooltip("Kart şablonu (UI/Shop/ShopCard.uxml). Sahne adımı bağlar.")]
        private VisualTreeAsset _cardTemplate;

        private Label _moneyValue;
        private Button _closeButton;
        private Button[] _tabButtons;
        private VisualElement[] _pages;
        private ShopPanelView _view;
        private ShopRefreshTriggers _refreshTriggers;
        private ShopCategory _selectedCategory = ShopCategory.Weapons;

        private IInputReader _input;
        private IShopService _shop;
        private IGameStateService _gameState;
        private IWallet _wallet;
        private bool _started;

        private bool IsInputAlive => _input is Object unityObject ? unityObject != null : _input != null;

        protected override void BindElements()
        {
            _moneyValue = Require<Label>(ShopElementNames.MoneyValue);
            _closeButton = Require<Button>(ShopElementNames.CloseButton);
            _tabButtons = new Button[ShopElementNames.Tabs.Length];
            _pages = new VisualElement[ShopElementNames.Pages.Length];
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                _tabButtons[i] = Require<Button>(ShopElementNames.Tabs[i]);
                _pages[i] = Require<VisualElement>(ShopElementNames.Pages[i]);
            }
        }

        private void Awake()
        {
            SetShown(false);
            _refreshTriggers = new ShopRefreshTriggers(() => _view.Refresh(_shop), money => _view.ShowMoney(money),
                itemId => _view.PlayPurchased(itemId));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            // UIDocument her açılışta ağacı yeniden kurar: görünüm ve kartlar yeni elementlere bağlanır.
            _view = new ShopPanelView(ScreenElement, _moneyValue, _closeButton, _tabButtons, _pages, _selectedCategory);
            _view.Bind(() => IsShown, Close);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);

            // Servisler Start'ta alınır; ilk kurulumu Start yapar.
            if (_started)
            {
                _view.BuildCards(_shop.GetItems(), _cardTemplate, OnBuyClicked);
                SubscribeInput();
            }
        }

        private void OnDisable()
        {
            Close();
            if (_view != null)
            {
                _selectedCategory = _view.SelectedCategory;
                _view.Unbind();
            }

            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            if (_input != null)
            {
                _input.ShopPressed -= OnShopPressed;
                _input.CancelPressed -= OnCancelPressed;
            }
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (_cardTemplate == null || !ServiceLocator.TryGet(out _input) || !ServiceLocator.TryGet(out _shop))
            {
                Debug.LogWarning("ShopPanelController: kart şablonu, IInputReader veya IShopService yok; market açılmayacak. "
                                 + "Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            ServiceLocator.TryGet(out _gameState);
            ServiceLocator.TryGet(out _wallet);
            _started = true;
            _view.BuildCards(_shop.GetItems(), _cardTemplate, OnBuyClicked);
            SubscribeInput();
        }

        private void SubscribeInput()
        {
            _input.ShopPressed += OnShopPressed;
            _input.CancelPressed += OnCancelPressed;
        }

        private void OnShopPressed()
        {
            if (IsShown)
            {
                Close();
                return;
            }

            bool hasGameState = _gameState != null;
            GamePhase phase = hasGameState ? _gameState.CurrentPhase : GamePhase.Day;
            if (!ShopPanelRules.CanOpen(_input.Mode, hasGameState, phase))
            {
                if (_input.Mode == InputMode.Gameplay)
                {
                    Debug.Log($"{LogPrefix}Market sadece gündüz açılır ({phase}).", this);
                }

                return;
            }

            SetShown(true);
            _input.SetMode(InputMode.UI);
            if (_wallet != null)
            {
                _view.ShowMoney(_wallet.Money);
            }

            _view.Refresh(_shop);
            _refreshTriggers.Subscribe();
            _view.FocusFirstCardLater();
            Debug.Log($"{LogPrefix}Market açıldı.", this);
        }

        private void Close()
        {
            if (!IsShown)
            {
                return;
            }

            _refreshTriggers.Unsubscribe();
            _view?.ReleaseFocus();
            SetShown(false);
            if (IsInputAlive && _input.Mode == InputMode.UI)
            {
                _input.SetMode(InputMode.Gameplay);
            }

            Debug.Log($"{LogPrefix}Market kapandı.", this);
        }

        private void OnBuyClicked(ShopCardView card)
        {
            if (IsShown && !_shop.Buy(card.Item.Id))
            {
                card.PlayDenied();
            }
        }

        private void OnCancelPressed()
        {
            Close();
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (IsShown && !ShopPanelRules.StaysOpen(changed.Phase))
            {
                Debug.Log($"{LogPrefix}Gündüz bitti ({changed.Phase}); market kapanıyor.", this);
                Close();
            }
        }
    }
}
