using LastHaven.Enemies;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Zincirleme Son (13.2, GDD §13): sadece bayrak açıkken, sadece oyuncunun öldürmesi, olasılık altında patlar.</summary>
    public class ChainExplosionRuleTests
    {
        [Test]
        public void Explodes_OnlyWhenActive_KilledByPlayer_AndRollBelowChance()
        {
            Assert.IsTrue(ChainExplosionRule.ShouldExplode(true, true, 0.05f, 0.1f));
            Assert.IsFalse(ChainExplosionRule.ShouldExplode(true, true, 0.1f, 0.1f), "Sınır dahil değil: [0, 0.1) = %10.");
            Assert.IsFalse(ChainExplosionRule.ShouldExplode(false, true, 0f, 0.1f), "Bayrak kapalı.");
            Assert.IsFalse(ChainExplosionRule.ShouldExplode(true, false, 0f, 0.1f), "Taret/tuzak öldürmesi.");
        }

        [Test]
        public void ChanceZero_Never_ChanceOne_Always()
        {
            Assert.IsFalse(ChainExplosionRule.ShouldExplode(true, true, 0f, 0f));
            Assert.IsTrue(ChainExplosionRule.ShouldExplode(true, true, 0.999f, 1f));
        }
    }
}
