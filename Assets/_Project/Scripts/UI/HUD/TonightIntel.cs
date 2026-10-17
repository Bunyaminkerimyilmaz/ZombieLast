using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD istihbaratının kaynağı (GDD §9): evre istihbarat gösteriyorsa (<see cref="HudPhaseRules.ShowsIntel"/>) ve gece indeksi
    /// geçerliyse bu gecenin bilgisi (<see cref="IWaveService.GetTonightInfo"/>), değilse null.
    /// </summary>
    public static class TonightIntel
    {
        public static NightInfo Get(IWaveService waves, GamePhase phase, int nightIndex)
        {
            if (waves == null || !HudPhaseRules.ShowsIntel(phase) || nightIndex < 0 || nightIndex >= waves.TotalNights)
            {
                return null;
            }

            return waves.GetTonightInfo(nightIndex);
        }
    }
}
