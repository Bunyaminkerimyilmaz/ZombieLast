using System;
using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>Kartın fiyat alanında ne görünür.</summary>
    public enum ShopPriceDisplay
    {
        /// <summary>Sıradaki alımın fiyatı.</summary>
        Price,

        /// <summary>"MAKS": seviyeli öğe en yüksek seviyede.</summary>
        Max,

        /// <summary>"ALINDI": tek seviyeli öğe (silah) alınmış.</summary>
        Owned,
    }

    /// <summary>Kartta fiyatın üstündeki uyarı satırı.</summary>
    public enum ShopCardNotice
    {
        None,

        /// <summary>"Önce al: X" (ön koşul öğesi alınmadı).</summary>
        Locked,

        /// <summary>"Şu an alınamaz" (can/depo dolu, oyuncu ölü, market kapalı).</summary>
        Unavailable,
    }

    /// <summary>
    /// Bir market kartının görünümü (saf): <see cref="IShopService"/>'in durum, seviye ve fiyatından hesaplanır. Para yetmeyen,
    /// kilitli veya şu an alınamayan kart soluk görünür; en yüksek seviyede fiyat yerine "MAKS" (tek seviyeli silahta "ALINDI").
    /// Seviye noktaları sadece birden fazla seviyesi olan öğede (yükseltmeler, mıknatıs) vardır. Eşitlik, kartın sadece durum
    /// değişince yeniden çizilmesi içindir.
    /// </summary>
    public readonly struct ShopCardState : IEquatable<ShopCardState>
    {
        private ShopCardState(ShopPurchaseStatus status, int price, int dotCount, int filledDots, ShopPriceDisplay priceDisplay)
        {
            Status = status;
            Price = price;
            DotCount = dotCount;
            FilledDots = filledDots;
            PriceDisplay = priceDisplay;
        }

        public ShopPurchaseStatus Status { get; }

        /// <summary>Sıradaki alımın fiyatı (<see cref="PriceDisplay"/> <see cref="ShopPriceDisplay.Price"/> iken gösterilir).</summary>
        public int Price { get; }

        /// <summary>Seviye noktası sayısı; tek seviyeli ve sarf öğelerde 0.</summary>
        public int DotCount { get; }

        /// <summary>Dolu nokta sayısı (alınan seviye, 0..<see cref="DotCount"/>).</summary>
        public int FilledDots { get; }

        public ShopPriceDisplay PriceDisplay { get; }

        /// <summary>Satın Al şu an işe yarar mı.</summary>
        public bool CanBuy => Status == ShopPurchaseStatus.Available;

        /// <summary>Kart soluk mu: alınabilir veya tamamlanmış (maks.) değilse.</summary>
        public bool IsDimmed => Status != ShopPurchaseStatus.Available && Status != ShopPurchaseStatus.MaxLevel;

        /// <summary>Fiyat "para yetmiyor" renginde mi.</summary>
        public bool IsPriceShort => Status == ShopPurchaseStatus.NotEnoughMoney;

        public ShopCardNotice Notice
        {
            get
            {
                switch (Status)
                {
                    case ShopPurchaseStatus.Locked:
                        return ShopCardNotice.Locked;
                    case ShopPurchaseStatus.NotApplicable:
                    case ShopPurchaseStatus.Closed:
                    case ShopPurchaseStatus.UnknownItem:
                        return ShopCardNotice.Unavailable;
                    default:
                        return ShopCardNotice.None;
                }
            }
        }

        /// <param name="status">Servisin durumu (<see cref="IShopService.GetStatus"/>).</param>
        /// <param name="level">Alınan seviye (<see cref="IShopService.GetLevel"/>).</param>
        /// <param name="maxLevel">Öğenin en yüksek seviyesi (<see cref="IShopItem.MaxLevel"/>; 0 = sarf).</param>
        /// <param name="price">Sıradaki alımın fiyatı (<see cref="IShopService.GetPrice"/>).</param>
        public static ShopCardState From(ShopPurchaseStatus status, int level, int maxLevel, int price)
        {
            int dotCount = maxLevel > 1 ? maxLevel : 0;
            int filled = Math.Max(0, Math.Min(level, dotCount));
            ShopPriceDisplay display = status != ShopPurchaseStatus.MaxLevel
                ? ShopPriceDisplay.Price
                : maxLevel == 1 ? ShopPriceDisplay.Owned : ShopPriceDisplay.Max;
            return new ShopCardState(status, Math.Max(0, price), dotCount, filled, display);
        }

        public bool Equals(ShopCardState other)
        {
            return Status == other.Status && Price == other.Price && DotCount == other.DotCount
                   && FilledDots == other.FilledDots && PriceDisplay == other.PriceDisplay;
        }

        public override bool Equals(object obj)
        {
            return obj is ShopCardState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Status, Price, DotCount, FilledDots, (int)PriceDisplay);
        }
    }
}
