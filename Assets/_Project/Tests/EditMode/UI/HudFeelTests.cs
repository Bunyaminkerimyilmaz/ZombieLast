using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// HUD oyun hissinin saf mantığı (14.1): uçan yazı yuvaları (sabit havuz, dolunca en eskisi), hareket (büyüyüp inme, yükselme,
    /// sönme), hasara göre boy, sayı metni önbelleği ve evre başlığı sırası.
    /// </summary>
    public class HudFeelTests
    {
        private const float Tolerance = 0.0001f;

        private static readonly BannerTiming Timing = new BannerTiming(fadeIn: 0.2f, hold: 1f, fadeOut: 0.5f, startScale: 1.5f);

        [Test]
        public void Slots_AcquireFree_ExpireAfterLifetime()
        {
            var slots = new FloatingTextSlots(2);

            int first = slots.Acquire(1f);
            int second = slots.Acquire(2f);
            Assert.AreNotEqual(first, second);
            Assert.AreEqual(2, slots.ActiveCount);

            slots.Tick(1f);
            Assert.IsFalse(slots.IsActive(first), "Ömrü dolan yuva boşalır.");
            Assert.IsTrue(slots.IsActive(second));
            Assert.AreEqual(0.5f, slots.Progress(second), Tolerance);
            Assert.AreEqual(first, slots.Acquire(1f), "Boş yuva yeniden kullanılır.");
        }

        [Test]
        public void Slots_Full_ReusesOldest_PoolNeverGrows()
        {
            var slots = new FloatingTextSlots(2);
            int older = slots.Acquire(1f);
            slots.Tick(0.6f);
            slots.Acquire(1f);

            int reused = slots.Acquire(1f);

            Assert.AreEqual(older, reused, "Hepsi doluyken sonuna en yakın yuva verilir.");
            Assert.AreEqual(2, slots.ActiveCount);
            Assert.AreEqual(0f, slots.Age(reused), "Yeniden kullanılan yuva baştan başlar.");
        }

        [Test]
        public void Slots_Restart_ResetsAge()
        {
            var slots = new FloatingTextSlots(1);
            int slot = slots.Acquire(1f);
            slots.Tick(0.8f);

            slots.Restart(slot, 1f);
            slots.Tick(0.5f);

            Assert.IsTrue(slots.IsActive(slot));
            Assert.AreEqual(0.5f, slots.Progress(slot), Tolerance);
        }

        [Test]
        public void Slots_ZeroCapacity_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new FloatingTextSlots(0));
        }

        [Test]
        public void Motion_PopsThenSettles_RisesAndFades()
        {
            var motion = new FloatingTextMotion(popFraction: 0.2f, popScale: 1.6f, fadeStartFraction: 0.6f);

            FloatingTextPose start = motion.Evaluate(0f);
            Assert.AreEqual(1.6f, start.Scale, Tolerance, "İlk anda büyük.");
            Assert.AreEqual(0f, start.Rise, Tolerance);
            Assert.AreEqual(1f, start.Alpha, Tolerance);

            Assert.AreEqual(1.3f, motion.Evaluate(0.1f).Scale, Tolerance);
            Assert.AreEqual(1f, motion.Evaluate(0.2f).Scale, Tolerance, "Vuruş büyümesi bitince normal boy.");
            Assert.AreEqual(1f, motion.Evaluate(0.6f).Alpha, Tolerance, "Sönme eşiğine kadar tam opak.");
            Assert.AreEqual(0.5f, motion.Evaluate(0.8f).Alpha, Tolerance);

            FloatingTextPose end = motion.Evaluate(1f);
            Assert.AreEqual(1f, end.Rise, Tolerance);
            Assert.AreEqual(0f, end.Alpha, Tolerance);
            Assert.Greater(motion.Evaluate(0.5f).Rise, 0.5f, "Yükselme yavaşlayarak (ilk yarıda daha hızlı).");
        }

        [TestCase(5f, 0.8f)]
        [TestCase(10f, 0.8f)]
        [TestCase(35f, 1.3f)]
        [TestCase(60f, 1.8f)]
        [TestCase(200f, 1.8f)]
        public void DamageScale_BiggerForHigherDamage(float damage, float expected)
        {
            Assert.AreEqual(expected, FloatingTextMotion.DamageScale(damage, 10f, 60f, 0.8f, 1.8f), Tolerance);
        }

        [Test]
        public void NumberText_MatchesHudFormat_AndIsCached()
        {
            Assert.AreEqual("12", NumberText.Count(12));
            Assert.AreEqual("+12", NumberText.Gain(12));
            Assert.AreSame(NumberText.Count(48), NumberText.Count(48), "Aynı sayı için aynı string (yeni bellek yok).");
            Assert.AreSame(NumberText.Gain(7), NumberText.Gain(7));
            Assert.AreEqual(HudFormat.Count(12345), NumberText.Count(12345), "Önbellek dışı sayılar da doğru.");
            Assert.AreEqual(HudFormat.Gain(-3), NumberText.Gain(-3));
        }

        [Test]
        public void Banner_ShowsImmediately_QueuesWhileShowing_NextAfterTotal()
        {
            var queue = new BannerQueue<string>(Timing, queueLimit: 2);

            Assert.IsTrue(queue.Enqueue("GECE 3"));
            Assert.IsFalse(queue.Enqueue("BOSS"), "Gösterilirken gelen sıraya girer.");
            Assert.AreEqual("GECE 3", queue.Current);
            Assert.AreEqual(1, queue.WaitingCount);

            Assert.IsFalse(queue.Tick(1.6f));
            Assert.IsTrue(queue.Tick(0.2f), "Toplam süre (1.7 sn) dolunca sıradaki başlar.");
            Assert.AreEqual("BOSS", queue.Current);
            Assert.AreEqual(0f, queue.Age, Tolerance);

            Assert.IsTrue(queue.Tick(2f));
            Assert.IsFalse(queue.IsShowing, "Sıra boşsa başlık kalkar.");
        }

        [Test]
        public void Banner_QueueLimit_DropsExtra_ClearRemovesAll()
        {
            var queue = new BannerQueue<int>(Timing, queueLimit: 1);
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            Assert.AreEqual(1, queue.WaitingCount, "Sıra doluyken yenisi atılır.");
            queue.Clear();
            Assert.IsFalse(queue.IsShowing);
            Assert.AreEqual(0, queue.WaitingCount);
        }

        [Test]
        public void BannerTiming_FadesInFromLarge_HoldsThenFadesOut()
        {
            BannerPose start = Timing.Evaluate(0f);
            Assert.AreEqual(0f, start.Opacity, Tolerance);
            Assert.AreEqual(1.5f, start.Scale, Tolerance);

            Assert.AreEqual(0.5f, Timing.Evaluate(0.1f).Opacity, Tolerance);
            BannerPose held = Timing.Evaluate(0.7f);
            Assert.AreEqual(1f, held.Opacity, Tolerance);
            Assert.AreEqual(1f, held.Scale, Tolerance);
            Assert.AreEqual(0.5f, Timing.Evaluate(1.45f).Opacity, Tolerance);
            Assert.AreEqual(0f, Timing.Evaluate(1.7f).Opacity, Tolerance);
            Assert.AreEqual(1.7f, Timing.Total, Tolerance);
        }
    }
}
