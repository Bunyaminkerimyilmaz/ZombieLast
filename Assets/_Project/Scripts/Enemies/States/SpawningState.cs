using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary>Yerden çıkma: <see cref="EnemyBehaviourSettings.SpawnDuration"/> boyunca yükselir, sonra kovalamaya geçer.</summary>
    internal sealed class SpawningState : EnemyState
    {
        private readonly Countdown _timer = new Countdown();

        public SpawningState(EnemyStateMachine machine) : base(machine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Spawning;

        public override void Enter()
        {
            _timer.Start(Settings.SpawnDuration);
            Body.Halt();
            Body.SetEmergence(0f);
        }

        public override void Tick(float deltaTime)
        {
            bool done = _timer.Tick(deltaTime);
            Body.SetEmergence(done ? 1f : _timer.Progress);
            if (done)
            {
                Machine.ChangeState(EnemyStateId.Chasing);
            }
        }

        public override void Exit()
        {
            // Yerden çıkarken ölürse batma, gövdenin o anki derinliğinden başlar.
            _timer.Stop();
        }
    }
}
