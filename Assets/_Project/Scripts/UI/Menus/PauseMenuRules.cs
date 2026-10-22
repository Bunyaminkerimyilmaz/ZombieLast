using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>Duraklatma menüsünün ne zaman açıldığı (saf kural; GDD §4 Esc / Start, §16).</summary>
    public static class PauseMenuRules
    {
        /// <summary>
        /// Esc / Start ile açılabilir mi: oyun veya inşa modunda (market, seviye kartları, run sonu gibi bir menü açıkken girdi UI'dadır;
        /// onların Esc'i kendilerini kapatır), oyun başka bir kaynakça duraklatılmamışken ve run sürerken (Boot / Oyun Bitti / Zafer
        /// değil). Oyun akışı yoksa (<paramref name="hasGameState"/> false) evreye bakılmaz.
        /// </summary>
        public static bool CanOpen(InputMode mode, bool isPaused, bool hasGameState, GamePhase phase)
        {
            return (mode == InputMode.Gameplay || mode == InputMode.BuildMode) && !isPaused
                   && (!hasGameState || LevelUpPanelRules.IsRunActive(phase));
        }

        /// <summary>Servislerden okur: duraklatma servisi yoksa duraklı sayılmaz, oyun akışı yoksa evreye bakılmaz.</summary>
        public static bool CanOpen(InputMode mode, IPauseService pause, IGameStateService gameState)
        {
            bool paused = pause != null && pause.IsPaused;
            GamePhase phase = gameState != null ? gameState.CurrentPhase : GamePhase.Day;
            return CanOpen(mode, paused, gameState != null, phase);
        }
    }
}
