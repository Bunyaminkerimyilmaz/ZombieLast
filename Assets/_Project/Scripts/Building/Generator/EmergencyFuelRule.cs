using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>Acil yakıt alımının sonucu.</summary>
    public enum EmergencyFuelResult
    {
        Purchased = 0,

        /// <summary>Depoda alınacak miktar kadar yer yok; hiçbir şey değişmedi.</summary>
        TankFull = 1,

        /// <summary>Para yetmedi; hiçbir şey değişmedi.</summary>
        NotEnoughMoney = 2,
    }

    /// <summary>
    /// Acil yakıt alımı (saf; GDD §12: 30 para → 10 yakıt). Depoda tam miktar kadar yer yoksa sunulmaz (para boşa gitmesin).
    /// </summary>
    public static class EmergencyFuelRule
    {
        /// <summary>Depoya <paramref name="amount"/> birim sığıyor mu.</summary>
        public static bool HasRoom(int fuel, int capacity, int amount)
        {
            return amount > 0 && (long)fuel + amount <= capacity;
        }

        /// <summary>Yer ve para yetiyorsa parayı düşer, yakıtı ekler. Değilse cüzdan değişmez.</summary>
        public static EmergencyFuelResult TryBuy(IWallet wallet, int price, int amount)
        {
            if (!HasRoom(wallet.Fuel, wallet.GetCapacity(WalletResource.Fuel), amount))
            {
                return EmergencyFuelResult.TankFull;
            }

            if (!wallet.TrySpend(WalletResource.Money, price))
            {
                return EmergencyFuelResult.NotEnoughMoney;
            }

            wallet.Add(WalletResource.Fuel, amount);
            return EmergencyFuelResult.Purchased;
        }
    }
}
