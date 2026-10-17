namespace LastHaven.Enemies
{
    /// <summary>
    /// Hedefe yürür; hedef menzile girince saldırıya geçer. Hedef yoksa yerinde bekler; gezgin modundaysa dolaşmaya geçer.
    /// </summary>
    internal sealed class ChasingState : EnemyState
    {
        public ChasingState(EnemyStateMachine machine) : base(machine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Chasing;

        public override void Tick(float deltaTime)
        {
            if (!Body.HasTarget)
            {
                if (Machine.IsWanderer)
                {
                    Machine.ChangeState(EnemyStateId.Wandering);
                    return;
                }

                Body.Halt();
                return;
            }

            if (Body.DistanceToTarget <= Settings.AttackRange)
            {
                Machine.ChangeState(EnemyStateId.Attacking);
                return;
            }

            Body.MoveToTarget();
        }
    }
}
