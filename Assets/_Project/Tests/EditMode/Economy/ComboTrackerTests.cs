using System.Collections.Generic;
using LastHaven.Economy;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>ComboTracker (7.3): GDD §10 eşikleri (5 → ×1.5, 15 → ×2, 30 → ×3), 2.5 sn pencere ve sıfırlanma.</summary>
    public class ComboTrackerTests
    {
        private const float Window = 2.5f;
        private const float Tolerance = 0.0001f;

        private static readonly ComboTier[] GddTiers =
        {
            new ComboTier(5, 1.5f),
            new ComboTier(15, 2f),
            new ComboTier(30, 3f),
        };

        private ComboTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new ComboTracker(Window, GddTiers);
        }

        [Test]
        public void New_IsInactive()
        {
            Assert.AreEqual(0, _tracker.Count);
            Assert.AreEqual(1f, _tracker.Multiplier, Tolerance);
            Assert.AreEqual(0f, _tracker.WindowRatio, Tolerance);
            Assert.IsFalse(_tracker.IsActive);
        }

        [TestCase(1, 1f)]
        [TestCase(4, 1f)]
        [TestCase(5, 1.5f)]
        [TestCase(14, 1.5f)]
        [TestCase(15, 2f)]
        [TestCase(29, 2f)]
        [TestCase(30, 3f)]
        [TestCase(100, 3f)]
        public void RegisterKill_ThresholdKillGetsMultiplier(int kills, float expected)
        {
            float last = 0f;
            for (int i = 0; i < kills; i++)
            {
                last = _tracker.RegisterKill();
            }

            Assert.AreEqual(kills, _tracker.Count);
            Assert.AreEqual(expected, last, Tolerance, "Eşiğe ulaştıran öldürme de o çarpanı alır.");
            Assert.AreEqual(expected, _tracker.Multiplier, Tolerance);
        }

        [Test]
        public void RegisterKill_RestartsWindow()
        {
            _tracker.RegisterKill();
            _tracker.Tick(2f);
            Assert.AreEqual(0.5f / Window, _tracker.WindowRatio, Tolerance);

            _tracker.RegisterKill();

            Assert.AreEqual(Window, _tracker.WindowRemaining, Tolerance);
            Assert.AreEqual(1f, _tracker.WindowRatio, Tolerance);
            Assert.AreEqual(2, _tracker.Count);
        }

        [Test]
        public void KillsInsideWindow_KeepCombo()
        {
            for (int i = 0; i < 20; i++)
            {
                _tracker.RegisterKill();
                Assert.IsFalse(_tracker.Tick(Window - 0.1f), $"{i}. öldürmeden sonra pencere dolmamalı.");
            }

            Assert.AreEqual(20, _tracker.Count);
            Assert.AreEqual(2f, _tracker.Multiplier, Tolerance);
        }

        [Test]
        public void Tick_WindowExpires_ResetsOnce()
        {
            for (int i = 0; i < 6; i++)
            {
                _tracker.RegisterKill();
            }

            Assert.IsFalse(_tracker.Tick(Window - 0.01f));
            Assert.IsTrue(_tracker.IsActive);

            Assert.IsTrue(_tracker.Tick(0.02f), "Pencere dolunca sıfırlanmalı.");
            Assert.AreEqual(0, _tracker.Count);
            Assert.AreEqual(1f, _tracker.Multiplier, Tolerance);
            Assert.AreEqual(0f, _tracker.WindowRatio, Tolerance);

            Assert.IsFalse(_tracker.Tick(1f), "Sonuç bir kez gelir.");
        }

        [Test]
        public void Tick_Inactive_DoesNothing()
        {
            Assert.IsFalse(_tracker.Tick(10f));
            Assert.AreEqual(0, _tracker.Count);
        }

        [Test]
        public void Reset_Active_ClearsAndReturnsTrue()
        {
            for (int i = 0; i < 16; i++)
            {
                _tracker.RegisterKill();
            }

            Assert.IsTrue(_tracker.Reset());
            Assert.AreEqual(0, _tracker.Count);
            Assert.AreEqual(1f, _tracker.Multiplier, Tolerance);
            Assert.AreEqual(0f, _tracker.WindowRemaining, Tolerance);
        }

        [Test]
        public void Reset_Inactive_ReturnsFalse()
        {
            Assert.IsFalse(_tracker.Reset());
        }

        [Test]
        public void AfterReset_CountsFromOne()
        {
            for (int i = 0; i < 5; i++)
            {
                _tracker.RegisterKill();
            }

            _tracker.Reset();

            Assert.AreEqual(1f, _tracker.RegisterKill(), Tolerance);
            Assert.AreEqual(1, _tracker.Count);
        }

        [Test]
        public void WindowRatio_DecreasesLinearly()
        {
            _tracker.RegisterKill();

            _tracker.Tick(Window * 0.25f);
            Assert.AreEqual(0.75f, _tracker.WindowRatio, Tolerance);
            _tracker.Tick(Window * 0.5f);
            Assert.AreEqual(0.25f, _tracker.WindowRatio, Tolerance);
        }

        [Test]
        public void GetMultiplier_UnsortedTiers_UsesHighestReached()
        {
            var tiers = new List<ComboTier> { new ComboTier(30, 3f), new ComboTier(5, 1.5f), new ComboTier(15, 2f) };

            Assert.AreEqual(1f, ComboTracker.GetMultiplier(4, tiers), Tolerance);
            Assert.AreEqual(1.5f, ComboTracker.GetMultiplier(5, tiers), Tolerance);
            Assert.AreEqual(2f, ComboTracker.GetMultiplier(20, tiers), Tolerance);
            Assert.AreEqual(3f, ComboTracker.GetMultiplier(30, tiers), Tolerance);
        }

        [Test]
        public void NoTiers_AlwaysBaseMultiplier()
        {
            var tracker = new ComboTracker(Window, null);

            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(ComboTracker.BaseMultiplier, tracker.RegisterKill(), Tolerance);
            }
        }

        [Test]
        public void Constructor_CopiesTiers()
        {
            var tiers = new List<ComboTier> { new ComboTier(2, 2f) };
            var tracker = new ComboTracker(Window, tiers);
            tiers.Clear();

            tracker.RegisterKill();

            Assert.AreEqual(2f, tracker.RegisterKill(), Tolerance, "Liste sonradan değişse de eşikler korunmalı.");
        }

        [Test]
        public void ZeroWindow_EndsOnNextTick()
        {
            var tracker = new ComboTracker(0f, GddTiers);
            tracker.RegisterKill();
            tracker.RegisterKill();

            Assert.AreEqual(2, tracker.Count, "Aynı karedeki öldürmeler sayılır.");
            Assert.IsTrue(tracker.Tick(0f));
        }
    }
}
