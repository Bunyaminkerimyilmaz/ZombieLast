namespace LastHaven.Enemies
{
    /// <summary>
    /// Durur, hedefe döner ve saldırı hızına göre vurur. Hedef kaybolursa veya menzil + pay dışına çıkarsa kovalamaya döner.
    /// </summary>
    internal sealed class AttackingState : EnemyState
    {
        public AttackingState(EnemyStateMachine machine) : base(machine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Attacking;

        public override void Enter()
        {
            Body.Halt();
        }

        public override void Tick(float deltaTime)
        {
            if (!Body.HasTarget || Body.DistanceToTarget > Settings.AttackRange + Settings.DisengageMargin)
            {
                Machine.ChangeState(EnemyStateId.Chasing);
                return;
            }

            Body.FaceTarget(deltaTime);
            if (Machine.TryConsumeAttack())
            {
                Body.Attack();
            }
        }
    }
}
