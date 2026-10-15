namespace LastHaven.Core
{
    /// <summary>
    /// Run'ın cüzdanı: para ve yakıt (GDD §10). Economy'deki <c>Wallet</c>; kaydı [Systems]/Economy'deki <c>WalletService</c>
    /// Awake'te yapar. Değişiklikler <see cref="MoneyChangedEvent"/> / <see cref="FuelChangedEvent"/> olarak duyurulur.
    /// Miktarlar tam sayıdır; negatif miktar programlama hatasıdır (<see cref="System.ArgumentOutOfRangeException"/>).
    /// </summary>
    public interface IWallet
    {
        /// <summary>Mevcut para.</summary>
        int Money { get; }

        /// <summary>Mevcut yakıt (birim).</summary>
        int Fuel { get; }

        /// <summary>Kaynağın mevcut miktarı.</summary>
        int Get(WalletResource resource);

        /// <summary>Miktar karşılanabiliyor mu (mevcut ≥ miktar). 0 her zaman karşılanır.</summary>
        bool CanAfford(WalletResource resource, int amount);

        /// <summary>Kaynağa ekler. 0 hiçbir şey yapmaz (olay gönderilmez).</summary>
        void Add(WalletResource resource, int amount);

        /// <summary>Yetiyorsa düşer ve true döner; yetmiyorsa hiçbir şey değişmez ve false döner. 0 her zaman true (olaysız).</summary>
        bool TrySpend(WalletResource resource, int amount);
    }
}
