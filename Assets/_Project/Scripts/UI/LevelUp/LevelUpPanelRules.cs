using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>Seviye kartlarının ne zaman açılıp kapandığı (saf kural; GDD §13).</summary>
    public static class LevelUpPanelRules
    {
        /// <summary>
        /// Bekleyen seviyenin kartları şimdi açılabilir mi: oyun modunda (inşa modu, market veya başka bir menü açıkken değil: onların
        /// Enter / Esc'i UI haritasıyla çakışır; kartlar menü kapanınca gelir), oyun başka bir kaynakça duraklatılmamışken ve run
        /// sürerken. Oyun akışı yoksa (<paramref name="hasGameState"/> false) evreye bakılmaz.
        /// </summary>
        public static bool CanOpen(InputMode mode, bool isPaused, bool hasGameState, GamePhase phase)
        {
            return mode == InputMode.Gameplay && !isPaused && (!hasGameState || IsRunActive(phase));
        }

        /// <summary>
        /// <see cref="CanOpen(InputMode, bool, bool, GamePhase)"/>'i servislerden okur: duraklatma servisi yoksa duraklı sayılmaz, oyun
        /// akışı yoksa evreye bakılmaz.
        /// </summary>
        public static bool CanOpen(InputMode mode, IPauseService pause, IGameStateService gameState)
        {
            bool paused = pause != null && pause.IsPaused;
            GamePhase phase = gameState != null ? gameState.CurrentPhase : GamePhase.Day;
            return CanOpen(mode, paused, gameState != null, phase);
        }

        /// <summary>Run sürüyor mu: akış başladı ve oyun bitmedi (Oyun Bitti / Zafer'de açık kartlar kapanır).</summary>
        public static bool IsRunActive(GamePhase phase)
        {
            return phase != GamePhase.Boot && phase != GamePhase.GameOver && phase != GamePhase.Victory;
        }
    }
}
