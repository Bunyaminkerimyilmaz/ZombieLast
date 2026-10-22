using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.DebugTools;
using LastHaven.Enemies;
using LastHaven.UI;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using AllocIs = UnityEngine.TestTools.Constraints.Is;
using NumberText = LastHaven.UI.NumberText;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// 15.2 performans: sıcak yolların bellek ayırmadığı (her kare / her vuruş çağrılanlar), stres testi karışımı, GC penceresi,
    /// dalga havuzu ihtiyacı ve HUD metin önbellekleri.
    /// </summary>
    public class PerformanceTests
    {
        private readonly List<Object> _created = new List<Object>();

        // Ölçülen çağrıların sonucu buraya yazılır (ölçüm void delegate ister; sonuç atılmasın).
        private float _floatSink;
        private string _textSink;
        private int _intSink;

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        // --- Sıcak yollar bellek ayırmaz ---

        [Test]
        public void PlanarDistanceToBounds_DoesNotAllocate()
        {
            var bounds = new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));
            var from = new Vector3(5f, 0f, 0f);
            Assert.AreEqual(4f, PlanarDistance.ToBounds(from, bounds), 0.0001f);

            Assert.That(() => { _floatSink = PlanarDistance.ToBounds(from, bounds); }, AllocIs.Not.AllocatingGCMemory());
        }

        [Test]
        public void BallisticFlightTime_DoesNotAllocate()
        {
            Assert.That(() => { _floatSink = BallisticArc.FlightTime(Vector3.zero, new Vector3(10f, 0f, 0f), 10f, 0.5f); },
                AllocIs.Not.AllocatingGCMemory());
        }

        [Test]
        public void CachedHudTexts_DoNotAllocateAfterFirstUse()
        {
            NumberText.PrefillCounts();
            FractionText.Prefill(1500);
            NumberText.Timer(125);

            Assert.That(() => { _textSink = NumberText.Count(437); }, AllocIs.Not.AllocatingGCMemory());
            Assert.That(() => { _textSink = FractionText.Get(1234, 1500); }, AllocIs.Not.AllocatingGCMemory());
            Assert.That(() => { _textSink = NumberText.Timer(125); }, AllocIs.Not.AllocatingGCMemory());
        }

        [Test]
        public void StressMixAndAllocationWindow_DoNotAllocate()
        {
            var mix = new StressMix(new[] { 16, 7, 2, 3, 3 });
            var window = new AllocationWindow();

            Assert.That(() => { _intSink = mix.PickIndex(123); }, AllocIs.Not.AllocatingGCMemory());
            Assert.That(() => { window.Add(64); }, AllocIs.Not.AllocatingGCMemory());
        }

        // --- Stres testi karışımı ---

        [Test]
        public void StressMix_CyclesThroughWeightsInOrder()
        {
            var mix = new StressMix(new[] { 2, 1, 0, 3 });

            int[] picks = new int[7];
            for (int i = 0; i < picks.Length; i++)
            {
                picks[i] = mix.PickIndex(i);
            }

            CollectionAssert.AreEqual(new[] { 0, 0, 1, 3, 3, 3, 0 }, picks, "Ağırlık 0 olan tip hiç doğmaz; döngü baştan başlar.");
            Assert.AreEqual(6, mix.TotalWeight);
        }

        [Test]
        public void StressMix_ShareRoundsUp_AndEmptyMixPicksNothing()
        {
            var mix = new StressMix(new[] { 16, 7, 2, 3, 3 });
            Assert.AreEqual(104, mix.Share(0, 200), "200 × 16 / 31 = 103.2 → 104.");
            Assert.AreEqual(13, mix.Share(2, 200));
            Assert.AreEqual(0, mix.Share(9, 200));

            var empty = new StressMix(new[] { 0, -2 });
            Assert.AreEqual(-1, empty.PickIndex(0));
            Assert.AreEqual(0, empty.Share(0, 200));
        }

        [TestCase(150, 200, 10, 10)]
        [TestCase(195, 200, 10, 5)]
        [TestCase(200, 200, 10, 0)]
        [TestCase(230, 200, 10, 0)]
        public void SpawnBudget_FillsUpToTargetPerFrame(int alive, int target, int perFrame, int expected)
        {
            Assert.AreEqual(expected, StressMix.SpawnBudget(alive, target, perFrame));
        }

        // --- GC penceresi ---

        [Test]
        public void AllocationWindow_AveragesAndCountsAllocatingFrames()
        {
            var window = new AllocationWindow();
            window.Add(0);
            window.Add(300);
            window.Add(-5);
            window.Add(100);

            Assert.AreEqual(4, window.Frames);
            Assert.AreEqual(2, window.AllocatingFrames);
            Assert.AreEqual(400, window.TotalBytes);
            Assert.AreEqual(300, window.MaxBytes);
            Assert.AreEqual(100d, window.AverageBytes, 0.0001);
            Assert.AreEqual(0.5d, window.AllocatingRatio, 0.0001);

            window.Reset();
            Assert.AreEqual(0, window.Frames);
            Assert.AreEqual(0d, window.AverageBytes);
        }

        // --- Dalga havuzu ihtiyacı ---

        [Test]
        public void WavePrewarm_SumsTypeWithinNight_CapsByMaxAlive_TakesLargestNight()
        {
            EnemyData walker = CreateEnemy();
            EnemyData runner = CreateEnemy();
            var needs = new Dictionary<EnemyData, int>();
            var scratch = new Dictionary<EnemyData, int>();

            WavePrewarm.AddNight(new[] { Group(walker, 30), Group(runner, 8), Group(walker, 10) }, 120, needs, scratch);
            WavePrewarm.AddNight(new[] { Group(walker, 20), Group(runner, 45) }, 120, needs, scratch);
            WavePrewarm.AddNight(new[] { Group(walker, 200) }, 120, needs, scratch);

            Assert.AreEqual(120, needs[walker], "Gece içi toplam canlı tavanını aşamaz.");
            Assert.AreEqual(45, needs[runner]);

            WavePrewarm.AddWanderers(walker, 6, needs);
            Assert.AreEqual(126, needs[walker], "Geceye katılan gezginler eklenir.");
        }

        [Test]
        public void WavePrewarm_IgnoresEmptyGroups()
        {
            EnemyData walker = CreateEnemy();
            var needs = new Dictionary<EnemyData, int>();

            WavePrewarm.AddNight(new[] { Group(walker, 0), Group(null, 10) }, 120, needs, new Dictionary<EnemyData, int>());
            WavePrewarm.AddWanderers(null, 5, needs);

            Assert.IsEmpty(needs);
        }

        // --- HUD metin önbellekleri ---

        [Test]
        public void FractionText_MatchesFormat_ReturnsSameInstance()
        {
            Assert.AreEqual(HudFormat.OfTotal(73, 100), FractionText.Get(73, 100));
            Assert.AreSame(FractionText.Get(42, 250), FractionText.Get(42, 250));
            Assert.AreEqual("15000/20000", FractionText.Get(15000, 20000), "Önbellek dışı toplam da doğru.");
            Assert.AreEqual("120/100", FractionText.Get(120, 100), "Değer toplamı aşsa da doğru (önbelleksiz).");
        }

        [Test]
        public void NumberText_TimerAndEstimate_MatchFormat()
        {
            Assert.AreEqual(HudFormat.Timer(125), NumberText.Timer(125));
            Assert.AreSame(NumberText.Timer(9), NumberText.Timer(9));
            Assert.AreEqual(HudFormat.Estimate(-1), NumberText.Estimate(-1));
            Assert.AreEqual(HudFormat.Timer(3725), NumberText.Timer(3725), "Önbellek dışı süre de doğru.");
        }

        private EnemyData CreateEnemy()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            _created.Add(enemy);
            return enemy;
        }

        private static WaveGroup Group(EnemyData enemy, int count)
        {
            return new WaveGroup(enemy, count, 1f, 0f, new[] { CompassDirection.North });
        }
    }
}
