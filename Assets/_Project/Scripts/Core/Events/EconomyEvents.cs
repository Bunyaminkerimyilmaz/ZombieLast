using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Cüzdandaki para değiştiğinde gönderilir (<see cref="IWallet"/>). HUD, market ve inşa önizlemesi dinler.</summary>
    public readonly struct MoneyChangedEvent : IEvent
    {
        public MoneyChangedEvent(int money, int delta)
        {
            Money = money;
            Delta = delta;
        }

        /// <summary>Yeni miktar.</summary>
        public int Money { get; }

        /// <summary>Değişim: kazançta pozitif, harcamada negatif.</summary>
        public int Delta { get; }
    }

    /// <summary>Cüzdandaki yakıt değiştiğinde gönderilir (<see cref="IWallet"/>). HUD ve jeneratör dinler.</summary>
    public readonly struct FuelChangedEvent : IEvent
    {
        public FuelChangedEvent(int fuel, int delta)
        {
            Fuel = fuel;
            Delta = delta;
        }

        /// <summary>Yeni miktar (birim).</summary>
        public int Fuel { get; }

        /// <summary>Değişim: eklemede pozitif, tüketimde negatif.</summary>
        public int Delta { get; }
    }

    /// <summary>
    /// Şafakta faiz ödendiğinde gönderilir (GDD §10: paranın %10'u, en fazla 50), <see cref="DawnEvent"/> işlenirken ve
    /// paranın <see cref="MoneyChangedEvent"/>'inden sonra. Faiz 0 olsa da gönderilir. Şafak ekranı ve istatistik dinler.
    /// </summary>
    public readonly struct InterestPaidEvent : IEvent
    {
        public InterestPaidEvent(int amount, int money, int nightIndex)
        {
            Amount = amount;
            Money = money;
            NightIndex = nightIndex;
        }

        /// <summary>Ödenen faiz (para); 0 olabilir.</summary>
        public int Amount { get; }

        /// <summary>Faizden sonraki para.</summary>
        public int Money { get; }

        /// <summary>Biten gecenin 0 tabanlı indeksi (<see cref="DawnEvent.NightIndex"/>).</summary>
        public int NightIndex { get; }
    }

    /// <summary>
    /// Oyuncu bir jetona değince gönderilir (GDD §10), değer cüzdana eklendikten (<see cref="MoneyChangedEvent"/>) sonra.
    /// Ses, efekt ve istatistik dinler.
    /// </summary>
    public readonly struct CoinCollectedEvent : IEvent
    {
        public CoinCollectedEvent(int value, Vector3 position)
        {
            Value = value;
            Position = position;
        }

        /// <summary>Jetonun değeri (para).</summary>
        public int Value { get; }

        /// <summary>Toplandığı yer (dünya).</summary>
        public Vector3 Position { get; }
    }

    /// <summary>
    /// Kombo durumu (GDD §10). Oyuncunun her sayılan öldürmesinde, sıfırlanınca (hasar veya pencere doldu: sayı 0) ve kombo
    /// sürerken <b>her kare</b> (azalan pencere için) gönderilir. HUD ve istatistik dinler.
    /// </summary>
    public readonly struct ComboChangedEvent : IEvent
    {
        public ComboChangedEvent(int count, float multiplier, float windowRatio)
        {
            Count = count;
            Multiplier = multiplier;
            WindowRatio = windowRatio;
        }

        /// <summary>Ardışık öldürme sayısı; 0 = kombo yok.</summary>
        public int Count { get; }

        /// <summary>Jeton değeri çarpanı (1, 1.5, 2, 3).</summary>
        public float Multiplier { get; }

        /// <summary>Pencerenin kalan oranı: 1 = yeni öldürme, 0 = bitti.</summary>
        public float WindowRatio { get; }
    }
}
