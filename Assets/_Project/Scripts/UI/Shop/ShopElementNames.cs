using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>
    /// Market panelinin (<c>UI/Shop/Shop.uxml</c>) ve kart şablonunun (<c>UI/Shop/ShopCard.uxml</c>) element adları. EditMode testi
    /// kontrol eder. Sekme ve sayfa adları <see cref="ShopCategory"/> değeriyle sıralıdır.
    /// </summary>
    public static class ShopElementNames
    {
        // Shop.uxml
        public const string MoneyValue = "shop-money-value";
        public const string CloseButton = "shop-close-button";
        public const string TabWeapons = "shop-tab-weapons";
        public const string TabUpgrades = "shop-tab-upgrades";
        public const string TabSupplies = "shop-tab-supplies";
        public const string PageWeapons = "shop-page-weapons";
        public const string PageUpgrades = "shop-page-upgrades";
        public const string PageSupplies = "shop-page-supplies";

        // ShopCard.uxml
        public const string CardName = "card-name";
        public const string CardDescription = "card-description";
        public const string CardDots = "card-dots";
        public const string CardPrice = "card-price";
        public const string CardMax = "card-max";
        public const string CardOwned = "card-owned";
        public const string CardLocked = "card-locked";
        public const string CardRequirement = "card-requirement";
        public const string CardUnavailable = "card-unavailable";
        public const string CardBuyButton = "card-buy-button";

        /// <summary>Sekme butonları, <see cref="ShopCategory"/> sırasıyla.</summary>
        public static readonly string[] Tabs = { TabWeapons, TabUpgrades, TabSupplies };

        /// <summary>Kart sayfaları, <see cref="ShopCategory"/> sırasıyla.</summary>
        public static readonly string[] Pages = { PageWeapons, PageUpgrades, PageSupplies };
    }
}
