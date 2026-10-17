using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// HoldRingProgress (10.3): halka tutma turunun ilerlemesini gösterir; tekrarlanan turlarda (onarım) bırakılana kadar dolu
    /// kalır; bırakınca ve hedef değişince sıfırlanır. ProgressRing doluluğu 0..1'e kırpar.
    /// </summary>
    public class HoldRingProgressTests
    {
        private const float Tolerance = 0.0001f;

        private FakeInteractable _repair;
        private FakeInteractable _fuel;
        private HoldRingProgress _ring;

        [SetUp]
        public void SetUp()
        {
            _repair = new FakeInteractable(Vector3.zero);
            _fuel = new FakeInteractable(Vector3.one);
            _ring = new HoldRingProgress();
        }

        [Test]
        public void NotHolding_IsEmpty()
        {
            Assert.AreEqual(0f, _ring.Update(_repair, false, 0.7f), Tolerance);
        }

        [Test]
        public void Holding_ShowsTurnProgress()
        {
            _ring.Update(_fuel, true, 0.25f);

            Assert.AreEqual(0.6f, _ring.Update(_fuel, true, 0.6f), Tolerance);
            Assert.AreEqual(0.6f, _ring.Value, Tolerance);
        }

        [Test]
        public void RepeatingTurns_StayFullUntilReleased()
        {
            _ring.Update(_repair, true, 0.5f);
            _ring.Update(_repair, true, 0.9f);

            Assert.AreEqual(1f, _ring.Update(_repair, true, 0.2f), Tolerance, "Yeni tur başladı: halka dolu kalmalı.");
            Assert.AreEqual(1f, _ring.Update(_repair, true, 0.7f), Tolerance);
            Assert.AreEqual(0f, _ring.Update(_repair, false, 0f), Tolerance);
            Assert.AreEqual(0.3f, _ring.Update(_repair, true, 0.3f), Tolerance, "Yeniden basınca baştan dolar.");
        }

        [Test]
        public void TargetChange_Resets()
        {
            _ring.Update(_repair, true, 0.8f);
            _ring.Update(_repair, true, 0.1f);

            Assert.AreEqual(0.1f, _ring.Update(_fuel, true, 0.1f), Tolerance);
        }

        [Test]
        public void ProgressAboveOne_IsClamped()
        {
            Assert.AreEqual(1f, _ring.Update(_fuel, true, 1.5f), Tolerance);
        }

        [Test]
        public void ProgressRing_ClampsProgress()
        {
            var ring = new ProgressRing(new UnityEngine.UIElements.VisualElement());

            ring.SetProgress(1.4f);
            Assert.AreEqual(1f, ring.Progress, Tolerance);
            ring.SetProgress(-0.2f);
            Assert.AreEqual(0f, ring.Progress, Tolerance);
            Assert.DoesNotThrow(ring.Detach);
        }
    }
}
