using LastHaven.Enemies;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>SpeedMultipliers: kaynaklar birbirini ezmez, etkili çarpan çarpımdır, sıfırlama ve negatif kırpma.</summary>
    public class SpeedMultipliersTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void New_IsOne()
        {
            var multipliers = new SpeedMultipliers();

            Assert.AreEqual(1f, multipliers.Product, Tolerance);
        }

        [Test]
        public void Sources_Multiply_AndDoNotOverwriteEachOther()
        {
            // GDD §7 Brute öfkesi ×1.4 + diken tuzağı %40 yavaşlatma (×0.6).
            var multipliers = new SpeedMultipliers();
            multipliers.Set(SpeedSource.Boss, 1.4f);
            multipliers.Set(SpeedSource.Status, 0.6f);

            Assert.AreEqual(0.84f, multipliers.Product, Tolerance);

            multipliers.Set(SpeedSource.Status, 1f); // Yavaşlama bitti.
            Assert.AreEqual(1.4f, multipliers.Product, Tolerance, "Öfke korunur.");
            Assert.AreEqual(1.4f, multipliers.Get(SpeedSource.Boss), Tolerance);
        }

        [Test]
        public void NightEvent_MultipliesWithOthers()
        {
            // GDD §14 Sis +%10, öfkeli Brute ×1.4, tuzak %40 yavaşlatma.
            var multipliers = new SpeedMultipliers();
            multipliers.Set(SpeedSource.NightEvent, 1.1f);
            multipliers.Set(SpeedSource.Boss, 1.4f);
            multipliers.Set(SpeedSource.Status, 0.6f);

            Assert.AreEqual(1.1f * 1.4f * 0.6f, multipliers.Product, Tolerance);
        }

        [Test]
        public void Negative_ClampsToZero_ResetRestoresOne()
        {
            var multipliers = new SpeedMultipliers();
            multipliers.Set(SpeedSource.Status, -0.5f);
            Assert.AreEqual(0f, multipliers.Product, Tolerance);

            multipliers.Set(SpeedSource.Boss, 1.4f);
            multipliers.Reset();

            Assert.AreEqual(1f, multipliers.Product, Tolerance);
            Assert.AreEqual(1f, multipliers.Get(SpeedSource.Boss), Tolerance);
        }
    }
}
