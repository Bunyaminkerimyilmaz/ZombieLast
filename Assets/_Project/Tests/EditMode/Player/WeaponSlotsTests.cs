using LastHaven.Player;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>WeaponSlots: kilitli yuvalar seçilemez; sırayla değiştirme kilitlileri atlar ve başa sarar.</summary>
    public class WeaponSlotsTests
    {
        [Test]
        public void New_HasNoSelectionAndAllLocked()
        {
            var slots = new WeaponSlots(3);

            Assert.AreEqual(WeaponSlots.None, slots.Current);
            Assert.IsFalse(slots.IsUnlocked(0));
            Assert.IsFalse(slots.TrySelect(0));
        }

        [Test]
        public void TrySelect_OnlyUnlockedAndDifferent()
        {
            var slots = new WeaponSlots(3);
            slots.Unlock(0);

            Assert.IsTrue(slots.TrySelect(0));
            Assert.IsFalse(slots.TrySelect(0), "Zaten seçili.");
            Assert.IsFalse(slots.TrySelect(1), "Kilitli.");
            Assert.IsFalse(slots.TrySelect(5), "Geçersiz.");
            Assert.AreEqual(0, slots.Current);
        }

        [Test]
        public void Unlock_ReturnsFalseWhenAlreadyUnlockedOrInvalid()
        {
            var slots = new WeaponSlots(3);

            Assert.IsTrue(slots.Unlock(2));
            Assert.IsFalse(slots.Unlock(2));
            Assert.IsFalse(slots.Unlock(-1));
            Assert.IsFalse(slots.Unlock(3));
            Assert.AreEqual(WeaponSlots.None, slots.Current, "Açmak seçimi değiştirmez.");
        }

        [Test]
        public void TryCycle_FromNone_SelectsFirstUnlocked()
        {
            var slots = new WeaponSlots(3);
            slots.Unlock(1);
            slots.Unlock(2);

            Assert.IsTrue(slots.TryCycle(1));
            Assert.AreEqual(1, slots.Current);
        }

        [Test]
        public void TryCycle_SkipsLockedAndWraps()
        {
            var slots = new WeaponSlots(3);
            slots.Unlock(0);
            slots.Unlock(2);
            slots.TrySelect(0);

            Assert.IsTrue(slots.TryCycle(1));
            Assert.AreEqual(2, slots.Current, "1 kilitli, atlanır.");
            Assert.IsTrue(slots.TryCycle(1));
            Assert.AreEqual(0, slots.Current, "Sondan başa sarar.");
            Assert.IsTrue(slots.TryCycle(-1));
            Assert.AreEqual(2, slots.Current, "Baştan sona sarar.");
        }

        [Test]
        public void TryCycle_WithSingleUnlocked_DoesNothing()
        {
            var slots = new WeaponSlots(3);
            slots.Unlock(0);
            slots.TrySelect(0);

            Assert.IsFalse(slots.TryCycle(1));
            Assert.IsFalse(slots.TryCycle(-1));
            Assert.AreEqual(0, slots.Current);
        }

        [Test]
        public void TryCycle_WithNothingUnlocked_StaysNone()
        {
            var slots = new WeaponSlots(3);

            Assert.IsFalse(slots.TryCycle(1));
            Assert.AreEqual(WeaponSlots.None, slots.Current);
        }
    }
}
