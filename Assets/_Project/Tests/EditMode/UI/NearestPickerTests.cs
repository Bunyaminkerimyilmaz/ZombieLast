using System;
using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>NearestPicker (11.1): en yakın K aday, yakından uzağa; eşitlikte önce gelen; Clear ile yeniden kullanılır.</summary>
    public class NearestPickerTests
    {
        [Test]
        public void KeepsNearestThree_InOrder()
        {
            var picker = new NearestPicker(3);

            picker.Offer(0, 30f);
            picker.Offer(1, 10f);
            picker.Offer(2, 50f);
            picker.Offer(3, 20f);
            picker.Offer(4, 5f);
            picker.Offer(5, 40f);

            Assert.AreEqual(3, picker.Count);
            Assert.AreEqual(4, picker.GetIndex(0));
            Assert.AreEqual(1, picker.GetIndex(1));
            Assert.AreEqual(3, picker.GetIndex(2));
        }

        [Test]
        public void FewerCandidatesThanCapacity()
        {
            var picker = new NearestPicker(3);

            picker.Offer(7, 12f);
            picker.Offer(2, 3f);

            Assert.AreEqual(2, picker.Count);
            Assert.AreEqual(2, picker.GetIndex(0));
            Assert.AreEqual(7, picker.GetIndex(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => picker.GetIndex(2));
        }

        [Test]
        public void Ties_KeepEarlierFirst_FullListRejectsEqualToLast()
        {
            var picker = new NearestPicker(2);

            picker.Offer(0, 10f);
            picker.Offer(1, 10f);
            picker.Offer(2, 10f);

            Assert.AreEqual(0, picker.GetIndex(0));
            Assert.AreEqual(1, picker.GetIndex(1));
        }

        [Test]
        public void Clear_Resets()
        {
            var picker = new NearestPicker(2);
            picker.Offer(0, 1f);

            picker.Clear();
            picker.Offer(5, 9f);

            Assert.AreEqual(1, picker.Count);
            Assert.AreEqual(5, picker.GetIndex(0));
        }

        [Test]
        public void ZeroCapacity_KeepsNothing()
        {
            var picker = new NearestPicker(0);

            picker.Offer(0, 1f);

            Assert.AreEqual(0, picker.Count);
            Assert.Throws<ArgumentOutOfRangeException>(() => new NearestPicker(-1));
        }
    }
}
