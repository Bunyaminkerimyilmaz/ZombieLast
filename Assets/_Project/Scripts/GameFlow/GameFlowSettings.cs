using System;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// <see cref="GameStateMachine"/>'in süre ve bonus ayarları (saf; testler doğrudan kurar). Oyunda
    /// <see cref="GameConfig.CreateFlowSettings"/>'ten gelir. Negatif süreler 0 sayılır.
    /// </summary>
    public readonly struct GameFlowSettings
    {
        public GameFlowSettings(
            float dayDuration,
            float duskDuration,
            float finalWarningDuration,
            int earlyReadyBonusPerStep,
            float earlyReadyStepSeconds)
        {
            DayDuration = Math.Max(0f, dayDuration);
            DuskDuration = Math.Max(0f, duskDuration);
            FinalWarningDuration = Math.Max(0f, finalWarningDuration);
            EarlyReadyBonusPerStep = Math.Max(0, earlyReadyBonusPerStep);
            EarlyReadyStepSeconds = Math.Max(0f, earlyReadyStepSeconds);
        }

        /// <summary>Gündüz süresi (sn).</summary>
        public float DayDuration { get; }

        /// <summary>Alacakaranlık süresi (sn).</summary>
        public float DuskDuration { get; }

        /// <summary>Gündüzün son uyarı süresi (sn); 0 ise uyarı yok.</summary>
        public float FinalWarningDuration { get; }

        /// <summary>Kalan her adım için bonus (para).</summary>
        public int EarlyReadyBonusPerStep { get; }

        /// <summary>Bonus adımı (sn); 0 ise bonus yok.</summary>
        public float EarlyReadyStepSeconds { get; }

        /// <summary>
        /// Erken hazır bonusu (GDD §9: kalan her 10 sn için +5 para). Kalan süre HUD'daki gibi tam saniyeye yukarı yuvarlanır;
        /// sonra tamamlanmış adımlar sayılır: 120 sn → 60, 110 sn → 55, 9.5 sn (HUD'da 10) → 5, 9 sn → 0.
        /// </summary>
        public int CalculateEarlyReadyBonus(float remainingSeconds)
        {
            if (EarlyReadyStepSeconds <= 0f || remainingSeconds <= 0f)
            {
                return 0;
            }

            double wholeSeconds = Math.Ceiling(remainingSeconds);
            return (int)Math.Floor(wholeSeconds / EarlyReadyStepSeconds) * EarlyReadyBonusPerStep;
        }
    }
}
