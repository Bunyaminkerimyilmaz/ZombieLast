using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Ölüm: ceset <see cref="EnemyBehaviourSettings.CorpseDuration"/> bekler, <see cref="EnemyBehaviourSettings.SinkDuration"/>
    /// boyunca yere batar, sonra havuza döner. Ajanın kapanması ve ölüm olayı <see cref="Enemy"/>'de, geçişten önce yapılır.
    /// </summary>
    internal sealed class DeadState : EnemyState
    {
        private readonly Countdown _corpse = new Countdown();
        private readonly Countdown _sink = new Countdown();

        public DeadState(EnemyStateMachine machine) : base(machine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Dead;

        public override void Enter()
        {
            _sink.Stop();
            _corpse.Start(Settings.CorpseDuration);
        }

        public override void Tick(float deltaTime)
        {
            if (_corpse.IsRunning)
            {
                if (_corpse.Tick(deltaTime))
                {
                    _sink.Start(Settings.SinkDuration);
                }

                return;
            }

            if (!_sink.IsRunning)
            {
                return;
            }

            bool done = _sink.Tick(deltaTime);
            Body.SetSink(done ? 1f : _sink.Progress);
            if (done)
            {
                Body.Despawn();
            }
        }

        public override void Exit()
        {
            _corpse.Stop();
            _sink.Stop();
        }
    }
}
