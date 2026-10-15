using System;
using LastHaven.Core;

namespace LastHaven.Economy
{
    /// <summary>
    /// Cüzdana akış dışı gelirler (saf): şafak faizi (<see cref="InterestRule"/>) ve erken hazır bonusu (miktarı GameFlow
    /// hesaplar, <see cref="EarlyReadyEvent.BonusMoney"/>). <see cref="WalletService"/> olaylardan çağırır.
    /// </summary>
    public sealed class WalletIncome
    {
        private readonly IWallet _wallet;

        public WalletIncome(IWallet wallet, InterestRule interest)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            Interest = interest;
        }

        public InterestRule Interest { get; }

        /// <summary>Mevcut paranın faizini ekler; eklenen miktarı döner (0 olabilir).</summary>
        public int PayInterest()
        {
            int interest = Interest.Calculate(_wallet.Money);
            _wallet.Add(WalletResource.Money, interest);
            return interest;
        }

        /// <summary>Erken hazır bonusunu ekler; eklenen miktarı döner. 0 ve negatif yok sayılır.</summary>
        public int AddEarlyReadyBonus(int bonusMoney)
        {
            if (bonusMoney <= 0)
            {
                return 0;
            }

            _wallet.Add(WalletResource.Money, bonusMoney);
            return bonusMoney;
        }
    }
}
