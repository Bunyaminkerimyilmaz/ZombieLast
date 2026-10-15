using LastHaven.Player;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// InteractionHold (8.3): basılı tutma süresi, tekrar (fazla süre devreder), OnInteract false → durma (bırakınca sıfırlanır),
    /// hedef değişince/bırakınca ilerleme 0.
    /// </summary>
    public class InteractionHoldTests
    {
        private const float Tolerance = 0.0001f;

        private InteractionHold _hold;
        private FakeInteractable _target;

        [SetUp]
        public void SetUp()
        {
            _hold = new InteractionHold();
            _target = new FakeInteractable(Vector3.zero, holdDuration: 1f);
        }

        [Test]
        public void SetTarget_ReportsChangeOnlyOnce()
        {
            Assert.IsTrue(_hold.SetTarget(_target));
            Assert.IsFalse(_hold.SetTarget(_target));
            Assert.IsTrue(_hold.SetTarget(null));
            Assert.IsNull(_hold.Target);
        }

        [Test]
        public void Tick_HoldingForDuration_InteractsOnceAndReportsProgress()
        {
            _hold.SetTarget(_target);

            _hold.Tick(true, 0.25f);
            Assert.IsTrue(_hold.IsHolding);
            Assert.AreEqual(0.25f, _hold.Progress, Tolerance);
            Assert.AreEqual(0.25f, _target.LastProgress, Tolerance);
            Assert.AreEqual(0, _target.InteractCount);

            _hold.Tick(true, 0.5f);
            _hold.Tick(true, 0.25f);
            Assert.AreEqual(1, _target.InteractCount, "Tam 1 sn'de etkileşim.");
            Assert.Contains(1f, _target.Progress, "Tur bitince 1 ilerleme.");
            Assert.AreEqual(0f, _hold.Progress, Tolerance, "Yeni tur baştan.");
        }

        [Test]
        public void Tick_KeepHolding_RepeatsAndCarriesOverExtraTime()
        {
            var cycle = new FakeInteractable(Vector3.zero, holdDuration: 0.125f);
            _hold.SetTarget(cycle);

            // 0.3 sn: 2 tur (0.25) + 0.05 devreder.
            _hold.Tick(true, 0.3f);
            Assert.AreEqual(2, cycle.InteractCount);
            Assert.AreEqual(0.05f / 0.125f, _hold.Progress, Tolerance);

            _hold.Tick(true, 0.075f);
            Assert.AreEqual(3, cycle.InteractCount, "Devreden süre sayılır: 0.05 + 0.075 = 0.125.");
        }

        [Test]
        public void Tick_RepairRate_IsFrameRateIndependent()
        {
            // GDD §11: 0.1 sn'de bir tur → 1 sn'de 10 tur; 60 fps'te de kayıp yok.
            var repair = new FakeInteractable(Vector3.zero, holdDuration: 0.1f);
            _hold.SetTarget(repair);

            for (int i = 0; i < 60; i++)
            {
                _hold.Tick(true, 1f / 60f);
            }

            Assert.That(repair.InteractCount, Is.InRange(9, 10), "Float birikimiyle en fazla 1 tur sınırda kalabilir.");
            _hold.Tick(true, 0.001f);
            Assert.AreEqual(10, repair.InteractCount);
        }

        [Test]
        public void Tick_Release_ResetsProgressAndNotifiesZero()
        {
            _hold.SetTarget(_target);
            _hold.Tick(true, 0.6f);

            _hold.Tick(false, 0.1f);

            Assert.IsFalse(_hold.IsHolding);
            Assert.AreEqual(0f, _hold.Elapsed);
            Assert.AreEqual(0f, _target.LastProgress, "Bırakınca bir kez 0.");
            Assert.AreEqual(0, _target.InteractCount);

            _hold.Tick(true, 0.6f);
            Assert.AreEqual(0, _target.InteractCount, "İlerleme baştan başladı.");
        }

        [Test]
        public void Tick_InteractReturnsFalse_StopsUntilReleased()
        {
            _target.InteractResult = false;
            _hold.SetTarget(_target);

            _hold.Tick(true, 1f);
            Assert.AreEqual(1, _target.InteractCount);
            Assert.IsTrue(_hold.IsStopped);
            Assert.IsFalse(_hold.IsHolding);
            Assert.AreEqual(0f, _target.LastProgress);

            _target.InteractResult = true;
            _hold.Tick(true, 5f);
            Assert.AreEqual(1, _target.InteractCount, "Basılı tutmaya devam etmek yeniden başlatmaz (para bitti).");

            _hold.Tick(false, 0f);
            Assert.IsFalse(_hold.IsStopped);
            _hold.Tick(true, 1f);
            Assert.AreEqual(2, _target.InteractCount, "Bırakıp yeniden basınca devam eder.");
        }

        [Test]
        public void Tick_TargetDone_StopsRepeatingAndDropsExtraTime()
        {
            var repair = new FakeInteractable(Vector3.zero, holdDuration: 0.1f) { InteractionsUntilDone = 2 };
            _hold.SetTarget(repair);

            _hold.Tick(true, 1f);

            Assert.AreEqual(2, repair.InteractCount, "Tam onarılınca tur biter.");
            Assert.AreEqual(0f, _hold.Elapsed);

            _hold.Tick(true, 1f);
            Assert.AreEqual(2, repair.InteractCount);
            Assert.IsFalse(_hold.IsHolding);
        }

        [Test]
        public void Tick_LongFrame_IsCapped()
        {
            var repair = new FakeInteractable(Vector3.zero, holdDuration: 0.1f);
            _hold.SetTarget(repair);

            _hold.Tick(true, 10f);

            Assert.AreEqual(8, repair.InteractCount, "Takılmada bir karede en fazla 8 etkileşim.");
            Assert.AreEqual(0f, _hold.Elapsed);
        }

        [Test]
        public void Tick_ZeroDuration_InteractsOncePerTick()
        {
            var instant = new FakeInteractable(Vector3.zero, holdDuration: 0f);
            _hold.SetTarget(instant);

            _hold.Tick(true, 0.5f);
            _hold.Tick(true, 0.5f);

            Assert.AreEqual(2, instant.InteractCount);
        }

        [Test]
        public void SetTarget_WhileHolding_CancelsOldAndStartsFresh()
        {
            var other = new FakeInteractable(Vector3.one);
            _hold.SetTarget(_target);
            _hold.Tick(true, 0.7f);

            _hold.SetTarget(other);
            Assert.AreEqual(0f, _target.LastProgress, "Eski hedefe 0.");
            Assert.AreEqual(0f, _hold.Elapsed);

            _hold.Tick(true, 0.5f);
            Assert.AreEqual(0, other.InteractCount, "Yeni hedefte süre baştan.");
            Assert.AreEqual(0, _target.InteractCount);
        }

        [Test]
        public void Tick_NoTargetOrCannotInteract_DoesNothing()
        {
            _hold.Tick(true, 1f);
            Assert.IsFalse(_hold.IsHolding);

            _target.CanInteract = false;
            _hold.SetTarget(_target);
            _hold.Tick(true, 2f);
            Assert.AreEqual(0, _target.InteractCount);
            Assert.AreEqual(0f, _hold.Progress);
        }
    }
}
