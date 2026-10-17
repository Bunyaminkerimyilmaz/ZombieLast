using LastHaven.Enemies;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>BossRage (GDD §7: %50 can altında öfke): eşik katı, bir kez, ölüyken yok, iyileşince sönmez, sıfırlama.</summary>
    public class BossRageTests
    {
        private const float Threshold = 0.5f;

        [Test]
        public void AboveOrAtThreshold_NoRage()
        {
            var rage = new BossRage();

            Assert.IsFalse(rage.TryEnrage(0.9f, Threshold, isAlive: true));
            Assert.IsFalse(rage.TryEnrage(0.5f, Threshold, isAlive: true), "Tam %50 'altında' değil.");
            Assert.IsFalse(rage.IsEnraged);
        }

        [Test]
        public void BelowThreshold_EnragesOnce_AndStays()
        {
            var rage = new BossRage();

            Assert.IsTrue(rage.TryEnrage(0.49f, Threshold, isAlive: true));
            Assert.IsTrue(rage.IsEnraged);
            Assert.IsFalse(rage.TryEnrage(0.3f, Threshold, isAlive: true), "İkinci kez tetiklenmez.");
            Assert.IsFalse(rage.TryEnrage(0.8f, Threshold, isAlive: true));
            Assert.IsTrue(rage.IsEnraged, "İyileşse de öfkeli kalır.");
        }

        [Test]
        public void Dead_DoesNotEnrage_ResetClears()
        {
            var rage = new BossRage();

            Assert.IsFalse(rage.TryEnrage(0f, Threshold, isAlive: false), "Öldüren tek vuruş öfkelendirmez.");

            rage.TryEnrage(0.2f, Threshold, isAlive: true);
            rage.Reset();

            Assert.IsFalse(rage.IsEnraged);
            Assert.IsTrue(rage.TryEnrage(0.2f, Threshold, isAlive: true), "Yeni doğuşta yeniden öfkelenebilir.");
        }
    }
}
