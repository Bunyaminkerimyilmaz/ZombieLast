using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Market panelinin UI Toolkit tarafı (<c>UI/Shop/Shop.uxml</c>): sekmeler (<see cref="ShopTabBar"/>), kartlar
    /// (<see cref="ShopCardGrid"/>), para etiketi, Kapat butonu ve odak. Açılınca seçili sekmenin ilk kartı odak alır (bir sonraki
    /// panel güncellemesinde: gizli element odak alamaz); kapanınca odak bırakılır (gizli butonda kalırsa oyundaki Enter / A
    /// onu tıklardı). Tab panelin kapatma tuşu olduğu için açıkken sıralı odak gezinmesi (Next/Previous) yutulur. UIDocument her
    /// açılışta ağacı yeniden kurduğu için denetleyici her <c>OnEnable</c>'da yenisini oluşturur.
    /// </summary>
    public sealed class ShopPanelView
    {
        private readonly VisualElement _screen;
        private readonly Label _moneyValue;
        private readonly Button _closeButton;
        private readonly ShopTabBar _tabs;
        private ShopCardGrid _grid;
        private VisualElement _navigationRoot;
        private Func<bool> _isOpen;
        private Action _closeClicked;

        /// <param name="tabs">Sekme butonları, <see cref="ShopCategory"/> sırasıyla.</param>
        /// <param name="pages">Kart sayfaları, aynı sırayla.</param>
        /// <param name="selected">Başta seçili sekme (önceki açılıştan).</param>
        public ShopPanelView(VisualElement screen, Label moneyValue, Button closeButton, Button[] tabs, VisualElement[] pages,
            ShopCategory selected)
        {
            _screen = screen;
            _moneyValue = moneyValue;
            _closeButton = closeButton;
            _tabs = new ShopTabBar(tabs, pages);
            _tabs.Select(selected);
        }

        public ShopCategory SelectedCategory => _tabs.Selected;

        /// <param name="isOpen">Panel açık mı (sıralı gezinme sadece açıkken yutulur).</param>
        /// <param name="closeClicked">Kapat butonu.</param>
        public void Bind(Func<bool> isOpen, Action closeClicked)
        {
            _isOpen = isOpen;
            _closeClicked = closeClicked;
            _tabs.Bind();
            _closeButton.clicked += _closeClicked;

            // Odak gezinmesi olayı, odak yokken panelin kökünü hedefler: süzgeç kökte (bütün UIDocument'lar aynı paneli paylaşır).
            _navigationRoot = _screen.panel?.visualTree;
            _navigationRoot?.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
        }

        public void Unbind()
        {
            _tabs.Unbind();
            if (_closeClicked != null)
            {
                _closeButton.clicked -= _closeClicked;
            }

            _navigationRoot?.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            _navigationRoot = null;
        }

        /// <summary>Kartları kurar (önceki kurulum varsa onu bırakır; UIDocument yeni ağaç kurduysa sayfalar zaten boştur).</summary>
        public void BuildCards(IReadOnlyList<IShopItem> items, VisualTreeAsset template, Action<ShopCardView> buyClicked)
        {
            _grid = new ShopCardGrid();
            _grid.Build(items, template, _tabs, buyClicked);
        }

        public void Refresh(IShopService shop)
        {
            _grid?.Refresh(shop);
        }

        public void ShowMoney(int money)
        {
            _moneyValue.text = HudFormat.Count(money);
        }

        public void PlayPurchased(string itemId)
        {
            _grid?.Find(itemId)?.PlayPurchased();
        }

        /// <summary>Seçili sekmenin ilk kartına (yoksa sekmenin kendisine) bir sonraki panel güncellemesinde odaklanır.</summary>
        public void FocusFirstCardLater()
        {
            _screen.schedule.Execute(FocusFirstCard);
        }

        /// <summary>Panel kapanırken: odak paneldeyse bırakılır, animasyonlar durur.</summary>
        public void ReleaseFocus()
        {
            if (_screen.panel?.focusController?.focusedElement is VisualElement focused && _screen.Contains(focused))
            {
                focused.Blur();
            }

            _grid?.StopAnimations();
        }

        private void FocusFirstCard()
        {
            if (_isOpen != null && _isOpen())
            {
                Focusable target = (Focusable)_grid?.FirstButton(_tabs.Selected) ?? _tabs.SelectedTab;
                target.Focus();
            }
        }

        private void OnNavigationMove(NavigationMoveEvent evt)
        {
            if (_isOpen != null && _isOpen()
                && (evt.direction == NavigationMoveEvent.Direction.Next || evt.direction == NavigationMoveEvent.Direction.Previous))
            {
                evt.StopPropagation();
                _screen.panel?.focusController?.IgnoreEvent(evt);
            }
        }
    }
}
