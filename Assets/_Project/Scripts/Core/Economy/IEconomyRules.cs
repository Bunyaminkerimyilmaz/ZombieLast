namespace LastHaven.Core
{
    /// <summary>
    /// Ekonomi ayarlarından (Economy'deki <c>EconomyConfig</c>) diğer modüllerin okuduğu para kuralları. Economy'yi referans
    /// alamayan modüller (ör. Building'deki satış) bunu ServiceLocator'dan alır; kaydı <c>WalletService</c> Awake'te yapar.
    /// </summary>
    public interface IEconomyRules
    {
        /// <summary>Yapıya harcanan toplam paranın satışta iade edilen kısmı (GDD §10: %50, aşağı yuvarlı). Negatif/0 → 0.</summary>
        int GetSellRefund(int totalSpent);
    }
}
