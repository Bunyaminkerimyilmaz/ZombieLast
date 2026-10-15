using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>
    /// Bir yapının onarım turlarını uygular (<see cref="StructureRepair"/>'ın parçası): <see cref="RepairRule"/>'a göre parayı
    /// cüzdandan düşer, canı artırır ve kesintisiz onarımın toplam harcamasını tutar. Cüzdan yoksa onarım ücretsizdir.
    /// </summary>
    public sealed class RepairSession
    {
        private readonly IWallet _wallet;

        /// <param name="wallet">Null olabilir (ücretsiz onarım).</param>
        public RepairSession(IWallet wallet)
        {
            _wallet = wallet;
        }

        /// <summary>Bu oturumda harcanan para (<see cref="Reset"/>'e kadar).</summary>
        public int Spent { get; private set; }

        /// <summary>Bir tur: yetiyorsa 1 para alır ve 5 cana kadar onarır. Para yetmezse hiçbir şey değişmez.</summary>
        public RepairStepResult Step(Health health, in RepairSettings settings)
        {
            int money = _wallet != null ? _wallet.Money : int.MaxValue;
            RepairStepResult result = RepairRule.Evaluate(health.Current, health.Max, health.IsAlive, money, settings,
                out float heal, out int cost);
            if (result != RepairStepResult.Repaired)
            {
                return result;
            }

            if (_wallet != null && !_wallet.TrySpend(WalletResource.Money, cost))
            {
                return RepairStepResult.NotEnoughMoney;
            }

            health.Heal(heal);
            Spent += cost;
            return RepairStepResult.Repaired;
        }

        public void Reset()
        {
            Spent = 0;
        }
    }
}
