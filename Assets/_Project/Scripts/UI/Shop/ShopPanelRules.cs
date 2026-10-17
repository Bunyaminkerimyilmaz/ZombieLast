using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>Market panelinin ne zaman açılıp kapandığı (saf kural; GDD §10, §16).</summary>
    public static class ShopPanelRules
    {
        /// <summary>
        /// Tab ile açılabilir mi: sadece oyun modundayken (inşa modunda veya başka bir menü açıkken değil) ve gündüz. Oyun akışı
        /// yoksa (<paramref name="hasGameState"/> false) evreden bağımsız açılır; market servisi de öyle satar.
        /// </summary>
        public static bool CanOpen(InputMode mode, bool hasGameState, GamePhase phase)
        {
            return mode == InputMode.Gameplay && (!hasGameState || StaysOpen(phase));
        }

        /// <summary>Açık panel bu evrede açık kalır mı; gündüz bitince (alacakaranlık ve sonrası) kapanır.</summary>
        public static bool StaysOpen(GamePhase phase)
        {
            return phase == GamePhase.Day;
        }
    }
}
