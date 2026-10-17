using LastHaven.Enemies;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// SlowEffects (9.3): en güçlü yavaşlatma geçerli, zayıf ama uzun olan güçlüsü bitince devreye girer, aynı oran süreyi uzatır
    /// (kısaltmaz), kapsanan kayıt tutulmaz, kırpma ve temizleme.
    /// </summary>
    public class SlowEffectsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Empty_IsNotSlowed()
        {
            var slows = new SlowEffects();

            Assert.IsFalse(slows.IsSlowed);
            Assert.AreEqual(0f, slows.Current);
        }

        [Test]
        public void StrongestApplies_NotSum()
        {
            var slows = new SlowEffects();

            slows.Apply(0.25f, 1f);
            slows.Apply(0.4f, 0.5f);

            Assert.IsTrue(slows.IsSlowed);
            Assert.AreEqual(0.4f, slows.Current, Tolerance, "Oranlar toplanmaz; en güçlüsü.");
        }

        [Test]
        public void WeakerLongerSlow_TakesOverWhenStrongerExpires()
        {
            var slows = new SlowEffects();
            slows.Apply(0.4f, 0.5f);
            slows.Apply(0.25f, 1f);

            slows.Tick(0.6f);

            Assert.AreEqual(0.25f, slows.Current, Tolerance, "Tuzak bitti, Şok Taret sürüyor.");

            slows.Tick(0.5f);

            Assert.IsFalse(slows.IsSlowed);
            Assert.AreEqual(0, slows.Count);
        }

        [Test]
        public void SameFraction_ExtendsButNeverShortens()
        {
            var slows = new SlowEffects();
            slows.Apply(0.4f, 1f);
            slows.Tick(0.6f); // kalan 0.4

            slows.Apply(0.4f, 1f); // yenilenir
            slows.Tick(0.9f);
            Assert.IsTrue(slows.IsSlowed, "Tuzakta her tik süreyi tazeler.");

            slows.Apply(0.4f, 0.05f); // kalan 0.1'den kısa: değişmez
            slows.Tick(0.08f);
            Assert.IsTrue(slows.IsSlowed);
            Assert.AreEqual(1, slows.Count);
        }

        [Test]
        public void DominatedSlow_IsNotStored_AndDominatingReplaces()
        {
            var slows = new SlowEffects();
            slows.Apply(0.4f, 1f);

            slows.Apply(0.2f, 0.5f);
            Assert.AreEqual(1, slows.Count, "Hem zayıf hem kısa: hiçbir an geçerli olmaz.");

            slows.Apply(0.6f, 2f);
            Assert.AreEqual(1, slows.Count, "Hem güçlü hem uzun: eskisini kapsar.");
            Assert.AreEqual(0.6f, slows.Current, Tolerance);
        }

        [Test]
        public void Apply_ClampsFraction_AndIgnoresZero()
        {
            var slows = new SlowEffects();

            slows.Apply(0f, 1f);
            slows.Apply(0.5f, 0f);
            Assert.IsFalse(slows.IsSlowed);

            slows.Apply(1.5f, 1f);
            Assert.AreEqual(1f, slows.Current, Tolerance);
        }

        [Test]
        public void Clear_RemovesAll()
        {
            var slows = new SlowEffects();
            slows.Apply(0.4f, 1f);
            slows.Apply(0.2f, 3f);

            slows.Clear();

            Assert.IsFalse(slows.IsSlowed);
            Assert.AreEqual(0, slows.Count);
        }
    }
}
