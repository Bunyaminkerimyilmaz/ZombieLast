using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Gezgin modu (GDD §9): alanda dolaşır. Bir ayak yürür (<see cref="IEnemyBody.StartWanderLeg"/>), varınca veya
    /// <see cref="WanderSettings.MaxLegDuration"/> dolunca durup rastgele süre bekler, sonra yeni ayağa başlar. Ayak başlatılamazsa
    /// bekleyip yeniden dener. Hedef bulunursa (aggro yarıçapındaki oyuncu) veya gezgin modu kapanırsa (gece) kovalamaya geçer.
    /// </summary>
    internal sealed class WanderingState : EnemyState
    {
        private readonly Countdown _pause = new Countdown();
        private float _legTime;
        private bool _walking;

        public WanderingState(EnemyStateMachine machine) : base(machine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Wandering;

        public override void Enter()
        {
            _pause.Stop();
            StartLeg();
        }

        public override void Tick(float deltaTime)
        {
            if (!Machine.IsWanderer || Body.HasTarget)
            {
                Machine.ChangeState(EnemyStateId.Chasing);
                return;
            }

            if (!_walking)
            {
                if (_pause.Tick(deltaTime))
                {
                    StartLeg();
                }

                return;
            }

            _legTime += deltaTime;
            if (Body.HasArrived(Machine.Wander.ArrivalTolerance) || _legTime >= Machine.Wander.MaxLegDuration)
            {
                Rest();
            }
        }

        public override void Exit()
        {
            _pause.Stop();
            _walking = false;
        }

        private void StartLeg()
        {
            _legTime = 0f;
            _walking = Body.StartWanderLeg(Machine.Wander);
            if (!_walking)
            {
                Rest();
            }
        }

        private void Rest()
        {
            _walking = false;
            Body.Halt();
            _pause.Start(Machine.Wander.PickPause(Machine.NextRandom()));
        }
    }
}
