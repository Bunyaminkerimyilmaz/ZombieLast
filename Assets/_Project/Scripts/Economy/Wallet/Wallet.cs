using System;
using LastHaven.Core;

namespace LastHaven.Economy
{
    /// <summary>
    /// Para ve yakıt cüzdanı (saf C#; GDD §10). Değişiklikler C# olaylarıyla bildirilir; <see cref="WalletService"/> bunları
    /// EventBus'a çevirir. Miktarlar int.MaxValue'da doyar. Negatif miktar <see cref="ArgumentOutOfRangeException"/> fırlatır.
    /// </summary>
    public sealed class Wallet : IWallet
    {
        private int _money;
        private int _fuel;

        public Wallet(int startingMoney, int startingFuel)
        {
            RequireNonNegative(startingMoney, nameof(startingMoney));
            RequireNonNegative(startingFuel, nameof(startingFuel));
            _money = startingMoney;
            _fuel = startingFuel;
        }

        /// <summary>Para değişti: (yeni miktar, değişim).</summary>
        public event Action<int, int> MoneyChanged;

        /// <summary>Yakıt değişti: (yeni miktar, değişim).</summary>
        public event Action<int, int> FuelChanged;

        /// <inheritdoc />
        public int Money => _money;

        /// <inheritdoc />
        public int Fuel => _fuel;

        /// <inheritdoc />
        public int Get(WalletResource resource)
        {
            return resource switch
            {
                WalletResource.Money => _money,
                WalletResource.Fuel => _fuel,
                _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
            };
        }

        /// <inheritdoc />
        public bool CanAfford(WalletResource resource, int amount)
        {
            RequireNonNegative(amount, nameof(amount));
            return Get(resource) >= amount;
        }

        /// <inheritdoc />
        public void Add(WalletResource resource, int amount)
        {
            RequireNonNegative(amount, nameof(amount));
            int current = Get(resource);
            if (amount == 0)
            {
                return;
            }

            int updated = (int)Math.Min((long)current + amount, int.MaxValue);
            Set(resource, updated, updated - current);
        }

        /// <inheritdoc />
        public bool TrySpend(WalletResource resource, int amount)
        {
            if (!CanAfford(resource, amount))
            {
                return false;
            }

            if (amount > 0)
            {
                Set(resource, Get(resource) - amount, -amount);
            }

            return true;
        }

        private void Set(WalletResource resource, int value, int delta)
        {
            if (delta == 0)
            {
                return;
            }

            if (resource == WalletResource.Money)
            {
                _money = value;
                MoneyChanged?.Invoke(value, delta);
            }
            else
            {
                _fuel = value;
                FuelChanged?.Invoke(value, delta);
            }
        }

        private static void RequireNonNegative(int amount, string paramName)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, amount, "Miktar negatif olamaz.");
            }
        }
    }
}
