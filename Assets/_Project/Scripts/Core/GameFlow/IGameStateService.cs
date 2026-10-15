namespace LastHaven.Core
{
    /// <summary>
    /// Oyun akışı servisi (GameFlow'daki <c>GameFlowController</c>, [Systems]/GameFlow). Evre değişimleri
    /// <see cref="PhaseChangedEvent"/> ile duyurulur; bu servis o anki durumu okumak içindir (HUD, market, inşa modu).
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>Şu anki evre. Akış ilk karede başlar; o zamana kadar <see cref="GamePhase.Boot"/>.</summary>
        GamePhase CurrentPhase { get; }

        /// <summary>
        /// 0 tabanlı gece indeksi: gündüz ve alacakaranlıkta yaklaşan gece, gece ve şafakta sürmekte olan / biten gece.
        /// Şafaktan gündüze geçince bir artar. Görünen numara = indeks + 1.
        /// </summary>
        int NightIndex { get; }

        /// <summary>Süreli evrede (Day, Dusk) kalan süre (sn); süresiz evrelerde 0.</summary>
        float PhaseTimeRemaining { get; }
    }
}
