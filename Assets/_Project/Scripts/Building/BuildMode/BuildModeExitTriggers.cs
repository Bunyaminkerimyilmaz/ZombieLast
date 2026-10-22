using System;
using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa modunu kendiliğinden kapatan olaylar: gündüz biterse (alacakaranlık ve sonrası) ve oyuncu ölürse (15.3).
    /// <see cref="BuildModeController"/> OnEnable/OnDisable'da abone eder; kapatma (mod açıksa) controller'ın işi.
    /// </summary>
    public sealed class BuildModeExitTriggers
    {
        private readonly Action<string> _exit;

        /// <param name="exit">Mod açıksa kapat; argüman log için neden.</param>
        public BuildModeExitTriggers(Action<string> exit)
        {
            _exit = exit;
        }

        public void Subscribe()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
        }

        public void Unsubscribe()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (changed.Phase != GamePhase.Day)
            {
                _exit($"Gündüz bitti ({changed.Phase})");
            }
        }

        private void OnPlayerDied(PlayerDiedEvent died) => _exit("Oyuncu öldü");
    }
}
