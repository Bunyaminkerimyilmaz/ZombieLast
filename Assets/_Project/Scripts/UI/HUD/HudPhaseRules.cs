using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>Evreye göre HUD'da ne görünür (saf kural).</summary>
    public static class HudPhaseRules
    {
        /// <summary>Gündüz zamanlayıcısı sadece gündüzde (sayaç <see cref="DayTimerTickEvent"/> ile gelir).</summary>
        public static bool ShowsDayTimer(GamePhase phase)
        {
            return phase == GamePhase.Day;
        }

        /// <summary>Kalan düşman sadece gece.</summary>
        public static bool ShowsRemainingEnemies(GamePhase phase)
        {
            return phase == GamePhase.Night;
        }

        /// <summary>Evre adını taşıyan UXML etiketinin adı; açılışta (Boot) evre adı yok (null).</summary>
        public static string PhaseLabelName(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Day:
                    return HudElementNames.PhaseDay;
                case GamePhase.Dusk:
                    return HudElementNames.PhaseDusk;
                case GamePhase.Night:
                    return HudElementNames.PhaseNight;
                case GamePhase.Dawn:
                    return HudElementNames.PhaseDawn;
                case GamePhase.GameOver:
                    return HudElementNames.PhaseGameOver;
                case GamePhase.Victory:
                    return HudElementNames.PhaseVictory;
                default:
                    return null;
            }
        }
    }
}
