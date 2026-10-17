using LastHaven.Combat;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Avcı (GDD §13): bayrak açık ve hedef yavaşlamışsa hasar × (1 + bonus), aksi halde ×1.</summary>
    public class HunterBonusTests
    {
        [TestCase(true, true, 0.5f, 1.5f)]
        [TestCase(false, true, 0.5f, 1f)]
        [TestCase(true, false, 0.5f, 1f)]
        [TestCase(false, false, 0.5f, 1f)]
        [TestCase(true, true, -0.5f, 1f)]
        public void GetMultiplier(bool hunterActive, bool targetSlowed, float bonus, float expected)
        {
            Assert.AreEqual(expected, HunterBonus.GetMultiplier(hunterActive, targetSlowed, bonus), 0.0001f);
        }
    }
}
