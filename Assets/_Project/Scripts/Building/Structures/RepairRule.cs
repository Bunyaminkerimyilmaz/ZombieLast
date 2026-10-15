using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>Onarımın hız ve maliyeti (GDD §11: 50 can/sn, 5 can başına 1 para); <see cref="RepairConfig.Settings"/>'ten gelir.</summary>
    public readonly struct RepairSettings
    {
        public RepairSettings(float healthPerSecond, float healthPerMoney)
        {
            HealthPerSecond = healthPerSecond;
            HealthPerMoney = healthPerMoney;
        }

        /// <summary>Onarım hızı (can/sn).</summary>
        public float HealthPerSecond { get; }

        /// <summary>Bir turda (1 para) onarılan can.</summary>
        public float HealthPerMoney { get; }

        /// <summary>Bir turun süresi (sn): 5 can / 50 can/sn = 0.1 sn. Ayarlar geçersizse 0.</summary>
        public float CycleDuration => HealthPerSecond > 0f ? HealthPerMoney / HealthPerSecond : 0f;
    }

    /// <summary>Bir onarım turunun sonucu.</summary>
    public enum RepairStepResult
    {
        /// <summary>1 para alındı, can arttı.</summary>
        Repaired,

        /// <summary>Can zaten tam; bir şey yapılmadı.</summary>
        AlreadyFull,

        /// <summary>Para yetmedi; onarım durur (GDD §11).</summary>
        NotEnoughMoney,

        /// <summary>Yapı yıkılmış; onarılamaz.</summary>
        Destroyed,
    }

    /// <summary>
    /// Onarım hesapları (saf). Onarım turlarla yapılır: her tur 1 para alır ve <see cref="RepairSettings.HealthPerMoney"/> kadar
    /// (eksik can azsa o kadar) onarır. Böylece para tam sayı kalır ve "5 can başına 1 para" birebir uygulanır.
    /// </summary>
    public static class RepairRule
    {
        /// <summary>Bir turun parası.</summary>
        public const int CostPerCycle = 1;

        // 299.99999 gibi float kalıntıları fazladan bir tur saydırmasın.
        private const float MissingHealthEpsilon = 0.001f;

        /// <summary>Eksik canı tamamen onarmanın parası (yukarı yuvarlı; 12 eksik can → 3 para). Tam canda 0.</summary>
        public static int CostToFull(float current, float max, in RepairSettings settings)
        {
            float missing = max - current;
            if (missing <= MissingHealthEpsilon || settings.HealthPerMoney <= 0f)
            {
                return 0;
            }

            return Mathf.CeilToInt((missing - MissingHealthEpsilon) / settings.HealthPerMoney) * CostPerCycle;
        }

        /// <summary>Onarılacak can var mı.</summary>
        public static bool NeedsRepair(float current, float max)
        {
            return max - current > MissingHealthEpsilon;
        }

        /// <summary>
        /// Bir turu değerlendirir (hiçbir şeyi değiştirmez): <see cref="RepairStepResult.Repaired"/> ise çağıran
        /// <paramref name="cost"/>'u öder ve <paramref name="heal"/> kadar iyileştirir.
        /// </summary>
        public static RepairStepResult Evaluate(float current, float max, bool isAlive, int money, in RepairSettings settings,
            out float heal, out int cost)
        {
            heal = 0f;
            cost = 0;
            if (!isAlive)
            {
                return RepairStepResult.Destroyed;
            }

            if (!NeedsRepair(current, max))
            {
                return RepairStepResult.AlreadyFull;
            }

            if (money < CostPerCycle)
            {
                return RepairStepResult.NotEnoughMoney;
            }

            heal = Mathf.Min(settings.HealthPerMoney, max - current);
            cost = CostPerCycle;
            return RepairStepResult.Repaired;
        }
    }
}
