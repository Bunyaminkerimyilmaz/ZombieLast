using System;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir market kartı (<c>UI/Shop/ShopCard.uxml</c> örneği): ad, açıklama, seviye noktaları, fiyat / "MAKS" / "ALINDI", uyarı
    /// satırı ve Satın Al butonu. Durumu <see cref="ShopCardState"/>'ten USS sınıflarıyla uygular (sadece değişince). Satın alınca
    /// <see cref="PurchasedClass"/>, alınamayınca <see cref="DeniedClass"/> eklenir; kartın kendi transition'ı bitince
    /// (<see cref="TransitionEndEvent"/>) sınıf kaldırılır ve aynı transition geri döndürür (süreler <c>Shop.uss</c>'te).
    /// Buton hiç kapatılmaz: odak sırası durumla değişmez, alınamayan kartta tıklama "reddedildi" animasyonu oynatır.
    /// </summary>
    public sealed class ShopCardView
    {
        public const string CardClass = "shop-card";
        public const string DimmedClass = "shop-card--dimmed";
        public const string DoneClass = "shop-card--done";
        public const string FocusedClass = "shop-card--focused";
        public const string PurchasedClass = "shop-card--purchased";
        public const string DeniedClass = "shop-card--denied";
        public const string PriceShortClass = "shop-card__price--short";
        public const string DotClass = "shop-card__dot";
        public const string DotFilledClass = "shop-card__dot--filled";

        private readonly Label _price;
        private readonly Label _max;
        private readonly Label _owned;
        private readonly VisualElement _locked;
        private readonly Label _unavailable;
        private readonly VisualElement[] _dots;
        private readonly Action<ShopCardView> _buyClicked;
        private ShopCardState _state;
        private bool _hasState;

        private ShopCardView(IShopItem item, VisualElement root, Button buyButton, Label price, Label max, Label owned,
            VisualElement locked, Label unavailable, VisualElement[] dots, Action<ShopCardView> buyClicked)
        {
            Item = item;
            Root = root;
            BuyButton = buyButton;
            _price = price;
            _max = max;
            _owned = owned;
            _locked = locked;
            _unavailable = unavailable;
            _dots = dots;
            _buyClicked = buyClicked;

            BuyButton.clicked += OnBuyClicked;
            BuyButton.RegisterCallback<FocusInEvent>(_ => Root.AddToClassList(FocusedClass));
            BuyButton.RegisterCallback<FocusOutEvent>(_ => Root.RemoveFromClassList(FocusedClass));
            Root.RegisterCallback<TransitionEndEvent>(OnTransitionFinished);
            Root.RegisterCallback<TransitionCancelEvent>(OnTransitionFinished);
        }

        public IShopItem Item { get; }

        /// <summary>Kartın kök elementi (şablon örneği).</summary>
        public VisualElement Root { get; }

        public Button BuyButton { get; }

        /// <summary>Son uygulanan durum; <see cref="Apply"/> çağrılmadıysa varsayılan.</summary>
        public ShopCardState State => _state;

        /// <summary>
        /// Şablonu örnekler, metinleri yazar ve seviye noktalarını kurar. Şablonda element eksikse hata loglar ve null döner.
        /// </summary>
        /// <param name="requirementName">Ön koşul öğesinin görünen adı ("Önce al: X"); yoksa boş.</param>
        /// <param name="buyClicked">Satın Al'a basılınca (fare, Enter, gamepad A).</param>
        public static ShopCardView Create(VisualTreeAsset template, IShopItem item, string requirementName, Action<ShopCardView> buyClicked)
        {
            TemplateContainer container = template.Instantiate();
            VisualElement root = container.Q(className: CardClass);
            Label name = root?.Q<Label>(ShopElementNames.CardName);
            Label description = root?.Q<Label>(ShopElementNames.CardDescription);
            VisualElement dots = root?.Q(ShopElementNames.CardDots);
            Label price = root?.Q<Label>(ShopElementNames.CardPrice);
            Label max = root?.Q<Label>(ShopElementNames.CardMax);
            Label owned = root?.Q<Label>(ShopElementNames.CardOwned);
            VisualElement locked = root?.Q(ShopElementNames.CardLocked);
            Label requirement = root?.Q<Label>(ShopElementNames.CardRequirement);
            Label unavailable = root?.Q<Label>(ShopElementNames.CardUnavailable);
            Button buy = root?.Q<Button>(ShopElementNames.CardBuyButton);
            if (name == null || description == null || dots == null || price == null || max == null || owned == null
                || locked == null || requirement == null || unavailable == null || buy == null)
            {
                Debug.LogError($"ShopCardView: kart şablonunda '{CardClass}' kökü veya bir element eksik (ShopElementNames.Card*).");
                return null;
            }

            name.text = item.DisplayName;
            description.text = item.Description;
            requirement.text = requirementName ?? string.Empty;

            // Şablonun TemplateContainer sarmalayıcısı yerleşimi bozmasın: kartın kendisi sayfaya eklenir.
            root.RemoveFromHierarchy();
            var dotElements = new VisualElement[item.MaxLevel > 1 ? item.MaxLevel : 0];
            for (int i = 0; i < dotElements.Length; i++)
            {
                dotElements[i] = new VisualElement { pickingMode = PickingMode.Ignore };
                dotElements[i].AddToClassList(DotClass);
                dots.Add(dotElements[i]);
            }

            dots.SetShown(dotElements.Length > 0);
            return new ShopCardView(item, root, buy, price, max, owned, locked, unavailable, dotElements, buyClicked);
        }

        /// <summary>Durumu uygular; öncekiyle aynıysa hiçbir şey yapmaz.</summary>
        public void Apply(ShopCardState state)
        {
            if (_hasState && state.Equals(_state))
            {
                return;
            }

            bool priceChanged = !_hasState || state.Price != _state.Price;
            _state = state;
            _hasState = true;

            Root.EnableInClassList(DimmedClass, state.IsDimmed);
            Root.EnableInClassList(DoneClass, state.PriceDisplay != ShopPriceDisplay.Price);
            _price.SetShown(state.PriceDisplay == ShopPriceDisplay.Price);
            _max.SetShown(state.PriceDisplay == ShopPriceDisplay.Max);
            _owned.SetShown(state.PriceDisplay == ShopPriceDisplay.Owned);
            _price.EnableInClassList(PriceShortClass, state.IsPriceShort);
            if (priceChanged)
            {
                _price.text = HudFormat.Count(state.Price);
            }

            _locked.SetShown(state.Notice == ShopCardNotice.Locked);
            _unavailable.SetShown(state.Notice == ShopCardNotice.Unavailable);
            for (int i = 0; i < _dots.Length; i++)
            {
                _dots[i].EnableInClassList(DotFilledClass, i < state.FilledDots);
            }
        }

        /// <summary>Satın alma animasyonu (kısa büyüme + yeşil parlama).</summary>
        public void PlayPurchased()
        {
            Root.RemoveFromClassList(DeniedClass);
            Root.AddToClassList(PurchasedClass);
        }

        /// <summary>Alınamadı animasyonu (kısa kayma + kırmızı çerçeve).</summary>
        public void PlayDenied()
        {
            if (!Root.ClassListContains(PurchasedClass))
            {
                Root.AddToClassList(DeniedClass);
            }
        }

        /// <summary>Animasyon sınıflarını hemen kaldırır (panel kapanırken).</summary>
        public void StopAnimations()
        {
            Root.RemoveFromClassList(PurchasedClass);
            Root.RemoveFromClassList(DeniedClass);
        }

        private void OnBuyClicked()
        {
            _buyClicked?.Invoke(this);
        }

        private void OnTransitionFinished(EventBase evt)
        {
            // Alt elementlerin (butonun hover rengi) transition olayları da kabarır; sadece kartın kendisininki.
            if (evt.target == Root)
            {
                StopAnimations();
            }
        }
    }
}
