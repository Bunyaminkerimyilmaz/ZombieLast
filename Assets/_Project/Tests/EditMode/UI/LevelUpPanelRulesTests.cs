using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Seviye kartlarının açılma kuralı (13.3): sadece oyun modunda, başka duraklatma yokken ve run sürerken.</summary>
    public class LevelUpPanelRulesTests
    {
        [TestCase(GamePhase.Day)]
        [TestCase(GamePhase.Dusk)]
        [TestCase(GamePhase.Night)]
        [TestCase(GamePhase.Dawn)]
        public void CanOpen_GameplayDuringRun(GamePhase phase)
        {
            Assert.IsTrue(LevelUpPanelRules.CanOpen(InputMode.Gameplay, false, true, phase));
        }

        [TestCase(InputMode.BuildMode)]
        [TestCase(InputMode.UI)]
        public void CanOpen_NotInBuildModeOrMenu(InputMode mode)
        {
            Assert.IsFalse(LevelUpPanelRules.CanOpen(mode, false, true, GamePhase.Day),
                "İnşa modu / market açıkken bekler; Enter / Esc onlarla çakışır.");
        }

        [Test]
        public void CanOpen_NotWhilePausedByAnotherSource()
        {
            Assert.IsFalse(LevelUpPanelRules.CanOpen(InputMode.Gameplay, true, true, GamePhase.Night));
        }

        [TestCase(GamePhase.Boot)]
        [TestCase(GamePhase.GameOver)]
        [TestCase(GamePhase.Victory)]
        public void CanOpen_NotOutsideRun(GamePhase phase)
        {
            Assert.IsFalse(LevelUpPanelRules.CanOpen(InputMode.Gameplay, false, true, phase));
            Assert.IsFalse(LevelUpPanelRules.IsRunActive(phase));
        }

        [Test]
        public void CanOpen_WithoutGameFlow_IgnoresPhase()
        {
            Assert.IsTrue(LevelUpPanelRules.CanOpen(InputMode.Gameplay, false, false, GamePhase.GameOver));
        }

        [Test]
        public void CanOpen_FromServices_ReadsPauseState()
        {
            var pause = new FakePauseService();

            Assert.IsTrue(LevelUpPanelRules.CanOpen(InputMode.Gameplay, pause, null));
            pause.Pause(this);
            Assert.IsFalse(LevelUpPanelRules.CanOpen(InputMode.Gameplay, pause, null), "Duraklatma menüsü açıkken kartlar bekler.");
            Assert.IsTrue(LevelUpPanelRules.CanOpen(InputMode.Gameplay, null, null), "Servis yoksa duraklı sayılmaz.");
        }
    }
}
