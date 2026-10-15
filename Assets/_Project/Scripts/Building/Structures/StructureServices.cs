using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// <see cref="Structure"/>'ın kullandığı servisler (ilk yerleşmede ServiceLocator'dan bir kez alınır). Eksik olan null kalır ve
    /// uyarı loglanır: hedef kaydı yoksa düşmanlar yapıyı görmez, cüzdan yoksa yükseltme ücretsizdir, ekonomi kuralları yoksa
    /// satış iade vermez.
    /// </summary>
    public sealed class StructureServices
    {
        private StructureServices(ITargetRegistry registry, IWallet wallet, IEconomyRules rules)
        {
            Registry = registry;
            Wallet = wallet;
            Rules = rules;
        }

        public ITargetRegistry Registry { get; }

        public IWallet Wallet { get; }

        public IEconomyRules Rules { get; }

        /// <param name="context">Uyarıların bağlamı (tıklanınca seçilen obje).</param>
        public static StructureServices Resolve(Object context)
        {
            if (!ServiceLocator.TryGet(out ITargetRegistry registry))
            {
                Debug.LogWarning("Structure: ITargetRegistry bulunamadı; düşmanlar yapıları hedefleyemeyecek.", context);
            }

            IEconomyRules rules = null;
            if (!ServiceLocator.TryGet(out IWallet wallet))
            {
                Debug.LogWarning("Structure: IWallet bulunamadı; yükseltme ücretsiz, satış iadesiz olacak.", context);
            }
            else if (!ServiceLocator.TryGet(out rules))
            {
                Debug.LogWarning("Structure: IEconomyRules bulunamadı; satış iade vermeyecek. Sahnede [Systems]/Economy var mı?", context);
            }

            return new StructureServices(registry, wallet, rules);
        }
    }
}
