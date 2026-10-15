using LastHaven.Core;
using LastHaven.Player;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Oyuncunun mıknatıs yarıçapı (7.2): PlayerData (GDD §5: 3 m) × IRunModifiers PickupRadius.</summary>
    public class PickupRadiusTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void NoModifiers_IsBase()
        {
            Assert.AreEqual(3f, PlayerCharacter.CalculatePickupRadius(3f, null), Tolerance);
            Assert.AreEqual(3f, PlayerCharacter.CalculatePickupRadius(3f, new RunModifiers()), Tolerance);
        }

        [Test]
        public void MarketFlat_AndPerkPercent_Apply()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Flat(StatType.PickupRadius, 1f, "shop_magnet"));

            Assert.AreEqual(4f, PlayerCharacter.CalculatePickupRadius(3f, modifiers), Tolerance, "Market Mıknatıs +1 m.");

            modifiers.AddModifier(StatModifier.Percent(StatType.PickupRadius, 0.5f, "perk_magnet"));

            Assert.AreEqual(6f, PlayerCharacter.CalculatePickupRadius(3f, modifiers), Tolerance, "(3 + 1) × 1.5.");
        }

        [Test]
        public void OtherStats_DoNotAffect()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.CoinValue, 1f, "blood_moon"));

            Assert.AreEqual(3f, PlayerCharacter.CalculatePickupRadius(3f, modifiers), Tolerance);
        }
    }
}
