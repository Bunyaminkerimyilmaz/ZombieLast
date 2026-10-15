using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>WaveRunner (GDD §8): zaman tabanlı grup işleme, canlı düşman tavanı ve kuyruk, bitiş koşulu.</summary>
    public class WaveRunnerTests
    {
        private readonly List<WaveSpawnRequest> _spawned = new List<WaveSpawnRequest>();
        private WaveRunner _runner;
        private bool _spawnSucceeds;
        private int _completedCount;
        private int _remainingChangedCount;
        private EnemyData _enemy;

        [SetUp]
        public void SetUp()
        {
            _spawned.Clear();
            _spawnSucceeds = true;
            _completedCount = 0;
            _remainingChangedCount = 0;

            _runner = new WaveRunner(request =>
            {
                _spawned.Add(request);
                return _spawnSucceeds;
            });
            _runner.Completed += () => _completedCount++;
            _runner.RemainingChanged += () => _remainingChangedCount++;
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemy != null)
            {
                Object.DestroyImmediate(_enemy);
            }
        }

        // --- Zamanlama ---

        [Test]
        public void Start_SetsTotals_WithoutSpawning()
        {
            _runner.Start(new[] { Group(3), Group(2) }, 10);

            Assert.IsTrue(_runner.IsRunning);
            Assert.AreEqual(5, _runner.TotalCount);
            Assert.AreEqual(5, _runner.PendingCount);
            Assert.AreEqual(5, _runner.RemainingCount);
            Assert.AreEqual(0, _runner.AliveCount);
            Assert.AreEqual(0, _runner.QueuedCount);
            Assert.IsEmpty(_spawned, "İlk spawn'lar ilk Tick'te üretilir.");
        }

        [Test]
        public void Tick_SpawnsFirstAtStart_ThenEveryInterval()
        {
            _runner.Start(new[] { Group(3, interval: 1f) }, 10);

            _runner.Tick(0f);
            Assert.AreEqual(1, _spawned.Count);

            _runner.Tick(0.5f);
            Assert.AreEqual(1, _spawned.Count);

            _runner.Tick(0.5f);
            Assert.AreEqual(2, _spawned.Count);

            _runner.Tick(1f);
            Assert.AreEqual(3, _spawned.Count);

            _runner.Tick(5f);
            Assert.AreEqual(3, _spawned.Count, "Adet aşılmaz.");
            Assert.AreEqual(3, _runner.AliveCount);
            Assert.AreEqual(0, _runner.PendingCount);
        }

        [Test]
        public void Tick_WaitsForStartDelay()
        {
            _runner.Start(new[] { Group(2, interval: 1f, delay: 2f) }, 10);

            _runner.Tick(1.5f);
            Assert.AreEqual(0, _spawned.Count);

            _runner.Tick(0.5f);
            Assert.AreEqual(1, _spawned.Count, "Gecikme tam dolunca ilk spawn.");

            _runner.Tick(1f);
            Assert.AreEqual(2, _spawned.Count);
        }

        [Test]
        public void Tick_LongFrame_EmitsAllDueSpawns()
        {
            _runner.Start(new[] { Group(5, interval: 0.25f) }, 10);

            _runner.Tick(1f);

            Assert.AreEqual(5, _spawned.Count, "0, 0.25, 0.5, 0.75 ve 1.0 sn'deki spawn'lar aynı karede.");
        }

        [Test]
        public void Tick_ZeroInterval_SpawnsWholeGroupAtOnce()
        {
            _runner.Start(new[] { Group(4, interval: 0f) }, 10);

            _runner.Tick(0f);

            Assert.AreEqual(4, _spawned.Count);
        }

        [Test]
        public void Directions_AreDistributedInOrder()
        {
            _runner.Start(new[] { Group(5, 0f, 0f, CompassDirection.North, CompassDirection.East) }, 10);

            _runner.Tick(0f);

            CollectionAssert.AreEqual(
                new[] { CompassDirection.North, CompassDirection.East, CompassDirection.North, CompassDirection.East, CompassDirection.North },
                _spawned.ConvertAll(r => r.Direction));
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, _spawned.ConvertAll(r => r.IndexInGroup));
        }

        [Test]
        public void Groups_RunOnTheirOwnTimers()
        {
            _runner.Start(new[]
            {
                Group(2, 1f, 0f, CompassDirection.North),
                Group(2, 1f, 0.5f, CompassDirection.South),
            }, 10);

            _runner.Tick(0f);
            _runner.Tick(0.5f);
            _runner.Tick(0.5f);
            _runner.Tick(0.5f);

            CollectionAssert.AreEqual(new[] { 0, 1, 0, 1 }, _spawned.ConvertAll(r => r.GroupIndex));
            Assert.AreEqual(CompassDirection.South, _spawned[1].Direction);
        }

        [Test]
        public void Request_CarriesEnemyAndBossFlag()
        {
            _enemy = ScriptableObject.CreateInstance<EnemyData>();
            _runner.Start(new[] { new WaveGroup(_enemy, 1, 0f, 0f, new[] { CompassDirection.West }, isBoss: true) }, 10);

            _runner.Tick(0f);

            Assert.AreSame(_enemy, _spawned[0].Enemy);
            Assert.IsTrue(_spawned[0].IsBoss);
            Assert.AreEqual(CompassDirection.West, _spawned[0].Direction);
        }

        [Test]
        public void Tick_NotRunning_DoesNothing()
        {
            _runner.Tick(10f);

            Assert.IsFalse(_runner.IsRunning);
            Assert.IsEmpty(_spawned);
            Assert.AreEqual(0, _completedCount);
        }

        // --- Tavan ve kuyruk ---

        [Test]
        public void Cap_QueuesExcess_AndSpawnsOnNextTickAfterDeath()
        {
            _runner.Start(new[] { Group(5, interval: 0f) }, 2);

            _runner.Tick(0f);
            Assert.AreEqual(2, _spawned.Count);
            Assert.AreEqual(2, _runner.AliveCount);
            Assert.AreEqual(3, _runner.QueuedCount);
            Assert.AreEqual(5, _runner.RemainingCount);

            Assert.IsTrue(_runner.ReportDeath());
            Assert.AreEqual(1, _runner.AliveCount);
            Assert.AreEqual(2, _spawned.Count, "Kuyruk sadece Tick'te boşalır.");

            _runner.Tick(0f);
            Assert.AreEqual(3, _spawned.Count);
            Assert.AreEqual(2, _runner.AliveCount);
            Assert.AreEqual(2, _runner.QueuedCount);
            Assert.AreEqual(4, _runner.RemainingCount);
        }

        [Test]
        public void Cap_QueueIsFirstInFirstOut()
        {
            _runner.Start(new[]
            {
                Group(2, 0f, 0f, CompassDirection.North),
                Group(2, 0f, 0f, CompassDirection.South),
            }, 1);

            _runner.Tick(0f);
            for (int i = 0; i < 3; i++)
            {
                _runner.ReportDeath();
                _runner.Tick(0f);
            }

            CollectionAssert.AreEqual(new[] { 0, 0, 1, 1 }, _spawned.ConvertAll(r => r.GroupIndex));
            CollectionAssert.AreEqual(new[] { 0, 1, 0, 1 }, _spawned.ConvertAll(r => r.IndexInGroup));
        }

        [Test]
        public void Cap_GroupsKeepEmittingIntoQueueWhileFull()
        {
            _runner.Start(new[] { Group(4, interval: 1f) }, 1);

            _runner.Tick(0f);
            _runner.Tick(3f);

            Assert.AreEqual(1, _spawned.Count);
            Assert.AreEqual(1, _runner.AliveCount);
            Assert.AreEqual(3, _runner.QueuedCount);
            Assert.AreEqual(0, _runner.PendingCount);
        }

        [Test]
        public void Cap_BelowOne_IsClampedToOne()
        {
            _runner.Start(new[] { Group(3, interval: 0f) }, 0);

            _runner.Tick(0f);

            Assert.AreEqual(1, _runner.MaxAlive);
            Assert.AreEqual(1, _spawned.Count);
        }

        // --- Bitiş ---

        [Test]
        public void Completes_OnlyWhenAllSpawnedAndAllDead()
        {
            _runner.Start(new[] { Group(2, interval: 1f) }, 10);

            _runner.Tick(0f);
            _runner.ReportDeath();
            _runner.Tick(0f);
            Assert.IsTrue(_runner.IsRunning, "Spawn'ı bekleyen düşman var.");

            _runner.Tick(1f);
            Assert.IsTrue(_runner.IsRunning, "Canlı düşman var.");

            _runner.ReportDeath();
            Assert.AreEqual(0, _completedCount, "Bitiş Tick'te kontrol edilir.");

            _runner.Tick(0f);
            Assert.IsFalse(_runner.IsRunning);
            Assert.AreEqual(1, _completedCount);
            Assert.AreEqual(0, _runner.RemainingCount);

            _runner.Tick(1f);
            Assert.AreEqual(1, _completedCount, "Completed bir kez gelir.");
        }

        [Test]
        public void Completes_NotBeforeDelayedGroupHasSpawned()
        {
            _runner.Start(new[] { Group(1), Group(1, interval: 0f, delay: 10f) }, 10);

            _runner.Tick(0f);
            _runner.ReportDeath();
            _runner.Tick(5f);
            Assert.IsTrue(_runner.IsRunning);

            _runner.Tick(5f);
            Assert.AreEqual(2, _spawned.Count);
            _runner.ReportDeath();
            _runner.Tick(0f);

            Assert.AreEqual(1, _completedCount);
        }

        [Test]
        public void Completes_NotWhileQueueHasEnemies()
        {
            _runner.Start(new[] { Group(2, interval: 0f) }, 1);

            _runner.Tick(0f);
            _runner.ReportDeath();
            Assert.AreEqual(1, _runner.QueuedCount);

            _runner.Tick(0f);
            Assert.IsTrue(_runner.IsRunning);
            Assert.AreEqual(2, _spawned.Count);
        }

        [Test]
        public void EmptyNight_CompletesOnFirstTick()
        {
            _runner.Start(new[] { Group(0) }, 10);

            Assert.AreEqual(0, _runner.TotalCount);
            _runner.Tick(0f);

            Assert.AreEqual(1, _completedCount);
            Assert.IsFalse(_runner.IsRunning);
        }

        [Test]
        public void FailedSpawn_IsDropped_AndNightCanComplete()
        {
            _spawnSucceeds = false;
            _runner.Start(new[] { Group(2, interval: 0f) }, 10);

            _runner.Tick(0f);

            Assert.AreEqual(2, _spawned.Count, "İki istek de denendi.");
            Assert.AreEqual(0, _runner.AliveCount);
            Assert.AreEqual(2, _runner.DroppedCount);
            Assert.AreEqual(0, _runner.RemainingCount);
            Assert.AreEqual(2, _remainingChangedCount);
            Assert.AreEqual(1, _completedCount);
        }

        // --- Ölüm bildirimi ---

        [Test]
        public void ReportDeath_DecrementsRemaining_AndRaisesEvent()
        {
            _runner.Start(new[] { Group(2, interval: 0f) }, 10);
            _runner.Tick(0f);

            Assert.IsTrue(_runner.ReportDeath());

            Assert.AreEqual(1, _runner.AliveCount);
            Assert.AreEqual(1, _runner.RemainingCount);
            Assert.AreEqual(1, _remainingChangedCount);
        }

        [Test]
        public void ReportDeath_WithoutAliveEnemies_IsIgnored()
        {
            Assert.IsFalse(_runner.ReportDeath(), "Gece yok.");

            _runner.Start(new[] { Group(1, interval: 0f, delay: 5f) }, 10);
            Assert.IsFalse(_runner.ReportDeath(), "Henüz doğan yok.");

            Assert.AreEqual(1, _runner.RemainingCount);
            Assert.AreEqual(0, _remainingChangedCount);
        }

        // --- Başlatma ---

        [Test]
        public void Start_Again_ResetsState()
        {
            _runner.Start(new[] { Group(3, interval: 0f) }, 1);
            _runner.Tick(2f);

            _runner.Start(new[] { Group(1) }, 10);

            Assert.AreEqual(0f, _runner.Elapsed);
            Assert.AreEqual(1, _runner.TotalCount);
            Assert.AreEqual(0, _runner.AliveCount);
            Assert.AreEqual(0, _runner.QueuedCount);
            Assert.AreEqual(1, _runner.RemainingCount);
        }

        [Test]
        public void Start_GroupWithCountButNoDirection_Throws()
        {
            var groups = new[] { new WaveGroup(null, 1, 0f, 0f, Array.Empty<CompassDirection>()) };

            Assert.Throws<ArgumentException>(() => _runner.Start(groups, 10));
        }

        [Test]
        public void Start_EmptyGroupWithoutDirection_IsAllowed()
        {
            var groups = new[] { new WaveGroup(null, 0, 0f, 0f, null) };

            Assert.DoesNotThrow(() => _runner.Start(groups, 10));
        }

        [Test]
        public void WaveGroup_ClampsNegativeValues()
        {
            var group = new WaveGroup(null, -3, -1f, -2f, null);

            Assert.AreEqual(0, group.Count);
            Assert.AreEqual(0f, group.SpawnInterval);
            Assert.AreEqual(0f, group.StartDelay);
            Assert.IsNotNull(group.Directions);
        }

        private static WaveGroup Group(int count, float interval = 1f, float delay = 0f, params CompassDirection[] directions)
        {
            if (directions.Length == 0)
            {
                directions = new[] { CompassDirection.North };
            }

            return new WaveGroup(null, count, interval, delay, directions);
        }
    }
}
