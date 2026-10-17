using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>NightCoinModifier (GDD §14 Kan Ayı: jeton ×2): geçici CoinValue yüzdesi eklenir, olay bitince kalkar; çarpan 1 ise eklenmez.</summary>
    public class NightCoinModifierTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void BloodMoon_DoublesCoinValue_UntilRemoved()
        {
            var modifiers = new RunModifiers();
            NightModifierSettings bloodMoon = Settings("blood_moon", 2f);

            Assert.IsTrue(NightCoinModifier.Apply(modifiers, bloodMoon));
            Assert.AreEqual(2f, modifiers.Get(StatType.CoinValue, 1f), Tolerance);

            Assert.AreEqual(1, NightCoinModifier.Remove(modifiers, bloodMoon));
            Assert.AreEqual(1f, modifiers.Get(StatType.CoinValue, 1f), Tolerance);
        }

        [Test]
        public void Apply_Twice_DoesNotStack_AndKeepsPerks()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.CoinValue, 0.25f, "perk_greedy"));
            NightModifierSettings bloodMoon = Settings("blood_moon", 2f);

            NightCoinModifier.Apply(modifiers, bloodMoon);
            NightCoinModifier.Apply(modifiers, bloodMoon);

            // Yüzdeler toplanır (IRunModifiers): 1 + 0.25 + 1.
            Assert.AreEqual(2.25f, modifiers.Get(StatType.CoinValue, 1f), Tolerance);

            NightCoinModifier.Remove(modifiers, bloodMoon);
            Assert.AreEqual(1.25f, modifiers.Get(StatType.CoinValue, 1f), Tolerance, "Açgözlü perk'i kalır.");
        }

        [Test]
        public void NoCoinMultiplier_AddsNothing()
        {
            var modifiers = new RunModifiers();

            Assert.IsFalse(NightCoinModifier.Apply(modifiers, Settings("fog", 1f)));
            Assert.AreEqual(1f, modifiers.Get(StatType.CoinValue, 1f), Tolerance);
            Assert.IsFalse(NightCoinModifier.Apply(null, Settings("fog", 2f)));
        }

        private static NightModifierSettings Settings(string id, float coinMultiplier)
        {
            return new NightModifierSettings(id, id, string.Empty, 1f, 1f, coinMultiplier, false, Color.black, 0f, false,
                Color.black, Color.black, false, 25f, 5f);
        }
    }
}
