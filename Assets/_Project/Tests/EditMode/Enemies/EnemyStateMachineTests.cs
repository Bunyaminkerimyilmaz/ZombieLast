using System.Collections.Generic;
using LastHaven.Enemies;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>EnemyStateMachine: Spawning → Chasing ⇄ Attacking → Dead geçişleri, saldırı hızı ve ceset süreleri.</summary>
    public class EnemyStateMachineTests
    {
        private const float Tolerance = 0.0001f;

        // Yürüyen (GDD §7): menzil 1.5 m, 1 saldırı/sn. Süreler EnemyData varsayılanları.
        private static readonly EnemyBehaviourSettings Walker = new EnemyBehaviourSettings(
            spawnDuration: 0.5f, attackRange: 1.5f, disengageMargin: 0.3f, attackInterval: 1f, corpseDuration: 2f, sinkDuration: 1f);

        private sealed class FakeBody : IEnemyBody
        {
            public bool HasTarget { get; set; }

            public float DistanceToTarget { get; set; } = float.PositiveInfinity;

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
        }

        private FakeBody _body;
        private EnemyStateMachine _machine;
        private List<EnemyStateId> _changes;

        [SetUp]
        public void SetUp()
        {
            _body = new FakeBody();
            _machine = new EnemyStateMachine(_body);
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
