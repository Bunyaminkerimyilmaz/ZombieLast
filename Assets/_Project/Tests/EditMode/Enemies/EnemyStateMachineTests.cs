using System.Collections.Generic;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// EnemyStateMachine: Spawning → Chasing ⇄ Attacking → Dead geçişleri, saldırı hızı ve ceset süreleri; gezgin modunda
    /// Chasing ⇄ Wandering (ayak, bekleme, zaman aşımı).
    /// </summary>
    public class EnemyStateMachineTests
    {
        private const float Tolerance = 0.0001f;

        // Yürüyen (GDD §7): menzil 1.5 m, 1 saldırı/sn. Süreler EnemyData varsayılanları.
        private static readonly EnemyBehaviourSettings Walker = new EnemyBehaviourSettings(
            spawnDuration: 0.5f, attackRange: 1.5f, disengageMargin: 0.3f, attackInterval: 1f, corpseDuration: 2f, sinkDuration: 1f);

        // DayWanderersConfig varsayılanları: bekleme 1–3 sn (rastgele 0.5 → 2 sn), ayak en fazla 12 sn.
        private static readonly WanderSettings Wander = new WanderSettings(Vector3.zero, 20f, 38f, 4f, 10f,
            minPause: 1f, maxPause: 3f, maxLegDuration: 12f, arrivalTolerance: 0.5f, navMeshSampleDistance: 3f);

        private const float Pause = 2f;

        private sealed class FakeBody : IEnemyBody
        {
            public bool HasTarget { get; set; }

            public float DistanceToTarget { get; set; } = float.PositiveInfinity;

            public bool IsAttackBusy { get; set; }

            public float Emergence { get; private set; } = -1f;

            public float Sink { get; private set; }

            public int MoveCalls { get; private set; }

            public int HaltCalls { get; private set; }

            public int FaceCalls { get; private set; }

            public int Attacks { get; private set; }

            public int Despawns { get; private set; }

            public void SetEmergence(float progress) => Emergence = progress;

            public void MoveToTarget() => MoveCalls++;

            public void Halt() => HaltCalls++;

            public void FaceTarget(float deltaTime) => FaceCalls++;

            public void Attack() => Attacks++;

            public void SetSink(float progress) => Sink = progress;

            public void Despawn() => Despawns++;

            public bool LegSucceeds { get; set; } = true;

            public bool Arrived { get; set; }

            public int Legs { get; private set; }

            public float ArrivalTolerance { get; private set; } = -1f;

            public bool StartWanderLeg(in WanderSettings settings)
            {
                Legs++;
                return LegSucceeds;
            }

            public bool HasArrived(float tolerance)
            {
                ArrivalTolerance = tolerance;
                return Arrived;
            }
        }

        private FakeBody _body;
        private EnemyStateMachine _machine;
        private List<EnemyStateId> _changes;

        [SetUp]
        public void SetUp()
        {
            _body = new FakeBody();
            _machine = new EnemyStateMachine(_body, () => 0.5f);
            _changes = new List<EnemyStateId>();
            _machine.StateChanged += _changes.Add;
        }

        [Test]
        public void Begin_StartsBuriedInSpawning()
        {
            Assert.IsFalse(_machine.HasBegun);

            _machine.Begin(Walker);

            Assert.AreEqual(EnemyStateId.Spawning, _machine.CurrentId);
            Assert.AreEqual(0f, _body.Emergence);
            Assert.AreEqual(1, _body.HaltCalls);
            CollectionAssert.AreEqual(new[] { EnemyStateId.Spawning }, _changes);
        }

        [Test]
        public void Spawning_RisesOverDuration_ThenChases()
        {
            _body.HasTarget = true;
            _machine.Begin(Walker);

            _machine.Tick(0.3f);
            Assert.AreEqual(EnemyStateId.Spawning, _machine.CurrentId);
            Assert.AreEqual(0.6f, _body.Emergence, Tolerance);
            Assert.AreEqual(0, _body.MoveCalls, "Yerden çıkarken yürümez.");

            _machine.Tick(0.2f);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId);
            Assert.AreEqual(1f, _body.Emergence);
        }

        [Test]
        public void Chasing_MovesWhenTargetFar_HaltsWithoutTarget()
        {
            StartChasing();
            _body.HasTarget = true;
            _body.DistanceToTarget = 10f;

            _machine.Tick(0.1f);
            Assert.AreEqual(1, _body.MoveCalls);

            int halts = _body.HaltCalls;
            _body.HasTarget = false;
            _machine.Tick(0.1f);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId);
            Assert.AreEqual(1, _body.MoveCalls);
            Assert.AreEqual(halts + 1, _body.HaltCalls);
        }

        [Test]
        public void Chasing_TargetInRange_AttacksImmediately()
        {
            StartChasing();
            _body.HasTarget = true;
            _body.DistanceToTarget = 1.5f;

            _machine.Tick(0.1f);
            Assert.AreEqual(EnemyStateId.Attacking, _machine.CurrentId, "Menzil sınırı dahil.");

            _machine.Tick(0.01f);
            Assert.AreEqual(1, _body.Attacks, "İlk vuruş menzile girer girmez.");
            Assert.AreEqual(1, _body.FaceCalls);
        }

        [Test]
        public void Attacking_FollowsAttackRate()
        {
            StartAttacking();
            Assert.AreEqual(1, _body.Attacks);

            _machine.Tick(0.5f);
            _machine.Tick(0.49f);
            Assert.AreEqual(1, _body.Attacks);

            _machine.Tick(0.01f);
            Assert.AreEqual(2, _body.Attacks);
        }

        [Test]
        public void Attacking_DisengagesOnlyBeyondMargin()
        {
            StartAttacking();

            _body.DistanceToTarget = 1.75f;
            _machine.Tick(0.1f);
            Assert.AreEqual(EnemyStateId.Attacking, _machine.CurrentId, "Menzil + pay içinde saldırmaya devam eder.");

            _body.DistanceToTarget = 1.85f;
            _machine.Tick(0.1f);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId);
        }

        [Test]
        public void Attacking_WhileAttackBusy_StaysAndDoesNotAttackAgain()
        {
            // 12.2 Brute: yere vurma hazırlığı (1 sn) sürerken hedef menzilden çıksa ya da kaybolsa da yerinde kalır.
            StartAttacking();
            Assert.AreEqual(1, _body.Attacks);
            _body.IsAttackBusy = true;
            int moves = _body.MoveCalls;

            _body.DistanceToTarget = 10f;
            _machine.Tick(0.5f);
            _body.HasTarget = false;
            _machine.Tick(0.6f);

            Assert.AreEqual(EnemyStateId.Attacking, _machine.CurrentId);
            Assert.AreEqual(1, _body.Attacks, "Meşgulken yeni saldırı başlamaz (bekleme dolsa da).");
            Assert.AreEqual(moves, _body.MoveCalls, "Yürümez.");

            _body.IsAttackBusy = false;
            _machine.Tick(0.01f);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId, "Bitince normal kurallar.");
        }

        [Test]
        public void Attacking_LostTarget_ReturnsToChasing()
        {
            StartAttacking();

            _body.HasTarget = false;
            _machine.Tick(0.1f);

            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId);
        }

        [Test]
        public void Cooldown_SurvivesLeavingAndReenteringRange()
        {
            StartAttacking();
            Assert.AreEqual(1, _body.Attacks);

            _body.DistanceToTarget = 5f;
            _machine.Tick(0.1f);
            _body.DistanceToTarget = 1f;
            _machine.Tick(0.1f);
            _machine.Tick(0.1f);
            Assert.AreEqual(EnemyStateId.Attacking, _machine.CurrentId);
            Assert.AreEqual(1, _body.Attacks, "Menzilden çıkıp girmek saldırıyı hızlandırmaz.");
            Assert.AreEqual(0.7f, _machine.AttackCooldown, Tolerance);

            _machine.Tick(0.71f);
            Assert.AreEqual(2, _body.Attacks);
        }

        [Test]
        public void Kill_StopsAttacking_ThenSinksAndDespawnsOnce()
        {
            StartAttacking();
            int attacks = _body.Attacks;

            _machine.Kill();
            Assert.IsTrue(_machine.IsDead);

            _machine.Tick(1.99f);
            Assert.AreEqual(0f, _body.Sink, "Ceset 2 sn bekler.");
            Assert.AreEqual(attacks, _body.Attacks);

            _machine.Tick(0.01f);
            _machine.Tick(0.5f);
            Assert.AreEqual(0.5f, _body.Sink, Tolerance);
            Assert.AreEqual(0, _body.Despawns);

            _machine.Tick(0.5f);
            Assert.AreEqual(1f, _body.Sink);
            Assert.AreEqual(1, _body.Despawns);

            _machine.Tick(5f);
            Assert.AreEqual(1, _body.Despawns, "Havuza bir kez döner.");
        }

        [Test]
        public void Kill_Twice_ChangesStateOnce()
        {
            _machine.Begin(Walker);
            _machine.Kill();
            _machine.Kill();

            CollectionAssert.AreEqual(new[] { EnemyStateId.Spawning, EnemyStateId.Dead }, _changes);
        }

        [Test]
        public void Kill_DuringSpawning_StopsRising()
        {
            _machine.Begin(Walker);
            _machine.Tick(0.2f);

            _machine.Kill();
            _machine.Tick(0.2f);

            Assert.AreEqual(0.4f, _body.Emergence, Tolerance);
            Assert.AreEqual(EnemyStateId.Dead, _machine.CurrentId);
        }

        [Test]
        public void Begin_AfterDeath_RestartsFresh()
        {
            StartAttacking();
            _machine.Kill();
            _machine.Tick(2.5f);

            _machine.Begin(Walker);

            Assert.AreEqual(EnemyStateId.Spawning, _machine.CurrentId);
            Assert.AreEqual(0f, _machine.AttackCooldown);
            Assert.AreEqual(0f, _body.Emergence);

            _machine.Tick(5f);
            Assert.AreEqual(0, _body.Despawns, "Önceki ölümün zamanlayıcıları sıfırlandı.");
        }

        // --- Gezgin modu (GDD §9) ---

        [Test]
        public void Wanderer_WithoutTarget_StartsWanderingAfterSpawning()
        {
            _machine.Begin(Walker);
            _machine.StartWandering(Wander);
            _machine.Tick(Walker.SpawnDuration);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId);

            _machine.Tick(0.01f);

            Assert.AreEqual(EnemyStateId.Wandering, _machine.CurrentId);
            Assert.AreEqual(1, _body.Legs, "Girişte ilk ayağa başlar.");
            Assert.AreEqual(0, _body.MoveCalls, "Hedefe yürümez.");
        }

        [Test]
        public void Wandering_Arrived_PausesThenStartsNextLeg()
        {
            StartWandering();
            int halts = _body.HaltCalls;
            _body.Arrived = true;

            _machine.Tick(0.01f);
            Assert.AreEqual(halts + 1, _body.HaltCalls, "Varınca durur.");
            Assert.AreEqual(0.5f, _body.ArrivalTolerance, Tolerance);

            _machine.Tick(Pause - 0.1f);
            Assert.AreEqual(1, _body.Legs, "Bekleme sürüyor.");

            _machine.Tick(0.2f);
            Assert.AreEqual(2, _body.Legs);
            Assert.AreEqual(EnemyStateId.Wandering, _machine.CurrentId);
        }

        [Test]
        public void Wandering_LegTimesOut_PausesThenPicksNewPoint()
        {
            StartWandering();

            _machine.Tick(Wander.MaxLegDuration - 0.1f);
            Assert.AreEqual(1, _body.Legs);

            _machine.Tick(0.2f);
            _machine.Tick(Pause + 0.01f);
            Assert.AreEqual(2, _body.Legs, "Takılan gezgin süre dolunca yeni nokta seçer.");
        }

        [Test]
        public void Wandering_LegFails_RetriesAfterPause()
        {
            _body.LegSucceeds = false;
            StartWandering();
            Assert.AreEqual(1, _body.Legs);

            _machine.Tick(Pause + 0.01f);

            Assert.AreEqual(2, _body.Legs);
            Assert.AreEqual(EnemyStateId.Wandering, _machine.CurrentId);
        }

        [Test]
        public void Wandering_TargetFound_Chases_ThenWandersAgainWhenLost()
        {
            StartWandering();
            _body.HasTarget = true;
            _body.DistanceToTarget = 5f;

            _machine.Tick(0.01f);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId, "Aggro yarıçapındaki oyuncuya saldırır.");
            _machine.Tick(0.01f);
            Assert.AreEqual(1, _body.MoveCalls);

            _body.HasTarget = false;
            _machine.Tick(0.01f);
            Assert.AreEqual(EnemyStateId.Wandering, _machine.CurrentId, "Oyuncu kaçınca yeniden dolaşır.");
        }

        [Test]
        public void StopWandering_ReturnsToChasing_AndWaitsForTarget()
        {
            StartWandering();

            _machine.StopWandering();
            _machine.Tick(0.01f);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId);

            int halts = _body.HaltCalls;
            _machine.Tick(0.01f);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId, "Normal düşman hedefsizken bekler.");
            Assert.AreEqual(halts + 1, _body.HaltCalls);
        }

        [Test]
        public void Begin_TurnsWandererOff()
        {
            StartWandering();

            _machine.Begin(Walker);

            Assert.IsFalse(_machine.IsWanderer, "Havuzdan çıkan düşman normal başlar.");
        }

        private void StartWandering()
        {
            StartChasing();
            _machine.StartWandering(Wander);
            _machine.Tick(0.01f);
            Assert.AreEqual(EnemyStateId.Wandering, _machine.CurrentId);
            Assert.AreEqual(1, _body.Legs);
        }

        private void StartChasing()
        {
            _machine.Begin(Walker);
            _machine.Tick(Walker.SpawnDuration);
            Assert.AreEqual(EnemyStateId.Chasing, _machine.CurrentId);
        }

        private void StartAttacking()
        {
            StartChasing();
            _body.HasTarget = true;
            _body.DistanceToTarget = 1f;
            _machine.Tick(0.01f);
            _machine.Tick(0.01f);
            Assert.AreEqual(EnemyStateId.Attacking, _machine.CurrentId);
        }
    }
}
