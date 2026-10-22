using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary><see cref="BuildModeEntryRule"/>: inşa modu sadece gündüz ve oyuncu canlıyken açılır (GDD §11, 15.3).</summary>
    public class BuildModeEntryRuleTests
    {
        [Test]
        public void Evaluate_DayAndAlive_IsAllowed()
        {
            Assert.AreEqual(BuildModeEntry.Allowed, BuildModeEntryRule.Evaluate(GamePhase.Day, playerAlive: true));
        }

        [TestCase(GamePhase.Dusk)]
        [TestCase(GamePhase.Night)]
        [TestCase(GamePhase.Dawn)]
        public void Evaluate_NotDay_IsRejected(GamePhase phase)
        {
            Assert.AreEqual(BuildModeEntry.NotDay, BuildModeEntryRule.Evaluate(phase, playerAlive: true));
        }

        [Test]
        public void Evaluate_DeadPlayerDuringDay_IsRejected()
        {
            Assert.AreEqual(BuildModeEntry.PlayerDead, BuildModeEntryRule.Evaluate(GamePhase.Day, playerAlive: false));
        }

        [Test]
        public void Evaluate_NotDayAndDead_ReportsPhaseFirst()
        {
            Assert.AreEqual(BuildModeEntry.NotDay, BuildModeEntryRule.Evaluate(GamePhase.Night, playerAlive: false));
        }

        [Test]
        public void Evaluate_NoGameFlow_OnlyChecksPlayer()
        {
            Assert.AreEqual(BuildModeEntry.Allowed, BuildModeEntryRule.Evaluate(null, playerAlive: true));
            Assert.AreEqual(BuildModeEntry.PlayerDead, BuildModeEntryRule.Evaluate(null, playerAlive: false));
        }

        [Test]
        public void Describe_RejectionsHaveText_AllowedIsEmpty()
        {
            Assert.IsNotEmpty(BuildModeEntryRule.Describe(BuildModeEntry.NotDay));
            Assert.IsNotEmpty(BuildModeEntryRule.Describe(BuildModeEntry.PlayerDead));
            Assert.IsEmpty(BuildModeEntryRule.Describe(BuildModeEntry.Allowed));
        }
    }
}
