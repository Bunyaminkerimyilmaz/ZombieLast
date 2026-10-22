using LastHaven.Core;
using LastHaven.Player;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Oyuncu perk hesapları (13.2, GDD §13): Kalın Deri maks. canı, Adrenalin atılma bekleme süresi.</summary>
    public class PlayerPerkMathTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void MaxHealth_WithoutModifiers_IsBase()
        {
            Assert.AreEqual(100f, PlayerPerkEffects.CalculateMaxHealth(100f, null), Tolerance);
            Assert.AreEqual(100f, PlayerPerkEffects.CalculateMaxHealth(100f, new RunModifiers()), Tolerance);
        }

        [Test]
        public void MaxHealth_ThickSkinStacks_AddFlat()
        {
            var modifiers = new RunModifiers();
            for (int i = 0; i < 3; i++)
            {
                modifiers.AddModifier(StatModifier.Flat(StatType.PlayerMaxHealth, 25f, "perk:thick_skin"));
            }

            Assert.AreEqual(175f, PlayerPerkEffects.CalculateMaxHealth(100f, modifiers), Tolerance, "3 yığın: 100 + 75.");
        }

        [Test]
        public void MaxHealth_NeverDropsBelowOne()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.PlayerMaxHealth, -2f, "test"));

            Assert.AreEqual(1f, PlayerPerkEffects.CalculateMaxHealth(100f, modifiers), Tolerance, "Health.SetMax pozitif ister.");
        }

        [Test]
        public void DashCooldown_Adrenaline_IsFortyPercentShorter()
        {
            var modifiers = new RunModifiers();
            Assert.AreEqual(2f, PlayerMotor.CalculateDashCooldown(2f, null), Tolerance);
            Assert.AreEqual(2f, PlayerMotor.CalculateDashCooldown(2f, modifiers), Tolerance);

            modifiers.AddModifier(StatModifier.Percent(StatType.DashCooldown, -0.4f, "perk:adrenaline"));

            Assert.AreEqual(1.2f, PlayerMotor.CalculateDashCooldown(2f, modifiers), Tolerance);
        }

        [Test]
        public void DashCooldown_NeverNegative()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Flat(StatType.DashCooldown, -5f, "test"));

            Assert.AreEqual(0f, PlayerMotor.CalculateDashCooldown(2f, modifiers), Tolerance);
        }
    }
}
